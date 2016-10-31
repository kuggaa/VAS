//
//  Copyright (C) 2016 
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
//

using System;
using System.Collections.Generic;
using Microsoft.HockeyApp.Extensibility;
using Microsoft.HockeyApp.Extensibility.Implementation.External;
using Microsoft.HockeyApp.Services;

namespace VAS.KPI.Services
{
	class PlatformService : IPlatformService
	{
		public IDebugOutput GetDebugOutput ()
		{
			throw new NotSupportedException ();
		}

		public ExceptionDetails GetExceptionDetails (Exception exception, ExceptionDetails parentExceptionDetails)
		{
			throw new NotSupportedException ();
		}

		public IDictionary<string, object> GetLocalApplicationSettings ()
		{
			return new Dictionary<string, object> ();
		}

		public IDictionary<string, object> GetRoamingApplicationSettings ()
		{
			return new Dictionary<string, object> ();
		}

		public string ReadConfigurationXml ()
		{
			throw new NotSupportedException ();
		}

		public string SdkName ()
		{
			return "HockeySDK.Mono";
		}
	}
}
