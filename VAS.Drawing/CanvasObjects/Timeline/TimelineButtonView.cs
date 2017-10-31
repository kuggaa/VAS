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
using System;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Interfaces.Drawing;
using VAS.Core.Store.Drawables;

namespace VAS.Drawing.CanvasObjects.Timeline
{
	/// <summary>
	/// Draws buttons in the timeline using the style ButtonTimeline in gtkrc
	/// </summary>
	public class TimelineButtonView : CanvasButtonObject, ICanvasSelectableObject
	{
		protected ISurface backBufferSurface;
		bool insensitive;

		public TimelineButtonView ()
		{
			BackgroundColor = App.Current.Style.ThemeContrastDisabled;
			BackgroundImage = App.Current.ResourcesLocator.LoadImage (StyleConf.TimelineButtonNormalTheme);
			BackgroundImageActive = App.Current.ResourcesLocator.LoadImage (StyleConf.TimelineButtonActiveTheme);
			BackgroundImageInsensitive = App.Current.ResourcesLocator.LoadImage (StyleConf.TimelineButtonInsensititveTheme);
			BackgroundImageHighlighted = App.Current.ResourcesLocator.LoadImage (StyleConf.TimelineButtonPrelightTheme);
			Width = App.Current.Style.ButtonTimelineWidth;
			Height = App.Current.Style.ButtonTimelineHeight;
			IconWidth = App.Current.Style.IconXSmallWidth;
			IconHeight = App.Current.Style.IconXSmallHeight;
			insensitive = false;
		}

		protected override void DisposeManagedResources ()
		{
			base.DisposeManagedResources ();
			ResetBackbuffer ();
		}

		/// <summary>
		/// Gets or sets the color of the background.
		/// </summary>
		/// <value>The color of the background.</value>
		public Color BackgroundColor {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the background image
		/// </summary>
		/// <value>The background image.</value>
		public Image BackgroundImage {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the background image active.
		/// </summary>
		/// <value>The background image active.</value>
		public Image BackgroundImageActive {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the background image insensitive.
		/// </summary>
		/// <value>The background image insensitive.</value>
		public Image BackgroundImageInsensitive {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the background image highlighted (Prelight).
		/// </summary>
		/// <value>The background image highlighted.</value>
		public Image BackgroundImageHighlighted {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the icon.
		/// </summary>
		/// <value>The icon.</value>
		public Image Icon {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the width of the icon.
		/// </summary>
		/// <value>The width of the icon.</value>
		public int IconWidth {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the height of the icon.
		/// </summary>
		/// <value>The height of the icon.</value>
		public int IconHeight {
			get;
			set;
		}

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

		public override void ReDraw ()
		{
			ResetBackbuffer ();
			base.ReDraw ();
		}

		public override void ResetDrawArea ()
		{
			ResetBackbuffer ();
			base.ResetDrawArea ();
		}

		public override void ClickPressed (Point p, ButtonModifier modif, Selection selection)
		{
			if (!insensitive) {
				base.ClickPressed (p, modif, selection);
				ReDraw ();
			}
		}

		public override void Draw (IDrawingToolkit tk, Area area)
		{
			IContext ctx = tk.Context;

			if (backBufferSurface == null) {
				CreateBackBufferSurface ();
			}
			tk.Context = ctx;
			tk.Begin ();
			tk.DrawSurface (backBufferSurface, Position);
			tk.End ();
		}

		public Selection GetSelection (Point point, double precision, bool inMotion = false)
		{
			if (point.X >= Position.X && point.X <= Position.X + Width) {
				if (point.Y >= Position.Y && point.Y <= Position.Y + Height) {
					return new Selection (this, SelectionPosition.All, 0);
				}
			}
			return null;
		}

		public void Move (Selection s, Point dst, Point start)
		{
		}

		protected void ResetBackbuffer ()
		{
			if (backBufferSurface != null) {
				backBufferSurface.Dispose ();
				backBufferSurface = null;
			}
		}

		protected void DrawIcon (IDrawingToolkit tk)
		{
			if (Icon != null) {
				tk.DrawImage (new Point (Position.X + (Width - IconWidth) / 2, Position.Y + (Height - IconHeight) / 2),
							  IconWidth, IconHeight, Icon, ScaleMode.AspectFit);
			}
		}

		protected void DrawBackground (IDrawingToolkit tk)
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

		void CreateBackBufferSurface ()
		{
			IDrawingToolkit tk = App.Current.DrawingToolkit;

			ResetBackbuffer ();
			backBufferSurface = tk.CreateSurface ((int)Width, (int)Height);
			using (IContext c = backBufferSurface.Context) {
				tk.Context = c;
				tk.TranslateAndScale (new Point (-Position.X, -Position.Y),
					new Point (1, 1));
				DrawBackground (tk);
				DrawIcon (tk);

			}
		}
	}
}
