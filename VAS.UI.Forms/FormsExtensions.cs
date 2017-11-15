//
//  Copyright (C) 2017 Fluendo S.A.
using System;
using Xamarin.Forms;

namespace VAS.UI.Forms
{
	public static class FormsExtensions
	{
		public static Page CurrentPage(this Element view)
		{
			var parent = view.Parent;
			if (parent is Page && !(parent is ContentPage))
			{
				return parent as Page;
			}
			if (parent == null)
			{
				return Application.Current.MainPage;
			}
			return CurrentPage(parent);
		}
	}
}
