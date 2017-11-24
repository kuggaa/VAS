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

using VAS.Core.Common;
using VAS.Core.Interfaces.Drawing;
using VAS.Core.Resources.Styles;
using VAS.Core.Store.Drawables;
using VAS.Drawing.CanvasObjects;
using VASDrawing = VAS.Drawing;

namespace VAS.Drawing.CanvasObjects
{
	public class ButtonObject : CanvasButtonObject, IMovableObject
	{
		const int BORDER_SIZE = 8;
		const int SELECTION_SIZE = 6;
		protected ISurface backBufferSurface;

		public ButtonObject ()
		{
			BackgroundColor = App.Current.Style.ThemeContrastDisabled;
			BackgroundColorActive = App.Current.Style.ThemeContrastDisabled;
			BorderColor = App.Current.Style.ThemeBase;
			TextColor = App.Current.Style.TextBase;
			UseBackBufferSurface = true;
			MinWidth = 20;
			MinHeight = 20;
		}

		protected override void DisposeManagedResources ()
		{
			base.DisposeManagedResources ();
			ResetBackbuffer ();
		}

		public virtual string Text {
			get;
			set;
		}

		public virtual Image Icon {
			get;
			set;
		}

		public virtual Color BorderColor {
			get;
			set;
		}

		public virtual Color BackgroundColor {
			get;
			set;
		}

		public virtual Color BackgroundColorActive {
			get;
			set;
		}

		public virtual Color TextColor {
			get;
			set;
		}

		public int MinWidth {
			get;
			set;
		}

		public int MinHeight {
			get;
			set;
		}

		protected Color CurrentBackgroundColor {
			get {
				if (!Active) {
					return BackgroundColor;
				} else {
					return BackgroundColorActive;
				}
			}
		}

		public virtual Image BackgroundImage {
			get;
			set;
		}

		public virtual Image BackgroundImageActive {
			get;
			set;
		}

		public virtual bool DrawsSelectionArea {
			get {
				return true;
			}
		}

		/// <summary>
		/// Gets a value indicating whether this <see cref="T:VAS.Drawing.CanvasObjects.ButtonObject"/> use back buffer surface.
		/// </summary>
		/// <value><c>true</c> if use back buffer surface; otherwise, <c>false</c>.</value>
		public bool UseBackBufferSurface {
			get;
			set;
		}

		public virtual Area Area {
			get {
				return new Area (Position, Width + SELECTION_SIZE / 2 + 1,
					Height + SELECTION_SIZE / 2 + 1);
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

		protected void ResetBackbuffer ()
		{
			if (backBufferSurface != null) {
				backBufferSurface.Dispose ();
				backBufferSurface = null;
			}
		}

		public virtual Selection GetSelection (Point p, double precision, bool inMotion = false)
		{
			Selection s;

			Rectangle r = new Rectangle (Position, Width, Height);
			s = r.GetSelection (p, precision);
			if (s != null) {
				s.Drawable = this;
				if (s.Position != SelectionPosition.BottomRight &&
					s.Position != SelectionPosition.Right &&
					s.Position != SelectionPosition.Bottom) {
					s.Position = SelectionPosition.All;
				}
			}
			return s;
		}

		public virtual void Move (Selection s, Point p, Point start)
		{
			switch (s.Position) {
			case SelectionPosition.Right:
				Width = (int)(p.X - Position.X);
				Width = (int)Math.Max (10, Width);
				break;
			case SelectionPosition.Bottom:
				Height = (int)(p.Y - Position.Y);
				Height = (int)Math.Max (10, Height);
				break;
			case SelectionPosition.BottomRight:
				Width = (int)(p.X - Position.X);
				Height = (int)(p.Y - Position.Y);
				Width = Math.Max (10, Width);
				Height = Math.Max (10, Height);
				break;
			case SelectionPosition.All:
				Position.X += p.X - start.X;
				Position.Y += p.Y - start.Y;
				Position.X = Math.Max (Position.X, 0);
				Position.Y = Math.Max (Position.Y, 0);
				break;
			default:
				throw new Exception ("Unsupported move for tagger object:  " + s.Position);
			}
			Width = Math.Max (MinWidth, Width);
			Height = Math.Max (MinHeight, Height);
			ResetBackbuffer ();
		}

		protected void DrawSelectionArea (IDrawingToolkit tk)
		{
			if (!Selected || !DrawsSelectionArea) {
				return;
			}

			tk.StrokeColor = App.Current.Style.DrawingSelectorShadow;
			tk.FillColor = null;
			tk.LineStyle = LineStyle.Dashed;
			tk.LineWidth = 2;
			tk.DrawRectangle (Position, Width, Height);

			tk.StrokeColor = tk.FillColor = App.Current.Style.DrawingSelectorAnchor;
			tk.LineStyle = LineStyle.Normal;
			tk.DrawRectangle (new Point (Position.X + Width - SELECTION_SIZE / 2,
				Position.Y + Height - SELECTION_SIZE / 2),
				SELECTION_SIZE, SELECTION_SIZE);
		}

		/// <summary>
		/// Draws the button.
		/// </summary>
		/// <param name="tk">Tk.</param>
		protected virtual void DrawButton (IDrawingToolkit tk)
		{
			Color front, back;

			if (Active) {
				tk.LineWidth = Sizes.ButtonLineWidth;
				front = BackgroundColor;
				back = BorderColor;
			} else {
				tk.LineWidth = 0;
				front = BorderColor;
				back = BackgroundColor;
			}
			tk.FillColor = back;
			tk.StrokeColor = front;
			tk.DrawRectangle (Position, Width, Height);
			if (Icon != null) {
				tk.FillColor = front;
				tk.DrawImage (new Point (Position.X + 5, Position.Y + 5),
							  Sizes.ButtonHeaderWidth, Sizes.ButtonHeaderHeight, Icon, ScaleMode.AspectFit, true);
			}
		}

		protected void DrawImage (IDrawingToolkit tk)
		{
			Point pos = new Point (Position.X + BORDER_SIZE / 2, Position.Y + BORDER_SIZE / 2);

			if (Active && BackgroundImageActive != null) {
				tk.DrawImage (pos, Width - BORDER_SIZE, Height - BORDER_SIZE, BackgroundImageActive,
					ScaleMode.AspectFit);
			} else if (BackgroundImage != null) {
				tk.DrawImage (pos, Width - BORDER_SIZE, Height - BORDER_SIZE, BackgroundImage,
					ScaleMode.AspectFit);
			}
		}

		/// <summary>
		/// Draws the text.
		/// </summary>
		/// <param name="tk">Tk.</param>
		protected virtual void DrawText (IDrawingToolkit tk)
		{
			if (Text != null) {
				if (Active) {
					tk.FillColor = BackgroundColor;
					tk.StrokeColor = BackgroundColor;
				} else {
					tk.FillColor = TextColor;
					tk.StrokeColor = TextColor;
				}
				tk.FontSize = Sizes.ButtonNameFontSize;
				tk.FontWeight = FontWeight.Light;
				tk.FontAlignment = FontAlignment.Center;
				tk.DrawText (Position, Width, Height, Text);
			}
		}

		void CreateBackBufferSurface ()
		{
			IDrawingToolkit tk = App.Current.DrawingToolkit;

			ResetBackbuffer ();
			backBufferSurface = tk.CreateSurface ((int)Width, (int)Height);
			using (IContext c = backBufferSurface.Context) {
				tk.Context = c;
				DrawBackBuffer (tk);
			}
		}

		/// <summary>
		/// Draws the back buffer depending on the property UseBackBufferSurface
		/// </summary>
		/// <param name="tk">Tk.</param>
		void DrawBackBuffer (IDrawingToolkit tk)
		{
			if (UseBackBufferSurface) {
				tk.TranslateAndScale (new Point (-Position.X, -Position.Y),
						new Point (1, 1));
			}
			DrawButton (tk);
			DrawImage (tk);
			DrawText (tk);
		}

		public override void Draw (IDrawingToolkit tk, Area area)
		{
			IContext ctx = tk.Context;

			if (!UpdateDrawArea (tk, area, Area)) {
				return;
			}
			if (UseBackBufferSurface && backBufferSurface == null) {
				CreateBackBufferSurface ();
			}
			tk.Context = ctx;
			tk.Begin ();
			if (UseBackBufferSurface) {
				tk.DrawSurface (backBufferSurface, Position);
			} else {
				DrawBackBuffer (tk);
			}
			DrawSelectionArea (tk);
			tk.End ();
		}
	}
}

