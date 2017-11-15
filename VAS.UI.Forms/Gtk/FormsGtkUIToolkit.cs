//
//  Copyright (C) 2017 Fluendo S.A.
using System;
using VAS.Core.Interfaces.GUI;

namespace VAS.UI.Forms
{
	public class FormsGtkUIToolkit : FormsUIToolkit
	{
		public FormsGtkUIToolkit(IMainController controller)
		{
			MainController = controller;
		}
	}
}
