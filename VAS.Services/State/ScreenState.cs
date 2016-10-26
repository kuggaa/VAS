//
//  Copyright (C) 2016 Andoni Morales Alastruey.
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
using System.Threading.Tasks;
using VAS.Core;
using VAS.Core.Hotkeys;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Interfaces.MVVMC;


namespace VAS.Services.State
{
	/// <summary>
	/// Generic base class for <see cref="IScreenState"/> that takes care of the <see cref="IPanel"/>
	/// <see cref="IViewModel"/> and <see cref="IController"/> creationg and wireup.
	/// </summary>
	public abstract class ScreenState<TViewModel> : IScreenState where TViewModel : IViewModel
	{
		public ScreenState ()
		{
			Panel = App.Current.ViewLocator.Retrieve (Name) as IPanel;
			Controllers = App.Current.ControllerLocator.RetrieveAll (Name);
			KeyContext = new KeyContext ();
			foreach (IController controller in Controllers) {
				KeyContext.KeyActions.AddRange (controller.GetDefaultKeyActions ());
			}
			KeyContext panelKeyContext = Panel.GetKeyContext ();
			if (panelKeyContext != null) {
				KeyContext.KeyActions.AddRange (Panel.GetKeyContext ()?.KeyActions);
			}
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		~ScreenState ()
		{
			Dispose (false);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (Disposed)
				return;

			if (disposing) {
				Controllers?.ForEach ((c) => c.Dispose ());
				Panel?.Dispose ();
			}

			Disposed = true;
		}


		public abstract string Name {
			get;
		}

		public List<IController> Controllers {
			get;
			set;
		}

		public IPanel Panel {
			get;
			set;
		}

		public KeyContext KeyContext {
			get;
			set;
		}

		public TViewModel ViewModel {
			get;
			protected set;
		}

		protected bool Disposed { get; private set; } = false;

		public virtual Task<bool> PostTransition ()
		{
			foreach (IController controller in Controllers) {
				controller.Stop ();
			}
			ViewModel = default (TViewModel);
			App.Current.KeyContextManager.RemoveContext (KeyContext);
			return AsyncHelpers.Return (true);
		}

		public virtual Task<bool> PreTransition (dynamic data)
		{
			return Initialize (data);
		}

		protected Task<bool> Initialize (dynamic data)
		{
			CreateViewModel (data);
			Panel.SetViewModel (ViewModel);
			foreach (IController controller in Controllers) {
				controller.SetViewModel (ViewModel);
				controller.Start ();
			}
			App.Current.KeyContextManager.AddContext (KeyContext);
			return AsyncHelpers.Return (true);
		}

		/// <summary>
		/// Creates the view model and fill using if needed the data passed as arguments.
		/// </summary>
		/// <param name="data">Data.</param>
		protected abstract void CreateViewModel (dynamic data);
	}
}

