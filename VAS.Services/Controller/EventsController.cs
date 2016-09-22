//
//  Copyright (C) 2016 Fluendo S.A.
using System;
using System.Collections.Generic;
using System.Linq;
using VAS.Core.Hotkeys;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.Store;
using VAS.Services.ViewModel;

namespace VAS.Services.Controller
{
	public class EventsController<TModel,TViewModel> : IController
		where TModel : TimelineEvent
		where TViewModel : TimelineEventVM<TModel>, new()
	{

		ProjectVM<Project> projectVM;

		public EventsController ()
		{
		}

		#region IController implementation

		public virtual void Start ()
		{
			
		}

		public virtual void Stop ()
		{
			
		}

		public virtual void SetViewModel (IViewModel viewModel)
		{
			projectVM = (ProjectVM<Project>)viewModel;
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
	}
}

