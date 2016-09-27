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

		ProjectVM<Project> projectVM;
		PlayerVM playerVM;

		public EventsController ()
		{
		}

		#region IController implementation

		public virtual void Start ()
		{
			App.Current.EventsBroker.Subscribe<CreateEvent<TModel>> (HandleCreateEvent);
			App.Current.EventsBroker.Subscribe<DeleteEvent<TModel>> (HandleDeleteEvent);
			App.Current.EventsBroker.Subscribe<OpenEvent<TModel>> (HandleOpenEvent);
		}

		public virtual void Stop ()
		{
			App.Current.EventsBroker.Unsubscribe<CreateEvent<TModel>> (HandleCreateEvent);
			App.Current.EventsBroker.Unsubscribe<DeleteEvent<TModel>> (HandleDeleteEvent);
			App.Current.EventsBroker.Unsubscribe<OpenEvent<TModel>> (HandleOpenEvent);
		}

		public virtual void SetViewModel (IViewModel viewModel)
		{
			if (viewModel is IAnalysisViewModel) {
				playerVM = (PlayerVM)(viewModel as IAnalysisViewModel).PlayerViewModel;
				//projectVM = (ProjectVM<Project>)(viewModel as IAnalysisViewModel).ProjectVM;
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

		void HandleCreateEvent (CreateEvent<TModel> e)
		{
			projectVM.Model.AddEvent (e.Object);
		}

		void HandleDeleteEvent (DeleteEvent<TModel> e)
		{
			projectVM.Model.Timeline.Remove (e.Object);
		}

		void HandleOpenEvent (OpenEvent<TModel> e)
		{
			playerVM.LoadEvent (e.Object);
		}
	}
}

