//
//  Copyright (C) 2017 Fluendo S.A.
using System;
using VAS.Core.Interfaces.GUI;
using Xamarin.Forms.Platform.GTK;

namespace VAS.UI.Forms
{
	public class GtkFormsWindow : FormsWindow, IMainController
	{
		public bool SetPanel(IPanel newPanel)
		{
			throw new NotImplementedException();
		}
	}
}
