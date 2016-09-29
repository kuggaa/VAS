//
//  Copyright (C) 2016 Fluendo S.A.
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
	public class EventsController<TModel,TViewModel> : IController
		where TModel : TimelineEvent
		where TViewModel : TimelineEventVM<TModel>, new()
	{
		PlayerVM playerVM;

		public EventsController ()
		{
		}

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

