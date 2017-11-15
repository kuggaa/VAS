//
//  Copyright (C) 2017 Fluendo S.A.
using System;
using System.Linq;
using System.Threading.Tasks;
using VAS.Core.Common;
using VAS.Core.Interfaces.GUI;
using Xamarin.Forms;

namespace VAS.UI.Forms
{
	public class FormsNavigation : VAS.Core.Interfaces.GUI.INavigation
	{
		public static Xamarin.Forms.INavigation Navigation
		{
			get
			{
				return Application.Current.MainPage?.Navigation;
			}
		}

		public async Task<bool> Push(IPanel panel)
		{
			if (Navigation == null)
			{
				Application.Current.MainPage = panel as Page;
			}
			else
			{
				await Navigation.PushAsync(panel as Page);
			}
			return true;
		}

		public async Task<bool> Pop(IPanel panel)
		{
			await Navigation.PopAsync();
			if (panel != null && (panel as Page) != Navigation.NavigationStack.Last())
			{
				Log.Error("Inconsistency in panels stack");
			}
			return true;
		}

		public Task PushModal(IPanel panel, IPanel parent)
		{
			UpdateParentVisibility(parent as Page, false);
			return Navigation.PushModalAsync(panel as Page);
		}

		public Task PopModal(IPanel panel)
		{
			UpdateParentVisibility(Navigation.NavigationStack.Last(), true);
			return Navigation.PopModalAsync();
		}

		void UpdateParentVisibility(Page parent, bool visible)
		{
			// Fixme: in android the new panel is set behind the parent, the visibility must be set to false
			if (Utils.OS == OperatingSystemID.Android)
			{
				var parentContent = (parent as ContentPage);
				if (parentContent != null)
				{
					parentContent.Content.IsVisible = visible;
				}
			}
		}
	}
}
