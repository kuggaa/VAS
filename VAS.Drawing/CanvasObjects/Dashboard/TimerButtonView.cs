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
using System;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Interfaces.Drawing;
using VAS.Core.MVVMC;
using VAS.Core.Store;
using VAS.Core.Store.Drawables;
using VAS.Core.ViewModel;

namespace VAS.Drawing.CanvasObjects.Dashboard
{
	/// <summary>
	/// Class for the TimerButton View
	/// </summary>
	[ViewAttribute ("TimerButtonView")]
	public class TimerButtonView : DashboardButtonView, ICanvasObjectView<TimerButtonVM>
	{
		Time currentTime;
		static Image iconImage;
		protected static Image cancelImage;
		protected Rectangle cancelRect;
		bool cancelPressed;
		TimerButton timerButton;
		TimerButtonVM viewModel;

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
		/// Gets or sets the timer button.
		/// </summary>
		/// <value>The timer button.</value>
		public TimerButton TimerButton {
			get {
				return timerButton;
			}
			set {
				timerButton = value;
				Button = value;
			}
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

		public Time CurrentTime {
			set {
				bool update = false;
				bool wasActive = Active;
				bool isActive = wasActive;

				if (!wasActive) {
					if (TimerButton.StartTime != null) {
						isActive = true;
						update = true;
					}
				} else {
					// In case the node is no longer valid, unactivate it
					if (TimerButton.StartTime == null) {
						isActive = false;
						update = true;
					} else if (value < TimerButton.StartTime) {
						// In case the application seeks to a position before the moment
						// the button was clicked, make sure to cancel such node
						TimerButton.Cancel ();
						update = true;
						isActive = false;
					}
				}

				if (value != null && currentTime != null &&
					currentTime.TotalSeconds != value.TotalSeconds) {
					update = true;
				}

				currentTime = value;

				if (update) {
					// It is possible that the button is activated but not thtough a click
					Active = isActive;
					ReDraw ();
				}
			}
			get {
				return currentTime;
			}
		}

		/// <summary>
		/// Gets the partial time.
		/// </summary>
		/// <value>The partial time.</value>
		protected Time PartialTime {
			get {
				if (TimerButton.StartTime == null) {
					return new Time (0);
				} else {
					return CurrentTime - TimerButton.StartTime;
				}
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
			if (Mode == DashboardMode.Edit) {
				return;
			}
			base.ClickReleased ();
			if (TimerButton.StartTime == null) {
				Log.Debug ("Start timer at " + CurrentTime.ToMSecondsString ());
				TimerButton.Start (CurrentTime, null);
			} else {
				if (cancelPressed) {
					Log.Debug ("Cancel timer from button");
					TimerButton.Cancel ();
				} else {
					Log.Debug ("Stop timer at " + CurrentTime.ToMSecondsString ());
					if (TimerButton.StartTime.MSeconds != CurrentTime.MSeconds) {
						TimerButton.Stop (CurrentTime, null);
					} else {
						TimerButton.Cancel ();
					}
				}
			}
		}

		/// <summary>
		/// Gets or sets the view model.
		/// </summary>
		/// <value>The view model.</value>
		public TimerButtonVM ViewModel {
			get {
				return viewModel;
			}

			set {
				viewModel = value;
				if (viewModel != null) {
					TimerButton = viewModel.Model;
					CurrentTime = new Time (0);
				}
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

			if (Active && Mode != DashboardMode.Edit) {
				tk.LineWidth = StyleConf.ButtonLineWidth;
				tk.StrokeColor = Button.BackgroundColor;
				tk.FillColor = Button.BackgroundColor;
				tk.FontWeight = FontWeight.Normal;
				tk.FontSize = StyleConf.ButtonHeaderFontSize;
				tk.FontAlignment = FontAlignment.Left;
				tk.DrawText (new Point (Position.X + TextHeaderX, Position.Y),
							 TimerButton.Width - TextHeaderX, StyleConf.ButtonHeaderHeight, TimerButton.Timer.Name);
				tk.FontWeight = FontWeight.Bold;
				tk.FontSize = StyleConf.ButtonTimerFontSize;
				tk.FontAlignment = FontAlignment.Center;
				tk.DrawText (new Point (Position.X, Position.Y + StyleConf.ButtonHeaderHeight),
					Button.Width, Button.Height - StyleConf.ButtonHeaderHeight,
					PartialTime.ToSecondsString (), false, true);

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
	}
}

