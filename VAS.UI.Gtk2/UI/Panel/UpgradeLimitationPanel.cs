//
//  Copyright (C) 2017 Fluendo S.A.
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
using System;
using Gtk;
using Pango;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Hotkeys;
using VAS.Core.Interfaces.GUI;
using VAS.Core.MVVMC;
using VAS.Services.State;
using VAS.Services.ViewModel;
using VAS.UI.Helpers;
using VAS.UI.Helpers.Bindings;
using CommonColor = VAS.Core.Common.Color;

namespace VAS.UI.Panel
{
	[System.ComponentModel.ToolboxItem (true)]
	[ViewAttribute (UpgradeLimitationState.NAME)]
	public partial class UpgradeLimitationPanel : Gtk.Bin, IPanel<UpgradeLimitationVM>
	{
		const string UPGRADE_BUTTON_COLOR = "#6E921C";
		const string UNDECIDED_COLOR = "#EEEEEE";
		const int HEADER_LABEL_SIZE = 20;
		const int FEATURE_HEADER_LABEL_SIZE = 16;
		const int FEATURE_LABEL_SIZE = 14;
		const int APPLY_SIZE = 13;
		const int UNDECIDED_LABEL_SIZE = 10;

		UpgradeLimitationVM viewModel;
		BindingContext ctx;
		ImageView captionBackground;
		Button upgradeButton2;
		Label undecidedlbl;
		Label seeOtherPlanslbl;

		public UpgradeLimitationPanel ()
		{
			this.Build ();

			eventbox.ModifyBg (StateType.Normal, Helpers.Misc.ToGdkColor (App.Current.Style.PaletteBackground));
			header.ModifyFont (FontDescription.FromString ($"{App.Current.Style.Font} normal {HEADER_LABEL_SIZE}px"));
			featuresHeader.ModifyFont (FontDescription.FromString ($"{App.Current.Style.Font} light {FEATURE_HEADER_LABEL_SIZE}px"));
			featuresCaption.ModifyFont (FontDescription.FromString ($"{App.Current.Style.Font} light {FEATURE_LABEL_SIZE}px"));

			upgradeButton2 = ButtonHelper.CreateButton ();
			upgradeButton2.Name = StyleConf.ButtonCallToActionRounded;
			upgradeButton2.WidthRequest = 240;
			upgradeButton2.HeightRequest = 40;
			upgradeButton2.ModifyFont (FontDescription.FromString ($"{App.Current.Style.Font} bold {FEATURE_LABEL_SIZE}px"));
			upgradeButton2.BorderWidth = 0;

			undecidedlbl = new Label ();
			undecidedlbl.WidthRequest = 240;
			undecidedlbl.HeightRequest = 11;
			undecidedlbl.Justify = Justification.Center;
			undecidedlbl.ModifyFont (FontDescription.FromString ($"{App.Current.Style.Font} light {UNDECIDED_LABEL_SIZE}px"));
			undecidedlbl.ModifyText (StateType.Normal, Helpers.Misc.ToGdkColor (CommonColor.Parse (UNDECIDED_COLOR)));

			seeOtherPlanslbl = new Label ();
			seeOtherPlanslbl.WidthRequest = 240;
			seeOtherPlanslbl.HeightRequest = 14;
			seeOtherPlanslbl.Justify = Justification.Center;
			seeOtherPlanslbl.ModifyFont (FontDescription.FromString ($"{App.Current.Style.Font} normal {FEATURE_LABEL_SIZE}px"));
			seeOtherPlanslbl.ModifyText (StateType.Normal, Helpers.Misc.ToGdkColor (CommonColor.Parse (UPGRADE_BUTTON_COLOR)));

			captionBackground = new ImageView (App.Current.ResourcesLocator.LoadImage (StyleConf.UpgradeDialogBackground));
			captionFixed.Put (captionBackground, 0, 0);
			captionFixed.Put (upgradeButton2, (captionBackground.Image.Width / 2) - (upgradeButton2.WidthRequest / 2), 0);
			captionFixed.Put (undecidedlbl, (captionBackground.Image.Width / 2) - (undecidedlbl.WidthRequest / 2),
							  upgradeButton2.HeightRequest + 15);
			captionFixed.Put (seeOtherPlanslbl, (captionBackground.Image.Width / 2) - (seeOtherPlanslbl.WidthRequest / 2),
							  upgradeButton2.HeightRequest + 15 + undecidedlbl.HeightRequest + 5);

			WidthRequest = captionBackground.Image.Width;
			header.WidthRequest = WidthRequest - (int)headersAlignment.LeftPadding - (int)headersAlignment.RightPadding;
			header.Wrap = true;
			header.LineWrap = true;
			featuresHeader.WidthRequest = WidthRequest - (int)headersAlignment.LeftPadding - (int)headersAlignment.RightPadding;
			featuresHeader.Wrap = true;
			featuresHeader.LineWrap = true;
			Bind ();
		}

		public string Title {
			get {
				return "";
			}
		}

		public UpgradeLimitationVM ViewModel {
			get {
				return viewModel;
			}

			set {
				viewModel = value;
				if (viewModel != null) {
					AddFeatures ();
					ConfigureSeeOtherPlans ();
					ctx.UpdateViewModel (viewModel);
					viewModel.Sync ();
				}
			}
		}

		public KeyContext GetKeyContext ()
		{
			return new KeyContext ();
		}

		public void OnLoad ()
		{
		}

		public void OnUnload ()
		{
		}

		public void SetViewModel (object viewModel)
		{
			ViewModel = (UpgradeLimitationVM)viewModel;
		}

		void AddFeatures ()
		{
			EmptyFeaturesTable ();
			featuresTable.NColumns = (uint)ViewModel.Features.Count;
			featuresTable.NRows = 2;
			uint leftAttach = 0;
			uint rightAttach = 1;
			uint topAttach = 0;
			uint bottomAttach = 1;

			foreach (var feature in ViewModel.Features) {
				var image = new ImageView (App.Current.ResourcesLocator.LoadIcon ("vas-apply-button", APPLY_SIZE));
				image.Xalign = 0;
				featuresTable.Add (image);
				Table.TableChild w1 = (Table.TableChild)(featuresTable [image]);
				w1.TopAttach = topAttach;
				w1.BottomAttach = bottomAttach;
				w1.LeftAttach = leftAttach;
				w1.RightAttach = rightAttach;
				Label lbl = new Label (feature);
				lbl.Xalign = 0;
				lbl.Justify = Justification.Left;
				lbl.ModifyFont (FontDescription.FromString ($"{App.Current.Style.Font} light {FEATURE_LABEL_SIZE}px"));
				featuresTable.Add (lbl);
				Table.TableChild w2 = (Table.TableChild)(featuresTable [lbl]);
				w2.TopAttach = topAttach;
				w2.BottomAttach = bottomAttach;
				w2.LeftAttach = leftAttach + 1;
				w2.RightAttach = rightAttach + 1;
				topAttach++;
				bottomAttach++;
			}
		}

		void ConfigureSeeOtherPlans ()
		{
			if (!String.IsNullOrEmpty (ViewModel.OtherPlansURL)) {
				seeOtherPlanslbl.Text = "<a href=\"" + ViewModel.OtherPlansURL + "\">" + Catalog.GetString ("SEE OTHER PLANS") + "</a>";
				seeOtherPlanslbl.UseMarkup = true;
				seeOtherPlanslbl.UseUnderline = false;
				seeOtherPlanslbl.SetLinkHandler (HandleLinkClicked);
			}
		}

		void EmptyFeaturesTable ()
		{
			foreach (var child in featuresTable.Children) {
				featuresTable.Remove (child);
			}
			featuresTable.NColumns = 1;
			featuresTable.NRows = 1;
		}


		/// <summary>
		/// Bind the view elements with the viewmodel properties.
		/// </summary>
		void Bind ()
		{
			ctx = this.GetBindingContext ();
			ctx.Add (header.Bind (vm => ((UpgradeLimitationVM)vm).Header));
			ctx.Add (featuresHeader.Bind (vm => ((UpgradeLimitationVM)vm).FeaturesHeader));
			ctx.Add (featuresCaption.Bind (vm => ((UpgradeLimitationVM)vm).FeaturesCaption));
			ctx.Add (upgradeButton2.Bind (vm => ((UpgradeLimitationVM)vm).UpgradeCommand));
			ctx.Add (undecidedlbl.Bind (vm => ((UpgradeLimitationVM)vm).Undecided));
		}

		void HandleLinkClicked (string obj)
		{
			Utils.OpenURL (obj);
		}
	}
}
