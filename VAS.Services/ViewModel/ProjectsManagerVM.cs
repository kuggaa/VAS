//
//  Copyright (C) 2016 Fluendo S.A.
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
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using VAS.Core;
using VAS.Core.Events;
using VAS.Core.License;
using VAS.Core.MVVMC;
using VAS.Core.Store;
using VAS.Core.ViewModel;

namespace VAS.Services.ViewModel
{
	public class ProjectsManagerVM<TModel, TViewModel> : LimitedCollectionViewModel<TModel, TViewModel>
		where TModel : Project
		where TViewModel : ProjectVM<TModel>, new()
	{
		TViewModel loadedProject;

		public ProjectsManagerVM ()
		{
			LoadedProject = new TViewModel ();
			NewCommand = new Command (New);
			OpenCommand = new AsyncCommand<TViewModel> (Open, (arg) => Selection.Count == 1);
			DeleteSelectionCommand = new AsyncCommand (DeleteSelection, () => Selection.Any ());
			SaveCommand = new AsyncCommand (Save, () => LoadedProject?.Model != null && LoadedProject.IsChanged);
			ExportCommand = new AsyncCommand (Export, () => Selection.Count == 1);
			DeleteCommand = new Command<TViewModel> (Delete, () => true) { IconName = "lma-trash" };
		}

		protected override void DisposeManagedResources ()
		{
			base.DisposeManagedResources ();
			LoadedProject = null;
		}

		public override CountLimitationVM Limitation {
			set {
				if (Limitation != null) {
					Limitation.PropertyChanged -= HandleLimitationChanged;
				}
				base.Limitation = value;
				if (Limitation != null) {
					Limitation.PropertyChanged += HandleLimitationChanged;
					CheckNewCommandEnabled ();
				}
			}
		}

		public TViewModel LoadedProject {
			get {
				return loadedProject;
			}
			set {
				if (loadedProject != null) {
					loadedProject.PropertyChanged -= HandleLoadedProjectChanged;
				}
				loadedProject = value;
				if (loadedProject != null) {
					loadedProject.PropertyChanged += HandleLoadedProjectChanged;
					loadedProject.Sync ();
				}
			}
		}

		[PropertyChanged.DoNotNotify]
		public Command NewCommand {
			get;
			protected set;
		}

		[PropertyChanged.DoNotNotify]
		public Command OpenCommand {
			get;
			protected set;
		}

		[PropertyChanged.DoNotNotify]
		public Command DeleteSelectionCommand {
			get;
			protected set;
		}

		[PropertyChanged.DoNotNotify]
		public Command DeleteCommand {
			get;
			protected set;
		}

		[PropertyChanged.DoNotNotify]
		public Command SaveCommand {
			get;
			protected set;
		}

		[PropertyChanged.DoNotNotify]
		public Command ExportCommand {
			get;
			protected set;
		}

		/// <summary>
		/// Command to export the currently loaded project.
		/// </summary>
		public async Task<bool> Export ()
		{
			return await Export (null);
		}

		public async Task<bool> Export (string format)
		{
			TModel template = Selection.FirstOrDefault ()?.Model;
			if (template != null) {
				return await App.Current.EventsBroker.PublishWithReturn (new ExportEvent<TModel> { Object = template, Format = format });
			}
			return false;
		}

		/// <summary>
		/// Command to import a project.
		/// </summary>
		public Task<bool> Import ()
		{
			return App.Current.EventsBroker.PublishWithReturn (new ImportEvent<TModel> ());
		}

		/// <summary>
		/// Command to create a new project.
		/// </summary>
		protected virtual void New ()
		{
			App.Current.EventsBroker.Publish (new CreateEvent<TModel> { });
		}

		/// <summary>
		/// Command to delete the selected projects.
		/// </summary>
		protected virtual async Task DeleteSelection ()
		{
			foreach (TModel project in Selection.Select (vm => vm.Model).ToList ()) {
				await App.Current.EventsBroker.Publish (new DeleteEvent<TModel> { Object = project });
			}
		}

		/// <summary>
		/// Command to delete the selected projects.
		/// </summary>
		protected virtual void Delete (TViewModel vm)
		{
			App.Current.EventsBroker.Publish (new DeleteEvent<TModel> { Object = vm.Model });
		}

		/// <summary>
		/// Command to save the currently loaded project.
		/// </summary>
		/// <param name="force">If set to <c>true</c> does not prompt to save.</param>
		protected virtual async Task<bool> Save ()
		{
			bool force = true;
			return await Save (force);
		}

		protected virtual async Task<bool> Save (bool force = true)
		{
			if (LoadedProject != null && LoadedProject.Model != null) {
				return await App.Current.EventsBroker.PublishWithReturn (new UpdateEvent<TViewModel> { Object = LoadedProject, Force = force });
			}
			return false;
		}

		/// <summary>
		/// Command to Open a project
		/// </summary>
		protected virtual async Task Open (TViewModel viewModel)
		{
			await App.Current.EventsBroker.Publish (new OpenEvent<TModel> { Object = viewModel?.Model });
		}

		void HandleLimitationChanged (object sender, PropertyChangedEventArgs e)
		{
			CheckNewCommandEnabled ();
		}

		void CheckNewCommandEnabled ()
		{
			NewCommand.Executable = Limitation == null || Limitation.Count < Limitation.Maximum;
		}

		void HandleLoadedProjectChanged (object sender, PropertyChangedEventArgs e)
		{
			ExportCommand?.EmitCanExecuteChanged ();
			SaveCommand?.EmitCanExecuteChanged ();
			OpenCommand?.EmitCanExecuteChanged ();
			DeleteSelectionCommand?.EmitCanExecuteChanged ();
		}
	}
}

