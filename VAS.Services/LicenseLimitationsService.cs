//
//  Copyright (C) 2017 Fluendo S.A.
//
//  This program is free software; you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation; either version 2 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program; if not, write to the Free Software
//  Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
//
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Interfaces;
using VAS.Core.License;
using VAS.Core.MVVMC;
using VAS.Core.ViewModel;
using VAS.Core.ViewModel.Statistics;
using VAS.Drawing.CanvasObjects.Blackboard;
using VAS.Services.State;

namespace VAS.Services
{
	/// <summary>
	/// License limitations service.
	/// </summary>
	public abstract class LicenseLimitationsService : IService, ILicenseLimitationsService
	{
		protected Dictionary<string, LimitationVM> Limitations;

		public LicenseLimitationsService ()
		{
			Limitations = new Dictionary<string, LimitationVM> ();
		}

		/// <summary>
		/// Gets the level of the service. Services are started in ascending level order and stopped in descending level order.
		/// </summary>
		/// <value>The level.</value>
		public int Level {
			get {
				return 30;
			}
		}

		/// <summary>
		/// Gets the name of the service
		/// </summary>
		/// <value>The name of the service.</value>
		public string Name {
			get {
				return "License limitations";
			}
		}

		/// <summary>
		/// Start the service.
		/// </summary>
		public virtual bool Start ()
		{
			UpdateCountLimitations ();
			UpdateFeatureLimitations ();
			App.Current.EventsBroker.Subscribe<LicenseChangeEvent> (HandleLicenseChangeEvent);
			return true;
		}

		/// <summary>
		/// Stop the service.
		/// </summary>
		public virtual bool Stop ()
		{
			App.Current.EventsBroker.Unsubscribe<LicenseChangeEvent> (HandleLicenseChangeEvent);
			return true;
		}

		/// <summary>
		/// Get the specified Limitation by name and type
		/// </summary>
		/// <param name="name">Limitation Name</param>
		/// <typeparam name="T">The Limitation Type</typeparam>
		public T Get<T> (string name) where T : LimitationVM
		{
			LimitationVM ret;
			Limitations.TryGetValue (name, out ret);
			return ret as T;
		}

		/// <summary>
		/// Gets all the limitations.
		/// </summary>
		/// <returns>A collection with all the limitations.</returns>
		public IEnumerable<LimitationVM> GetAll ()
		{
			return Limitations.Values;
		}

		///// <summary>
		///// Gets all the limitations of type T.
		///// </summary>
		///// <returns>A collection with all the limitations of type T</returns>
		public IEnumerable<T> GetAll<T> () where T : LimitationVM
		{
			return Limitations.Values.OfType<T> ();
		}

		/// <summary>
		/// Add the specified limitation by name.
		/// </summary>
		/// <param name="limitation">Limitation.</param>
		public void Add (CountLicenseLimitation limitation, Command command = null)
		{
			if (Limitations.ContainsKey (limitation.RegisterName)) {
				throw new InvalidOperationException ("Limitations cannot be overwritten");
			}
			CountLimitationVM viewModel = new CountLimitationVM {
				Model = limitation,
			};
			if (command != null) {
				viewModel.UpgradeCommand = command;
			}
			Limitations [limitation.RegisterName] = viewModel;
		}

		/// <summary>
		/// Add the specified feature limitation and command.
		/// </summary>
		/// <param name="limitation">Limitation.</param>
		/// <param name="command">Limitation Command.</param>
		public void Add (FeatureLicenseLimitation limitation)
		{
			if (Limitations.ContainsKey (limitation.RegisterName)) {
				throw new InvalidOperationException ("Limitations cannot be overwritten");
			}

			FeatureLimitationVM viewModel = new FeatureLimitationVM {
				Model = limitation
			};
			Limitations [limitation.RegisterName] = viewModel;
		}

		/// <summary>
		/// Checks if a limitation can be executed
		/// </summary>
		/// <param name="name">Name of the limitation</param>
		public bool CanExecute (string name)
		{
			var limitationVM = Get<LimitationVM> (name);
			if (limitationVM == null) {
				Log.Warning ("Cannot get limitation because it wasn't registered." +
													 " Returning true");
				return true;
			}
			if (!limitationVM.Enabled) {
				return true;
			}
			var countVM = limitationVM as CountLimitationVM;
			if (countVM != null) {
				return countVM.Count < countVM.Maximum;
			}
			return !limitationVM.Enabled;
		}

		/// <summary>
		/// Moves to the upgrade dialog
		/// </summary>
		/// <returns>The Task of the transition </returns>
		/// <param name="name">Name of the limitation</param>
		public Task<bool> MoveToUpgradeDialog (string name)
		{
			var limitationVM = Get<LimitationVM> (name);

			if (limitationVM == null) {
				Log.Warning ("Cannot get Feature, because it wasn't registered," +
													 " Do not move to UpgradeDialog state");
				return AsyncHelpers.Return (false);
			}
			if (limitationVM.Enabled) {
				dynamic properties = new ExpandoObject ();
				properties.limitationVM = limitationVM;
				return App.Current.StateController.MoveToModal (UpgradeLimitationState.NAME, properties);
			} else {
				return AsyncHelpers.Return (false);
			}
		}

		protected void UpdateCountLimitations ()
		{
			bool enable = App.Current.LicenseManager.LicenseStatus.Limited;
			foreach (var limitation in GetAll<CountLimitationVM> ().Select ((arg) => arg.Model)) {
				limitation.Enabled = enable;
			}
		}

		protected abstract void UpdateFeatureLimitations ();

		void HandleLicenseChangeEvent (LicenseChangeEvent e)
		{
			UpdateCountLimitations ();
			UpdateFeatureLimitations ();
		}

		public CountLimitationBarChartVM CreateBarChartVM (string limitationName)
		{
			var limitation = Get<CountLimitationVM> (limitationName);
			TwoBarChartVM barChart = new TwoBarChartVM (limitation.Maximum,
														new SeriesVM { Title = "Remaining", Elements = limitation.Remaining, Color = Color.Green1 },
														new SeriesVM { Title = "Current", Elements = limitation.Count, Color = Color.Transparent });
			barChart.Height = 10;
			barChart.Background = new ImageCanvasObject {
				Image = App.Current.ResourcesLocator.LoadImage ("images/lm-widget-full-bar" + Constants.IMAGE_EXT),
				Mode = ScaleMode.Fill
			};

			var result = new CountLimitationBarChartVM { Limitation = limitation, BarChart = barChart };
			result.Bind ();
			return result;
		}
	}
}
