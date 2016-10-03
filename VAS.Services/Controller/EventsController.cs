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
using VAS.Services.ViewModel;
using System.Collections.ObjectModel;
using VAS.Core.Events;

namespace VAS.Services.Controller
{
	/// <summary>
	/// Events controller, base class of the Events Controller.
	/// </summary>
	public class EventsController<TModel,TViewModel> : IController
		where TModel : TimelineEvent
		where TViewModel : TimelineEventVM<TModel>, new()
	{
		PlayerVM playerVM;

		#region IController implementation

		public virtual void Start ()
		{
			App.Current.EventsBroker.Subscribe<OpenEvent<TModel>> (HandleOpenEvent);
			App.Current.EventsBroker.Subscribe<OpenEvent<List<TModel>>> (HandleOpenListEvent);
		}

		public virtual void Stop ()
		{
			App.Current.EventsBroker.Unsubscribe<OpenEvent<TModel>> (HandleOpenEvent);
			App.Current.EventsBroker.Unsubscribe<OpenEvent<List<TModel>>> (HandleOpenListEvent);
		}

		public virtual void SetViewModel (IViewModel viewModel)
		{
			if (viewModel is IAnalysisViewModel) {
				playerVM = (PlayerVM)(viewModel as IAnalysisViewModel).PlayerViewModel;
			}
		}

		public virtual IEnumerable<VAS.Core.Hotkeys.KeyAction> GetDefaultKeyActions ()
		{
			return Enumerable.Empty<KeyAction> ();
		}

		#endregion

		#region IDisposable implementation

		public void Dispose ()
		{
			
		}

		#endregion

		void HandleOpenEvent (OpenEvent<TModel> e)
		{
			playerVM.LoadEvent (e.Object);
		}

		void HandleOpenListEvent (OpenEvent<List<TModel>> e)
		{
			playerVM.LoadEvents (e.Object.OfType<TimelineEvent> ().ToList ());
		}
	}
}

