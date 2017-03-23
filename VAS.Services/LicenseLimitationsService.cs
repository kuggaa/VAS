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
using System.Linq;
using VAS.Core.Events;
using VAS.Core.Interfaces;
using VAS.Core.License;
using VAS.Core.MVVMC;
using VAS.Core.ViewModel;

namespace VAS.Services
{
	/// <summary>
	/// License limitations service.
	/// </summary>
	public abstract class LicenseLimitationsService : IService, ILicenseLimitationsService
	{
		protected Dictionary<string, LicenseLimitationVM> Limitations;

		public LicenseLimitationsService ()
		{
			Limitations = new Dictionary<string, LicenseLimitationVM> ();
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
			UpdateLimitatonVisibility ();
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
		/// Gets the limitation.
		/// </summary>
		/// <returns>The limitation with the specified name, or null.</returns>
		/// <param name="name">Limitation name.</param>
		public LicenseLimitationVM Get (string name)
		{
			LicenseLimitationVM ret;
			Limitations.TryGetValue (name, out ret);
			return ret;
		}

		/// <summary>
		/// Gets all the limitations.
		/// </summary>
		/// <returns>A collection with all the limitations.</returns>
		public IEnumerable<LicenseLimitationVM> GetAll ()
		{
			return Limitations.Values;
		}

		/// <summary>
		/// Add the specified limitation by name.
		/// </summary>
		/// <param name="limitation">Limitation.</param>
		public void Add (LicenseLimitation limitation, Command command = null)
		{
			if (Limitations.ContainsKey (limitation.Name)) {
				throw new InvalidOperationException ("Limitations cannot be overwritten");
			}
			LicenseLimitationVM viewModel = new LicenseLimitationVM {
				Model = limitation,
			};
			if (command != null) {
				viewModel.UpgradeCommand = command;
			}
			Limitations [limitation.Name] = viewModel;
		}

		protected void UpdateLimitatonVisibility ()
		{
			bool enable = App.Current.LicenseManager.LicenseStatus.Limited;
			foreach (var limitation in GetAll ().Select ((arg) => arg.Model)) {
				limitation.Enabled = enable;
			}
		}

		void HandleLicenseChangeEvent (LicenseChangeEvent e)
		{
			UpdateLimitatonVisibility ();
		}
	}
}
