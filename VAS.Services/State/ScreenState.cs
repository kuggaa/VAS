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
using System.Linq;
using System.Threading.Tasks;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Hotkeys;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;

namespace VAS.Services.State
{
	/// <summary>
	/// Generic base class for <see cref="IScreenState"/> that takes care of the <see cref="IPanel"/>
	/// <see cref="IViewModel"/> and <see cref="IController"/> creationg and wireup.
	/// </summary>
	public abstract class ScreenState<TViewModel> : DisposableBase, IScreenState where TViewModel : IViewModel
	{
		protected List<Task> taskList;

		public ScreenState ()
		{
			ViewModelOwner = true;
			Panel = App.Current.ViewLocator.Retrieve (Name) as IPanel;
			Controllers = App.Current.ControllerLocator.RetrieveAll (Name).ToList ();
			KeyContext = new KeyContext ();
			ToolbarCommands = new List<Command> ();
			taskList = new List<Task> ();
		}

		protected override void DisposeManagedResources ()
		{
			base.DisposeManagedResources ();
			Controllers?.ForEach ((c) => c.Dispose ());
			Panel?.Dispose ();
			if (ViewModelOwner) {
				ViewModel?.Dispose ();
			}
			ViewModel = default (TViewModel);
			Controllers.Clear ();
			Panel = null;
			KeyContext = null;
		}

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="T:VAS.Services.State.ScreenState`1"/> is the
		/// view model owner.
		/// </summary>
		/// <value><c>true</c> if view model owner; otherwise, <c>false</c>.</value>
		public bool ViewModelOwner {
			get;
			set;
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

		/// <summary>
		/// Gets the toolbar commands.
		/// </summary>
		/// <value>The toolbar commands.</value>
		public virtual IEnumerable<Command> ToolbarCommands {
			get;
		}

		public virtual Task<bool> LoadState (dynamic data)
		{
			Log.Debug ($"Loading state {Name}");
			return Initialize (data);
		}

		public virtual Task<bool> UnloadState ()
		{
			Log.Debug ($"Unloading state {Name}");
			foreach (var controller in Controllers) {
				controller.Dispose ();
			}
			Controllers.Clear ();
			return AsyncHelpers.Return (true);
		}

		public virtual Task<bool> HideState ()
		{
			Log.Debug ($"Hiding state {Name}");
			return InternalStopState ();
		}

		public virtual Task<bool> ShowState ()
		{
			Log.Debug ($"Showing state {Name}");
			return InternalStartState ();
		}

		public virtual Task<bool> FreezeState ()
		{
			Log.Debug ($"Freezing state {Name}");
			return InternalStopState ();
		}

		public virtual Task<bool> UnfreezeState ()
		{
			Log.Debug ($"Unfreezing state {Name}");
			return InternalStartState ();
		}

		/// <summary>
		/// Registers the task to the list of tasks that will be awaited together.
		/// </summary>
		/// <param name="newTask">New task.</param>
		protected void RegisterTask (Task newTask)
		{
			taskList.Add (newTask);
		}

		/// <summary>
		/// Waits for all tasks registered.
		/// </summary>
		protected internal async Task WhenAllTasks ()
		{
			await Task.WhenAll (taskList);
			taskList.Clear ();
		}

		protected Task<bool> Initialize (dynamic data)
		{
			CreateViewModel (data);
			CreateControllers (data);
			foreach (IController controller in Controllers) {
				controller.SetViewModel (ViewModel);
			}
			Panel.SetViewModel (ViewModel);

			foreach (IController controller in Controllers) {
				KeyContext.KeyActions.AddRange (controller.GetDefaultKeyActions ());
			}
			KeyContext panelKeyContext = Panel?.GetKeyContext ();
			if (panelKeyContext != null) {
				KeyContext.KeyActions.AddRange (panelKeyContext.KeyActions);
			}

			return AsyncHelpers.Return (true);
		}

		/// <summary>
		/// Creates the view model and fill using if needed the data passed as arguments.
		/// </summary>
		/// <param name="data">Data.</param>
		protected abstract void CreateViewModel (dynamic data);

		/// <summary>
		/// Creates secondary controllers. Controllers that aren't linked directly to the state view model.
		/// This method should create this controllers, set their properties and add them to the Controllers list.
		/// </summary>
		/// <param name="data">Data.</param>
		protected virtual void CreateControllers (dynamic data)
		{
		}

		async Task<bool> InternalStopState ()
		{
			foreach (IController controller in Controllers) {
				await controller.Stop ();
			}
			await WhenAllTasks ();
			App.Current.KeyContextManager.RemoveContext (KeyContext);
			return true;
		}

		async Task<bool> InternalStartState ()
		{
			foreach (IController controller in Controllers) {
				await controller.Start ();
			}
			App.Current.KeyContextManager.AddContext (KeyContext);
			return true;
		}
	}
}

