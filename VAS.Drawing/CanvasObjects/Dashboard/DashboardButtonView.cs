//
//  Copyright (C) 2014 Andoni Morales Alastruey
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
using System.Collections.Generic;
using System.ComponentModel;
using VAS.Core.Common;
using VAS.Core.Interfaces.Drawing;
using VAS.Core.Store;
using VAS.Core.Store.Drawables;
using VAS.Core.ViewModel;

namespace VAS.Drawing.CanvasObjects.Dashboard
{
	/// <summary>
	/// Class for the DashboardButton View.
	/// </summary>
	public class DashboardButtonView : ButtonObject, ICanvasSelectableObject
	{
		protected LinkAnchorView anchor;
		protected const int HOTKEY_WIDTH = 5;
		DashboardButtonVM buttonVM;

		public DashboardButtonView ()
		{
			SupportsLinks = true;
			anchor = new LinkAnchorView (this, null, new Point (0, 0));
			anchor.RedrawEvent += (co, area) => {
				EmitRedrawEvent (anchor, area);
			};
		}

		protected override void DisposeManagedResources ()
		{
			base.DisposeManagedResources ();
			anchor.Dispose ();
		}

		public bool SupportsLinks {
			get;
			set;
		}

		public bool ShowLinks {
			get;
			set;
		}

		public DashboardButtonVM ButtonVM {
			get {
				return buttonVM;
			}
			set {
				if (buttonVM != null) {
					buttonVM.PropertyChanged -= HandlePropertyChanged;
				}
				buttonVM = value;
				if (buttonVM != null) {
					buttonVM.PropertyChanged += HandlePropertyChanged;
				}
			}
		}

		// FIXME: remove it when everything is ported to MVVM
		public DashboardButton Button {
			get {
				return ButtonVM.Model;
			}
		}

		public bool EditActionLinks {
			get;
			set;
		}

		public override bool DrawsSelectionArea {
			get {
				return ButtonVM.Mode == DashboardMode.Edit;
			}
		}

		public override Point Position {
			get {
				return Button.Position;
			}
			set {
				Button.Position = value;
			}
		}

		public override double Width {
			get {
				return Button.Width;
			}
			set {
				Button.Width = (int)value;
			}
		}

		public override double Height {
			get {
				return Button.Height;
			}
			set {
				Button.Height = (int)value;
			}
		}

		public override Color BackgroundColor {
			get {
				return Button.BackgroundColor;
			}
		}

		public override Color BackgroundColorActive {
			get {
				return Button.DarkColor;
			}
		}

		public override Color BorderColor {
			get {
				return Button.TextColor;
			}
		}

		public override Color TextColor {
			get {
				return Button.TextColor;
			}
		}

		public override Image BackgroundImage {
			get {
				return Button.BackgroundImage;
			}
		}

		public override Image BackgroundImageActive {
			get {
				return Button.BackgroundImage;
			}
		}

		public override bool Active {
			get {
				return base.Active;
			}
			set {
				if (ButtonVM.Mode != DashboardMode.Edit) {
					base.Active = value;
				}
			}
		}

		public virtual int NRows {
			get {
				return 1;
			}
		}

		/// <summary>
		/// Gets a value indicating whether this <see cref="T:VAS.Drawing.CanvasObjects.Dashboard.DashboardButtonView"/> show hotkey.
		/// </summary>
		/// <value><c>true</c> if show hotkey; otherwise, <c>false</c>.</value>
		bool ShowHotkey {
			get {
				return (Button.ShowHotkey && Button.HotKey.Key != -1);
			}
		}

		public virtual LinkAnchorView GetAnchor (IList<TagVM> sourceTags)
		{
			return anchor;
		}

		/// <summary>
		/// Draws the hotkey.
		/// </summary>
		/// <param name="tk">Tk.</param>
		protected virtual void DrawHotkey (IDrawingToolkit tk)
		{
			if (!ShowHotkey)
				return;

			Point pos;

			pos = new Point (Position.X + 5, Position.Y + 5);
			tk.FontFamily = App.Current.Style.NamesFontFamily;
			tk.FontSize = App.Current.Style.NamesFontSize;
			tk.StrokeColor = App.Current.Style.Text_DarkColor;
			tk.StrokeColor = App.Current.Style.Text_DarkColor;
			tk.FontWeight = FontWeight.Bold;
			tk.FontAlignment = FontAlignment.Left;
			tk.DrawText (pos, HOTKEY_WIDTH, App.Current.Style.TitlesFontSize, Button.HotKey.ToString (), false, false);
		}

		protected void DrawAnchor (IDrawingToolkit tk, Area area)
		{
			if (ShowLinks && SupportsLinks) {
				anchor.Draw (tk, area);
			}
		}

		public override void ResetDrawArea ()
		{
			anchor.ResetDrawArea ();
			base.ResetDrawArea ();
		}

		public override void Draw (IDrawingToolkit tk, Area area)
		{
			base.Draw (tk, area);
			DrawAnchor (tk, area);
		}

		public override Selection GetSelection (Point p, double precision, bool inMotion = false)
		{
			if (ShowLinks && SupportsLinks) {
				Selection sel = anchor.GetSelection (p, precision, inMotion);
				if (sel != null)
					return sel;
			}
			return base.GetSelection (p, precision, inMotion);
		}

		protected virtual void HandlePropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (sender == ButtonVM && (
				ButtonVM.NeedsSync (e, nameof (ButtonVM.BackgroundColor)) ||
				ButtonVM.NeedsSync (e, nameof (ButtonVM.TextColor)) ||
				ButtonVM.NeedsSync (e, nameof (ButtonVM.Name)) ||
				ButtonVM.NeedsSync (e, nameof (ButtonVM.HotKey)))) {
				ReDraw ();
			}
		}
	}
}

