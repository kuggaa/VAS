//
//  Copyright (C) 2016 Fluendo S.A.
using VAS.Core.Store.Templates;

namespace VAS.Core.Interfaces
{
	public interface ITeamTemplatesProvider<T> : ITemplateProvider<T> where T : Team
	{
	}
}
