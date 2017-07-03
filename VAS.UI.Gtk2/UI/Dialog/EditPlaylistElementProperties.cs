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
using VAS.Core.Common;
using VAS.Core.Hotkeys;
using VAS.Core.Interfaces.GUI;
using VAS.Core.MVVMC;
using VAS.Core.ViewModel;
using VAS.Services.State;
using VAS.UI.Helpers.Bindings;

namespace VAS.UI.Dialog
{
	[ViewAttribute (EditPlaylistElementState.NAME)]
	public partial class EditPlaylistElementProperties : Gtk.Dialog, IPanel<PlaylistElementVM>
	{
		BindingContext ctx;
		SizeGroup sizegroupLeft, sizegroupRight;
		PlaylistElementVM plElement;

		public EditPlaylistElementProperties ()
		{
			this.Build ();

			sizegroupLeft = new SizeGroup (SizeGroupMode.Horizontal);
			sizegroupLeft.IgnoreHidden = false;
			foreach (Widget w in vbox2.Children) {
				foreach (Widget t in (w as Table).Children) {
					if ((t is Label)) {
						t.ModifyFont (FontDescription.FromString (App.Current.Style.Font + " 10"));
						sizegroupLeft.AddWidget (t);
					}
				}
			}

			sizegroupRight = new SizeGroup (SizeGroupMode.Horizontal);
			sizegroupRight.IgnoreHidden = false;
			foreach (Widget w in vbox2.Children) {
				foreach (Widget t in (w as Table).Children) {
					if (!(t is Label)) {
						sizegroupRight.AddWidget (t);
					}
				}
			}
			buttonOk.Clicked += HandleButtonOkClicked;
			Bind ();
		}

		public PlaylistElementVM ViewModel {
			get {
				return plElement;
			}

			set {

				plElement = value;
				if (plElement != null) {
					if (plElement is PlaylistImageVM || plElement is PlaylistDrawingVM) {
						slidetable.Visible = true;
						durationspinbutton.Value = plElement.Duration.Seconds;
					} else {
						slidetable.Visible = false;
					}

					if (plElement is PlaylistPlayElementVM) {
						nameentry.Text = ((PlaylistPlayElementVM)plElement).Title;
						nametable.Visible = true;
					} else {
						nametable.Visible = false;
					}
					Show ();
					ctx.UpdateViewModel (plElement);
				}
			}
		}

		public void OnLoad ()
		{
		}

		public void OnUnload ()
		{
		}

		public void SetViewModel (object viewModel)
		{
			ViewModel = (PlaylistElementVM)viewModel;
		}

		public KeyContext GetKeyContext ()
		{
			return null;
		}

		void Bind ()
		{
			ctx = this.GetBindingContext ();
			ctx.Add (nameentry.Bind (vm => ((PlaylistPlayElementVM)vm).Title));
			ctx.Add (durationspinbutton.Bind (vm => ((PlaylistElementVM)vm).Duration.TotalSeconds,
											 new VASInt32Converter ()));
		}

		void HandleButtonOkClicked (object sender, EventArgs e)
		{
			App.Current.StateController.MoveBack ();
		}
	}
}
