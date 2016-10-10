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
using System;
using System.Linq;
using System.Windows.Input;
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
			ExportCommand = new Command<string> (Export);
			ImportCommand = new Command (Import);
			NewCommand = new Command (Delete);
			SaveCommand = new Command<bool> (Save);
			OpenCommand = new Command<TViewModel> (Open);
			ResyncCommand = new Command (Resync);
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
		public Command ImportCommand {
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

		/// <summary>
		/// Command to export the currently loaded project.
		/// </summary>
		virtual protected void Export (string format = null)
		{
			TModel template = Selection.FirstOrDefault ()?.Model;
			if (template != null) {
				App.Current.EventsBroker.Publish (new ExportEvent<TModel> { Object = template, Format = format });
			}
		}

		/// <summary>
		/// Command to import a project.
		/// </summary>
		virtual protected void Import ()
		{
			App.Current.EventsBroker.Publish (new ImportEvent<TModel> ());
		}

		/// <summary>
		/// Command to create a new project.
		/// </summary>
		virtual protected void New ()
		{
			App.Current.EventsBroker.Publish (new CreateEvent<TModel> { });
		}

		/// <summary>
		/// Command to delete the selected projects.
		/// </summary>
		virtual protected void Delete ()
		{
			foreach (TModel project in Selection.Select (vm => vm.Model).ToList ()) {
				App.Current.EventsBroker.Publish (new DeleteEvent<TModel> { Object = project });
			}
		}

		/// <summary>
		/// Command to save the currently loaded project.
		/// </summary>
		/// <param name="force">If set to <c>true</c> does not prompt to save.</param>
		virtual protected void Save (bool force)
		{
			TModel project = LoadedProject.Model;
			if (project != null) {
				App.Current.EventsBroker.Publish (new UpdateEvent<TModel> { Object = project, Force = force });
			}
		}

		/// <summary>
		/// Command to Open a project
		/// </summary>
		virtual protected void Open (TViewModel viewModel)
		{
			App.Current.EventsBroker.Publish (new OpenEvent<TModel> { Object = viewModel.Model });
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

