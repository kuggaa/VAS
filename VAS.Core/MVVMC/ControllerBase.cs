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
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using VAS.Core.Common;
using VAS.Core.Hotkeys;
using VAS.Core.Interfaces.MVVMC;

namespace VAS.Core.MVVMC
{
	/// <summary>
	/// Temporal class to concentrate common things for the new services.
	/// </summary>
	public abstract class ServiceBase : DisposableBase
	{
		public virtual IEnumerable<KeyAction> GetDefaultKeyActions ()
		{
			return Enumerable.Empty<KeyAction> ();
		}

	}

	public class ServiceBase<TViewModel> : ServiceBase
		where TViewModel : class, IViewModel
	{
		TViewModel viewModel;

		public virtual TViewModel ViewModel {
			get {
				return viewModel;
			}
			set {
				viewModel = value;
			}
		}
	}

	public abstract class ControllerBase : DisposableBase, IController
	{
		protected override void DisposeManagedResources ()
		{
			base.DisposeManagedResources ();
			if (Started) {
				Stop ();
			}
		}

		protected override void DisposeUnmanagedResources ()
		{
			base.DisposeUnmanagedResources ();
			if (Started) {
				Log.Error ($"The controller {GetType ()} was not stopped correctly.");
			}
		}

		#region IController

		public bool Started {
			get;
			private set;
		}

		public virtual IEnumerable<KeyAction> GetDefaultKeyActions ()
		{
			return Enumerable.Empty<KeyAction> ();
		}

		public abstract void SetViewModel (IViewModel viewModel);

		/// <summary>
		/// Start this controller.
		/// This should be pre-called by child classes.
		/// </summary>
		public virtual Task Start ()
		{
			Log.Debug ($"Starting controller {GetType ()}");
			if (Started) {
				throw new InvalidOperationException ("The controller is already running");
			}
			Started = true;
			return AsyncHelpers.Return ();
		}

		/// <summary>
		/// Stop this controller.
		/// This should be pre-called by child classes.
		/// </summary>
		public virtual Task Stop ()
		{
			Log.Debug ($"Stopping controller {GetType ()}");
			if (!Started) {
				throw new InvalidOperationException ("The controller is already stopped");
			}
			Started = false;
			return AsyncHelpers.Return ();
		}

		protected virtual void ConnectEvents () { }

		protected virtual void DisconnectEvents () { }

		#endregion
	}

	public class ControllerBase<TViewModel> : ControllerBase
		where TViewModel : class, IViewModel
	{
		TViewModel viewModel;

		public virtual TViewModel ViewModel {
			get {
				return viewModel;
			}
			set {
				if (Started) {
					throw new InvalidOperationException ("The ViewModel can't be changed while the controller is running");
				}
				viewModel = value;
			}
		}

		public override async Task Start ()
		{
			await base.Start ();
			if (ViewModel == null) {
				throw new InvalidOperationException ($"The controller {GetType ()} needs a ViewModel before starting");
			}
			ConnectEvents ();
		}

		public override async Task Stop ()
		{
			await base.Stop ();
			if (ViewModel == null) {
				Log.Error ($"Controller {GetType ()} stopped without a ViewModel. This should never happen");
				throw new InvalidOperationException ($"The controller {GetType ()} needs a ViewModel before stopping");
			}
			DisconnectEvents ();
		}

		public override void SetViewModel (IViewModel viewModel)
		{
			ViewModel = (TViewModel)viewModel;
		}

		protected override void ConnectEvents ()
		{
			ViewModel.PropertyChanged += HandleViewModelChanged;
		}

		protected override void DisconnectEvents ()
		{
			ViewModel.PropertyChanged -= HandleViewModelChanged;
		}

		protected virtual void HandleViewModelChanged (object sender, PropertyChangedEventArgs e)
		{
		}
	}
}
