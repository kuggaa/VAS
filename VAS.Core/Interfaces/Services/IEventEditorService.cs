//
//  Copyright (C) 2018 Fluendo S.A.
using System.Threading.Tasks;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.ViewModel;

namespace VAS.Core.Interfaces.Services
{
	public interface IEventEditorService : IController
	{
		Task EditEvent (TimelineEventVM timelineEvent);

		void SetDefaultCallbacks (TimelineVM timeline);
	}
}
