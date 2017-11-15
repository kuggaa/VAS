//
//  Copyright (C) 2017 Fluendo S.A.
using System;
using FormsView = Xamarin.Forms.View;

namespace VAS.UI.Forms.Components
{
	public class GtkWidget : FormsView
	{
		public Type ViewType {
			get;
			set;
		}
	}
}
