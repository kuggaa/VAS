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
using System.ComponentModel;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Interfaces.Drawing;
using VAS.Core.MVVMC;
using VAS.Core.Store.Drawables;
using VAS.Core.ViewModel;

namespace VAS.Drawing.CanvasObjects.Dashboard
{
	/// <summary>
	/// Class for the TimerButton View
	/// </summary>
	[View ("TimerButtonView")]
	public class TimerButtonView : DashboardButtonView, ICanvasObjectView<TimerButtonVM>
	{
		static Image iconImage;
		protected static Image cancelImage;
		protected Rectangle cancelRect;
		bool cancelPressed;

		public TimerButtonView () : base ()
		{
			Toggle = true;
			if (iconImage == null) {
				iconImage = Resources.LoadImage (StyleConf.ButtonTimerIcon, StyleConf.ButtonHeaderWidth * 2, StyleConf.ButtonHeaderHeight * 2);
			}
			if (cancelImage == null) {
				cancelImage = Resources.LoadImage (StyleConf.CancelButton, StyleConf.ButtonHeaderWidth * 2, StyleConf.ButtonHeaderHeight * 2);
			}
			MinWidth = StyleConf.ButtonMinWidth;
			MinHeight = iconImage.Height + StyleConf.ButtonTimerFontSize;
			cancelRect = new Rectangle ();
		}

		/// <summary>
		/// Gets the icon.
		/// </summary>
		/// <value>The icon.</value>
		public override Image Icon {
			get {
				return iconImage;
			}
		}

		public Image TeamImage {
			get;
			set;
		}

		public override void ClickPressed (Point p, ButtonModifier modif)
		{
			cancelPressed = cancelRect.GetSelection (p) != null;
			base.ClickPressed (p, modif);
		}

		public override void ClickReleased ()
		{
			if (ButtonVM.Mode == DashboardMode.Edit) {
				return;
			}
			base.ClickReleased ();
			ViewModel.Click (cancelPressed);
		}

		/// <summary>
		/// Gets or sets the view model.
		/// </summary>
		/// <value>The view model.</value>
		public TimerButtonVM ViewModel {
			get {
				return ButtonVM as TimerButtonVM;
			}

			set {
				ButtonVM = value;
			}
		}

		/// <summary>
		/// Gets the height of the header.
		/// </summary>
		/// <value>The height of the header.</value>
		protected int HeaderHeight {
			get {
				return iconImage.Height + 5;
			}
		}

		/// <summary>
		/// Gets the text header x.
		/// </summary>
		/// <value>The text header x.</value>
		protected int TextHeaderX {
			get {
				return iconImage.Width + 5 * 2;
			}
		}

		/// <summary>
		/// Sets the view model.
		/// </summary>
		/// <param name="viewModel">View model.</param>
		public void SetViewModel (object viewModel)
		{
			ViewModel = (TimerButtonVM)viewModel;
		}

		/// <summary>
		/// Draw the specified tk and area.
		/// </summary>
		/// <param name="tk">Tk.</param>
		/// <param name="area">Area.</param>
		public override void Draw (IDrawingToolkit tk, Area area)
		{
			if (!UpdateDrawArea (tk, area, Area)) {
				return;
			}

			base.Draw (tk, area);

			tk.Begin ();
			DrawTimer (tk);
			tk.End ();
		}

		/// <summary>
		/// Draws the timer.
		/// </summary>
		/// <param name="tk">Tk.</param>
		protected virtual void DrawTimer (IDrawingToolkit tk)
		{
			cancelRect = new Rectangle (
				new Point ((Position.X + Width) - StyleConf.ButtonRecWidth, Position.Y),
				StyleConf.ButtonRecWidth, HeaderHeight);

			if (ViewModel.TimerTime != null && ButtonVM.Mode != DashboardMode.Edit) {
				tk.LineWidth = StyleConf.ButtonLineWidth;
				tk.StrokeColor = Button.BackgroundColor;
				tk.FillColor = Button.BackgroundColor;
				tk.FontWeight = FontWeight.Normal;
				tk.FontSize = StyleConf.ButtonHeaderFontSize;
				tk.FontAlignment = FontAlignment.Left;
				tk.DrawText (new Point (Position.X + TextHeaderX, Position.Y),
							 Button.Width - TextHeaderX, StyleConf.ButtonHeaderHeight, ViewModel.Name);
				tk.FontWeight = FontWeight.Bold;
				tk.FontSize = StyleConf.ButtonTimerFontSize;
				tk.FontAlignment = FontAlignment.Center;
				tk.DrawText (new Point (Position.X, Position.Y + StyleConf.ButtonHeaderHeight),
					Button.Width, Button.Height - StyleConf.ButtonHeaderHeight,
					ViewModel.TimerTime.ToSecondsString (), false, true);

				tk.FillColor = tk.StrokeColor = BackgroundColor;
				tk.DrawRectangle (cancelRect.TopLeft, cancelRect.Width, cancelRect.Height);
				tk.StrokeColor = TextColor;
				tk.FillColor = TextColor;
				tk.DrawImage (new Point (cancelRect.TopLeft.X, cancelRect.TopLeft.Y + 5),
					cancelRect.Width, cancelRect.Height - 10, cancelImage, ScaleMode.AspectFit, true);
			} else {
				Text = Button.Name;
				DrawText (tk);
				Text = null;
			}

			if (TeamImage != null) {
				tk.DrawImage (new Point (Position.X + Width - 40, Position.Y + 5), 40,
					iconImage.Height, TeamImage, ScaleMode.AspectFit);
			}
		}

		protected override void HandlePropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (sender != ViewModel) {
				return;
			}

			if (ViewModel.NeedsSync (e, nameof (ViewModel.BackgroundColor)) ||
				ViewModel.NeedsSync (e, nameof (ViewModel.TextColor)) ||
				ViewModel.NeedsSync (e, nameof (ViewModel.Name)) ||
				ViewModel.NeedsSync (e, nameof (ViewModel.TimerTime)) ||
				ViewModel.NeedsSync (e, nameof (ViewModel.HotKey))) {
				ReDraw ();
			} else if (ViewModel.NeedsSync (e, nameof (ViewModel.Active))) {
				Active = ViewModel.Active;
				ReDraw ();
			}
		}
	}
}

