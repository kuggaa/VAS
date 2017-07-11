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
using Pango;
using VAS.Core.Common;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
using VAS.Core.ViewModel;
using VAS.UI.Helpers;
using VAS.UI.Helpers.Bindings;

namespace VAS.UI.Component
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class MessageWidget : Gtk.Bin, IView<MessageVM>
	{
		MessageVM viewModel;
		BindingContext ctx;

		public MessageWidget ()
		{
			this.Build ();

			messageLabel.ModifyFont (FontDescription.FromString (App.Current.Style.ContentFont));

			Bind ();
		}

		public MessageVM ViewModel {
			get {
				return viewModel;
			}

			set {
				viewModel = value;
				ctx.UpdateViewModel (viewModel);
				viewModel?.Sync ();
			}
		}

		public void SetViewModel (object viewmodel)
		{
			ViewModel = (MessageVM)viewmodel;
			Bind ();
		}

		void Bind ()
		{
			ctx = this.GetBindingContext ();
			/*
			ctx.Add (messageLabel.Bind (vm => ((MessageVM)vm).Message));
			ctx.Add (iconImage.Bind (vm => ((MessageVM)vm).Icon));
			var closeDialogImage = App.Current.ResourcesLocator.LoadImage (StyleConf.CloseDialog);
			closeButton.SetImage (closeDialogImage.Value);
			//ctx.Add (messageEventbox.ModifyFg (Gtk.StateType.Normal, Misc.ToGdkColor (App.Current.Style.TextColor)).Bind (vm => ((MessageVM)vm).Icon));
			messageEventbox.ModifyFg (Gtk.StateType.Normal, Misc.ToGdkColor (App.Current.Style.TextColor));
			*/
		}
	}
}
