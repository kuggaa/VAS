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
using System.Linq;
using System.Threading.Tasks;
using VAS.Core;
using VAS.Core.Events;
using VAS.Core.MVVMC;
using VAS.Core.Store;

namespace VAS.Services.ViewModel
{
	public class ProjectsManagerVM<TModel, TViewModel> : CollectionViewModel<TModel, TViewModel>
		where TModel : Project
		where TViewModel : ProjectVM<TModel>, new()
	{
		public ProjectsManagerVM ()
		{
			LoadedProject = new TViewModel ();
			ExportCommand = new Command<string> (Export, (arg) => LoadedProject.Model != null);
			ImportCommand = new Command (Import);
			DeleteCommand = new Command (Delete, () => LoadedProject.Model != null);
			NewCommand = new Command (New);
			SaveCommand = new Command<bool> (Save, (arg) => {
				return LoadedProject.Model != null && LoadedProject.IsChanged;
			});
			OpenCommand = new Command<TViewModel> (Open, (arg) => LoadedProject.Model != null);
			ResyncCommand = new Command (Resync, () => LoadedProject.Model != null && LoadedProject.Model.FileSet.Count () > 1);
		}

		[PropertyChanged.DoNotNotify]
		public TViewModel LoadedProject {
			get;
			private set;
		}

		[PropertyChanged.DoNotNotify]
		public Command ExportCommand {
			get;
			protected set;
		}

		[PropertyChanged.DoNotNotify]
		public ICommand ImportCommand {
			get;
			protected set;
		}

		[PropertyChanged.DoNotNotify]
		public Command SaveCommand {
			get;
			protected set;
		}

		[PropertyChanged.DoNotNotify]
		public Command NewCommand {
			get;
			protected set;
		}

		[PropertyChanged.DoNotNotify]
		public Command DeleteCommand {
			get;
			protected set;
		}

		[PropertyChanged.DoNotNotify]
		public Command OpenCommand {
			get;
			protected set;
		}

		[PropertyChanged.DoNotNotify]
		public Command ResyncCommand {
			get;
			protected set;
		}

		virtual protected bool IsProjectLoaded ()
		{
			return IsProjectLoaded (null);
		}

		virtual protected bool IsProjectLoaded (object o)
		{
			return LoadedProject != null;
		}

		/// <summary>
		/// Command to export the currently loaded project.
		/// </summary>
		public virtual Task<bool> Export (string format = null)
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
		public virtual Task<bool> Import ()
		{
			return App.Current.EventsBroker.PublishWithReturn (new ImportEvent<TModel> ());
		}

		/// <summary>
		/// Command to create a new project.
		/// </summary>
		public virtual Task<bool> New ()
		{
			return App.Current.EventsBroker.PublishWithReturn (new CreateEvent<TModel> { });
		}

		/// <summary>
		/// Command to delete the selected projects.
		/// </summary>
		public virtual async Task<bool> Delete ()
		{
			bool ret = true;
			foreach (TModel project in Selection.Select (vm => vm.Model).ToList ()) {
				ret &= await App.Current.EventsBroker.PublishWithReturn (new DeleteEvent<TModel> { Object = project });
			}
			return ret;
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
		public Task<bool> Open (TViewModel viewModel)
		{
			return App.Current.EventsBroker.PublishWithReturn (new OpenEvent<TModel> { Object = viewModel.Model });
		}

		virtual protected void Resync ()
		{
			TModel project = Selection.FirstOrDefault ()?.Model;
			if (project != null) {
				App.Current.EventsBroker.Publish (new ResyncProjectEvent { Project = project });
			}
		}
	}
}

