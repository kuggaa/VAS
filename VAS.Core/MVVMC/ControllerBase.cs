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
using VAS.Core.Common;
using VAS.Core.Hotkeys;
using VAS.Core.Interfaces.MVVMC;

namespace VAS.Core.MVVMC
{
	public abstract class ControllerBase : DisposableBase, IController
	{
		protected bool started;

		protected override void DisposeManagedResources ()
		{
			base.DisposeManagedResources ();
			if (started) {
				Stop ();
			}
		}

		protected override void DisposeUnmanagedResources ()
		{
			base.DisposeUnmanagedResources ();
			if (started) {
				Log.Error ($"The controller {GetType ()} was not stopped correctly.");
			}
		}

		#region IController

		public virtual IEnumerable<KeyAction> GetDefaultKeyActions ()
		{
			return Enumerable.Empty<KeyAction> ();
		}

		public abstract void SetViewModel (IViewModel viewModel);

		/// <summary>
		/// Start this controller.
		/// This should be pre-called by child classes.
		/// </summary>
		public virtual void Start ()
		{
			Log.Debug ($"Starting controller {GetType ()}");
			if (started) {
				throw new InvalidOperationException ("The controller is already running");
			}
			started = true;
		}

		/// <summary>
		/// Stop this controller.
		/// This should be pre-called by child classes.
		/// </summary>
		public virtual void Stop ()
		{
			Log.Debug ($"Stopping controller {GetType ()}");
			if (!started) {
				throw new InvalidOperationException ("The controller is already stopped");
			}
			started = false;
		}

		#endregion
	}
}
