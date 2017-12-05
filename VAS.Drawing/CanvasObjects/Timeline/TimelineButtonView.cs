//
//  Copyright (C) 2016 Fluendo S.A.
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
using VAS.Core.Common;
using VAS.Core.Resources.Styles;
using VAS.Core.Interfaces.Drawing;
using VAS.Core.Store.Drawables;
using static VAS.Core.Resources.Styles.Themes;

namespace VAS.Drawing.CanvasObjects.Timeline
{
	/// <summary>
	/// Draws buttons in the timeline using the style ButtonTimeline in gtkrc
	/// </summary>
	public class ThemeButtonView : ButtonView
	{
		bool insensitive;

		public ThemeButtonView (ButtonStyle style)
		{
			BackgroundColor = App.Current.Style.ThemeContrastDisabled;
			Style = style;
			insensitive = false;
			DrawsSelectionArea = false;
		}

		protected override void DisposeManagedResources ()
		{
			base.DisposeManagedResources ();
			ResetBackbuffer ();
		}

		public ButtonStyle Style {
			set {
				string normal = null, active = null, insensitive = null, hightlighted = null;

				switch (value) {
				case ButtonStyle.Timeline:
					normal = Themes.NormalButtonNormalTheme;
					active = Themes.NormalButtonActiveTheme;
					insensitive = Themes.NormalButtonInsensititveTheme;
					hightlighted = Themes.NormalButtonPrelightTheme;
					Width = App.Current.Style.ButtonTimelineWidth;
					Height = App.Current.Style.ButtonTimelineHeight;
					IconPadding = 0;
					break;
				case ButtonStyle.Circular:
					Width = App.Current.Style.ButtonNormalWidth;
					Height = App.Current.Style.ButtonNormalHeight;
					Circular = true;
					break;
				case ButtonStyle.Normal:
				default:
					normal = Themes.TimelineButtonNormalTheme;
					active = Themes.TimelineButtonActiveTheme;
					insensitive = Themes.TimelineButtonInsensititveTheme;
					hightlighted = Themes.TimelineButtonPrelightTheme;
					Width = App.Current.Style.ButtonNormalWidth;
					Height = App.Current.Style.ButtonNormalHeight;
					break;
				}
				if (normal != null) {
					BackgroundImage = App.Current.ResourcesLocator.LoadImage (normal);
				}
				if (active != null) {
					BackgroundImageActive = App.Current.ResourcesLocator.LoadImage (active);
				}
				if (insensitive != null) {
					BackgroundImageInsensitive = App.Current.ResourcesLocator.LoadImage (insensitive);
				}
				if (hightlighted != null) {
					BackgroundImageHighlighted = App.Current.ResourcesLocator.LoadImage (hightlighted);
				}
			}
		}

		/// <summary>
		/// Gets or sets the background image for the normal state.
		/// </summary>
		/// <value>The background image.</value>
		public Image BackgroundImage { get; set; }

		/// <summary>
		/// Gets or sets the background image insensitive.
		/// </summary>
		/// <value>The background image insensitive.</value>
		public Image BackgroundImageInsensitive { get; set; }

		/// <summary>
		/// Gets or sets the background image highlighted (Prelight).
		/// </summary>
		/// <value>The background image highlighted.</value>
		public Image BackgroundImageHighlighted { get; set; }

		/// <summary>
		/// Gets or sets the background image insensitive.
		/// </summary>
		/// <value>The background image insensitive.</value>
		public Image BackgroundImageActive { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="T:VAS.Drawing.CanvasObjects.Timeline.TimelineButtonView"/>
		/// is insensitive.
		/// </summary>
		/// <value><c>true</c> if insensitive; otherwise, <c>false</c>.</value>
		public bool Insensitive {
			get {
				return insensitive;
			}
			set {
				bool changed = insensitive != value;
				insensitive = value;
				if (changed) {
					ReDraw ();
				}
			}
		}

		public override void ClickPressed (Point p, ButtonModifier modif, Selection selection)
		{
			if (!insensitive) {
				base.ClickPressed (p, modif, selection);
				ReDraw ();
			}
		}

		protected override void DrawButton (IDrawingToolkit tk)
		{
			tk.FillColor = BackgroundColor;
			tk.DrawRectangle (Position, Width, Height);

			if (Active && BackgroundImageActive != null) {
				tk.DrawImage (Position, Width, Height, BackgroundImageActive,
					ScaleMode.AspectFit);
			} else if (Insensitive && BackgroundImageInsensitive != null) {
				tk.DrawImage (Position, Width, Height, BackgroundImageInsensitive,
					ScaleMode.AspectFit);
			} else if (Highlighted && BackgroundImageHighlighted != null) {
				tk.DrawImage (Position, Width, Height, BackgroundImageHighlighted,
					ScaleMode.AspectFit);
			} else if (BackgroundImage != null) {
				tk.DrawImage (Position, Width, Height, BackgroundImage,
					ScaleMode.AspectFit);
			}
		}
	}
}
