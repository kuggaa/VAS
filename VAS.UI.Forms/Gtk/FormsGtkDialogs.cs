//
//  Copyright (C) 2017 Fluendo S.A.
using System;
using VAS.Core.Store;
using VAS.UI.Helpers;

namespace VAS.UI.Forms
{
	public class FormsGtkDialogs : FormsDialogs
	{
		public override MediaFile OpenMediaFile(object parent = null)
		{
			return Helpers.Misc.OpenFile(parent);
		}

		public override string OpenFile(string title, string defaultName, string defaultFolder, string filterName = null, string[] extensionFilter = null)
		{
			return FileChooserHelper.OpenFile(null, title, defaultName, defaultFolder, filterName, extensionFilter);
		}

		public override Core.Interfaces.GUI.IBusyDialog BusyDialog(string message, object parent = null)
		{
			return base.BusyDialog(message, parent);
		}
	}
}
