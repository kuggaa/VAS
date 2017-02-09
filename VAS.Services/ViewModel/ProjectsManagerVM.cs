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
		public ProjectsManagerVM ()
		{
			LoadedProject = new TViewModel ();
			NewCommand = new Command (New);
			OpenCommand = new Command<TViewModel> (Open, (arg) => Selection.Count == 1);
			DeleteCommand = new Command (Delete, () => Selection.Any ());
		}

		public override LicenseLimitationVM Limitation {
			set {
				if (Limitation != null) {
					Limitation.PropertyChanged -= HandleLimitationChanged;
				}
				base.Limitation = value;
				if (Limitation != null) {
					Limitation.PropertyChanged += HandleLimitationChanged;
				}
			}
		}

		[PropertyChanged.DoNotNotify]
		public TViewModel LoadedProject {
			get;
			private set;
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
		public Command DeleteCommand {
			get;
			protected set;
		}

		/// <summary>
		/// Control whether the save button is clickable or not.
		/// </summary>
		/// <value><c>true</c> if save clickable; otherwise, <c>false</c>.</value>
		public bool SaveSensitive {
			get;
			set;
		}

		/// <summary>
		/// Control whether the delete button is clickable or not.
		/// </summary>
		/// <value><c>true</c> if delete clickable; otherwise, <c>false</c>.</value>
		public bool DeleteSensitive {
			get;
			set;
		}

		/// <summary>
		/// Control whether the export button is clickable or not.
		/// </summary>
		/// <value><c>true</c> if delete clickable; otherwise, <c>false</c>.</value>
		public bool ExportSensitive {
			get;
			set;
		}

		/// <summary>
		/// Command to export the currently loaded project.
		/// </summary>
		public Task<bool> Export (string format = null)
		{
			TModel template = Selection.FirstOrDefault ()?.Model;
			if (template != null) {
				return App.Current.EventsBroker.PublishWithReturn (new ExportEvent<TModel> { Object = template, Format = format });
			}
			return AsyncHelpers.Return (false);
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
		protected virtual void Delete ()
		{
			foreach (TModel project in Selection.Select (vm => vm.Model).ToList ()) {
				App.Current.EventsBroker.Publish (new DeleteEvent<TModel> { Object = project });
			}
		}

		/// <summary>
		/// Command to save the currently loaded project.
		/// </summary>
		/// <param name="force">If set to <c>true</c> does not prompt to save.</param>
		public Task<bool> Save (bool force)
		{
			TModel project = LoadedProject.Model;
			if (project != null) {
				return App.Current.EventsBroker.PublishWithReturn (new UpdateEvent<TModel> { Object = project, Force = force });
			}
			return AsyncHelpers.Return (false);
		}

		/// <summary>
		/// Command to Open a project
		/// </summary>
		protected virtual void Open (TViewModel viewModel)
		{
			App.Current.EventsBroker.Publish (new OpenEvent<TModel> { Object = viewModel?.Model });
		}

		void HandleLimitationChanged (object sender, PropertyChangedEventArgs e)
		{
			NewCommand.Executable = Limitation.Count < Limitation.Maximum;
		}
	}
}

