//
//   Copyright (C) 2016 Fluendo S.A.
//
using System;

namespace VAS.Core.Events
{
	public class ClickedArgs : EventArgs
	{
		public ClickedArgs (string path)
		{
			Path = path;
		}

		public string Path {
			get;
		}
	}
}

