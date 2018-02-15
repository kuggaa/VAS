// EventsManagerBase.cs
//
//  Copyright (C2007-2009 Andoni Morales Alastruey
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
//Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
//
//
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VAS.Core.Events;
using VAS.Core.Hotkeys;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
using VAS.Core.ViewModel;

namespace VAS.Services.Controller
{
	public class CoreEventsController : ControllerBase
	{
		/* current project in use */
		protected ProjectVM project;

		public override async Task Start ()
		{
			await base.Start ();

			App.Current.EventsBroker.Subscribe<DashboardEditedEvent> (HandleDashboardEditedEvent);
			App.Current.EventsBroker.Subscribe<TagSubcategoriesChangedEvent> (HandleTagSubcategoriesChangedEvent);
			App.Current.EventsBroker.Subscribe<ShowFullScreenEvent> (HandleShowFullScreenEvent);
		}

		public override async Task Stop ()
		{
			await base.Stop ();

			App.Current.EventsBroker.Unsubscribe<DashboardEditedEvent> (HandleDashboardEditedEvent);
			App.Current.EventsBroker.Unsubscribe<TagSubcategoriesChangedEvent> (HandleTagSubcategoriesChangedEvent);
			App.Current.EventsBroker.Unsubscribe<ShowFullScreenEvent> (HandleShowFullScreenEvent);
		}

		public override void SetViewModel (IViewModel viewModel)
		{
			project = ((IProjectDealer)viewModel).Project;
		}

		public override IEnumerable<KeyAction> GetDefaultKeyActions ()
		{
			return Enumerable.Empty<KeyAction> ();
		}

		// FIXME [LON-995]: This is only called from a View, it should be refactored to a command
		protected virtual void HandleDashboardEditedEvent (DashboardEditedEvent e)
		{
			project.Model.UpdateEventTypesAndTimers ();
		}

		// FIXME [LON-995]: This is never called, should it?
		protected virtual void HandleTagSubcategoriesChangedEvent (TagSubcategoriesChangedEvent e)
		{
			App.Current.Config.FastTagging = !e.Active;
		}

		// FIXME [LON-995]: This is called only from MainWindow, it should be in a different controller
		protected virtual void HandleShowFullScreenEvent (ShowFullScreenEvent e)
		{
			App.Current.GUIToolkit.FullScreen = e.Active;
		}
	}
}
