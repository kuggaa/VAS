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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Filters;
using VAS.Core.MVVMC;
using VAS.Core.Store;
using VAS.Core.ViewModel;
using static VAS.Core.Resources.Strings;

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
			NewCommand = new LimitationCommand (VASCountLimitedObjects.Projects.ToString (), New) { IconName = "vas-plus" };
			OpenCommand = new AsyncCommand<TViewModel> (Open, (arg) => Selection.Count == 1);
			DeleteCommand = new AsyncCommand<TViewModel> (Delete, (arg) => Selection.Any () || arg != null) { IconName = "vas-delete" };
			SaveCommand = new AsyncCommand (Save, () => LoadedProject?.Model != null && LoadedProject.IsChanged);
			ExportCommand = new AsyncCommand (Export, () => Selection.Count == 1);
			SearchCommand = new Command<string> (textFilter => App.Current.EventsBroker.Publish (
				new SearchEvent { TextFilter = textFilter }));
			VisibleViewModels = new VisibleRangeObservableProxy<TViewModel> (ViewModels);
			ProjectMenu = new MenuVM ();
			EmptyCard = new EmptyCardVM {
				HeaderText = ProjectsNoneCreated,
				DescriptionText = ProjectsCreateHelper,
				TipText = ProjectsCreateTip,
			};
			FillProjectMenu ();
			FilterText = string.Empty;
		}

		protected override void DisposeManagedResources ()
		{
			base.DisposeManagedResources ();
			LoadedProject = null;
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
		public Command NewCommand { get; protected set; }

		[PropertyChanged.DoNotNotify]
		public Command OpenCommand { get; protected set; }

		[PropertyChanged.DoNotNotify]
		public Command DeleteCommand { get; protected set; }

		[PropertyChanged.DoNotNotify]
		public Command SaveCommand { get; protected set; }

		public Command ExportCommand { get; protected set; }

		/// <summary>
		/// Publishes SearchEvent<TModel> with text filter as parameter.
		/// </summary>
		/// <value>The search projects command.</value>
		[PropertyChanged.DoNotNotify]
		public Command<string> SearchCommand { get; protected set; }

		/// <summary>
		/// Gets or sets the type of the sort.
		/// </summary>
		/// <value>The type of the sort.</value>
		public ProjectSortType SortType { get; set; }

		/// <summary>
		/// Gets or sets the filter text.
		/// </summary>
		/// <value>The filter text.</value>
		public string FilterText { get; set; }

		/// <summary>
		/// Gets the project menu.
		/// </summary>
		/// <value>The project menu.</value>
		public MenuVM ProjectMenu { get; private set; }

		/// <summary>
		/// Gets or sets the visible view models, viewmodels that has boolean Visible property setted to true.
		/// </summary>
		/// <value>The visible view models.</value>
		public VisibleRangeObservableProxy<TViewModel> VisibleViewModels { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the current search filter returned no results.
		/// </summary>
		/// <value><c>true</c> if no results; otherwise, <c>false</c>.</value>
		public bool NoResults { get; set; }

		/// <summary>
		/// Gets or sets the empty card View Model to be used when the user has no projects created.
		/// </summary>
		/// <value>The empty card.</value>
		public EmptyCardVM EmptyCard { get; set; }

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
		protected virtual async Task Delete (TViewModel viewModel)
		{
			if (viewModel != null) {
				await App.Current.EventsBroker.Publish (new DeleteEvent<TModel> { Object = viewModel.Model });
			} else {
				foreach (TModel project in Selection.Select (vm => vm.Model).ToList ()) {
					await App.Current.EventsBroker.Publish (new DeleteEvent<TModel> { Object = project });
				}
			}
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

		protected virtual void FillProjectMenu ()
		{
			ProjectMenu.ViewModels.AddRange (new List<MenuNodeVM> {
				new MenuNodeVM (DeleteCommand, null, Catalog.GetString("Delete")) { Color = App.Current.Style.ColorAccentError },
			});
		}

		void HandleLoadedProjectChanged (object sender, PropertyChangedEventArgs e)
		{
			ExportCommand?.EmitCanExecuteChanged ();
			SaveCommand?.EmitCanExecuteChanged ();
			OpenCommand?.EmitCanExecuteChanged ();
			DeleteCommand?.EmitCanExecuteChanged ();
		}
	}
}

