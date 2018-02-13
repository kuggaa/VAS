//
//  Copyright (C) 2018 Fluendo S.A.
using System.Threading.Tasks;
using VAS.Core.ViewModel;

namespace VAS.Core.Interfaces.Services
{
	public interface IEventEditorService
	{
		Task EditEvent (TimelineEventVM timelineEvent);
	}
}
