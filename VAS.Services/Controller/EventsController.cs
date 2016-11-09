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
using System.Collections.Generic;
using System.Linq;
using VAS.Core.Hotkeys;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.Store;
using VAS.Core.Events;
using VAS.Core.MVVMC;
using VAS.Core.ViewModel;
using VAS.Services.ViewModel;

namespace VAS.Services.Controller
{
	/// <summary>
	/// Events controller, base class of the Events Controller.
	/// </summary>
	public class EventsController<TModel, TViewModel> : DisposableBase, IController
		where TModel : TimelineEvent
		where TViewModel : TimelineEventVM<TModel>, new()
	{
		VideoPlayerVM playerVM;

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

		#region IController implementation

		public virtual void Start ()
		{
			App.Current.EventsBroker.Subscribe<LoadTimelineEvent<TModel>> (HandleOpenEvent);
			App.Current.EventsBroker.Subscribe<LoadTimelineEvent<IEnumerable<TModel>>> (HandleOpenListEvent);
		}

		public virtual void Stop ()
		{
			App.Current.EventsBroker.Unsubscribe<LoadTimelineEvent<TModel>> (HandleOpenEvent);
			App.Current.EventsBroker.Unsubscribe<LoadTimelineEvent<IEnumerable<TModel>>> (HandleOpenListEvent);
		}

		public virtual void SetViewModel (IViewModel viewModel)
		{
			if (viewModel is IAnalysisViewModel) {
				PlayerVM = (VideoPlayerVM)(viewModel as IAnalysisViewModel).PlayerViewModel;
			}
		}

		public virtual IEnumerable<VAS.Core.Hotkeys.KeyAction> GetDefaultKeyActions ()
		{
			return Enumerable.Empty<KeyAction> ();
		}

		#endregion

		void HandleOpenEvent (LoadTimelineEvent<TModel> e)
		{
			PlayerVM.LoadEvent (e.Object, e.Playing);
		}

		void HandleOpenListEvent (LoadTimelineEvent<IEnumerable<TModel>> e)
		{
			PlayerVM.LoadEvents (e.Object.OfType<TimelineEvent> ().ToList (), e.Playing);
		}

		protected virtual void HandlePlayerVMPropertyChanged (object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
		}
	}
}

