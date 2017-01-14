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
using VAS.Core.Events;
using VAS.Core.Hotkeys;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
using VAS.Core.ViewModel;

namespace VAS.Services.Controller
{
	/// <summary>
	/// Events controller, base class of the Events Controller.
	/// </summary>
	public class EventsController : DisposableBase, IController
	{
		VideoPlayerVM playerVM;
		TimelineVM timelineVM;

		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
			Stop ();
		}

		public VideoPlayerVM PlayerVM {
			get {
				return playerVM;
			}
			set {
				if (playerVM != null) {
					playerVM.PropertyChanged -= HandlePlayerVMPropertyChanged;
				}
				playerVM = value;
				if (playerVM != null) {
					playerVM.PropertyChanged += HandlePlayerVMPropertyChanged;
				}
			}
		}

		public virtual TimelineVM Timeline {
			get {
				return timelineVM;
			}
			set {
				if (timelineVM != null) {
					timelineVM.Filters.PropertyChanged -= HandlePropertyChanged;
				}
				timelineVM = value;
				if (timelineVM != null) {
					HandleFiltersChanged ();
					timelineVM.Filters.PropertyChanged += HandlePropertyChanged;
				}
			}
		}

		#region IController implementation

		public virtual void Start ()
		{
			App.Current.EventsBroker.Subscribe<LoadTimelineEventEvent<TimelineEventVM>> (HandleLoadEvent);
			App.Current.EventsBroker.Subscribe<LoadTimelineEventEvent<IEnumerable<TimelineEventVM>>> (HandleLoadEventsList);
			App.Current.EventsBroker.Subscribe<LoadTimelineEventEvent<EventTypeTimelineVM>> (HandleLoadEventType);
		}

		public virtual void Stop ()
		{
			App.Current.EventsBroker.Unsubscribe<LoadTimelineEventEvent<TimelineEventVM>> (HandleLoadEvent);
			App.Current.EventsBroker.Unsubscribe<LoadTimelineEventEvent<IEnumerable<TimelineEventVM>>> (HandleLoadEventsList);
			App.Current.EventsBroker.Unsubscribe<LoadTimelineEventEvent<EventTypeTimelineVM>> (HandleLoadEventType);
		}

		public virtual void SetViewModel (IViewModel viewModel)
		{
			PlayerVM = (VideoPlayerVM)(viewModel as dynamic);
			Timeline = (TimelineVM)(viewModel as dynamic);
		}

		public virtual IEnumerable<KeyAction> GetDefaultKeyActions ()
		{
			return Enumerable.Empty<KeyAction> ();
		}

		#endregion

		protected virtual void HandlePlayerVMPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
		}

		void HandleLoadEvent (LoadTimelineEventEvent<TimelineEventVM> e)
		{
			PlayerVM.LoadEvent (e.Object.Model, e.Playing);
		}

		void HandleLoadEventsList (LoadTimelineEventEvent<IEnumerable<TimelineEventVM>> e)
		{
			PlayerVM.LoadEvents (e.Object.Select (vm => vm.Model), e.Playing);
		}

		void HandleLoadEventType (LoadTimelineEventEvent<EventTypeTimelineVM> e)
		{
			var timelineEvents = e.Object.ViewModels.Where ((arg) => arg.Visible == true)
													.Select ((arg) => arg.Model);
			PlayerVM.LoadEvents (timelineEvents, e.Playing);
		}

		void HandlePropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "Collection" || e.PropertyName == "Active") {
				HandleFiltersChanged ();
			}
		}

		void HandleFiltersChanged ()
		{
			foreach (var eventVM in Timeline.SelectMany (eventTypeVM => eventTypeVM.ViewModels)) {
				eventVM.Visible = Timeline.Filters.Filter (eventVM);
			}
		}
	}
}

