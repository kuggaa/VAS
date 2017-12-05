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
using static VAS.Core.Resources.Styles.Colors;

namespace VAS.Drawing.CanvasObjects
{
	public class ButtonView : FixedSizeCanvasObject, ICanvasSelectableObject
	{
		const int DEFAULT_ICON_PADDING = 5;
		const int SELECTION_SIZE = 6;
		protected ISurface backBufferSurface;
		bool active;
		bool clicked;

		public ButtonView ()
		{
			BackgroundColor = App.Current.Style.ThemeContrastDisabled;
			BackgroundColorActive = App.Current.Style.ThemeContrastDisabled;
			BorderColor = App.Current.Style.ThemeBase;
			TextColor = App.Current.Style.TextBase;
			UseBackBufferSurface = true;
			IconPadding = DEFAULT_ICON_PADDING;
			MinWidth = 20;
			MinHeight = 20;
		}

		protected override void DisposeManagedResources ()
		{
			base.DisposeManagedResources ();
			ResetBackbuffer ();
		}

		/// <summary>
		/// Gets or sets the minimum width to which the button can be resized.
		/// </summary>
		/// <value>The minimum width.</value>
		public int MinWidth { get; set; }

		/// <summary>
		/// Gets or sets the minimum height to which the button can be resized.
		/// </summary>
		/// <value>The minimum height.</value>
		public int MinHeight { get; set; }

		/// <summary>
		/// Gets or sets the text of the button.
		/// </summary>
		/// <value>The text.</value>
		public virtual string Text { get; set; }

		/// <summary>
		/// Gets or sets the icon of the button.
		/// </summary>
		/// <value>The icon.</value>
		public virtual Image Icon { get; set; }

		/// <summary>
		/// Gets or sets the icon of the button.
		/// </summary>
		/// <value>The icon.</value>
		public virtual Image IconInactive { get; set; }

		/// <summary>
		/// Gets or sets the icon padding.
		/// </summary>
		/// <value>The icon padding.</value>
		public virtual int IconPadding { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="T:VAS.Drawing.CanvasObjects.ButtonObject"/> draws
		/// the selection area when it's selected.
		/// </summary>
		/// <value><c>true</c> if draws selection area; otherwise, <c>false</c>.</value>
		public virtual bool DrawsSelectionArea { get; set; }

		/// <summary>
		/// Gets or sets the color of the mask.
		/// </summary>
		/// <value>The color of the mask.</value>
		public Color MaskColor { get; set; }

		/// <summary>
		/// Gets or sets the color of the border.
		/// </summary>
		/// <value>The color of the border.</value>
		public virtual Color BorderColor { get; set; }

		/// <summary>
		/// Gets or sets the color of the background.
		/// </summary>
		/// <value>The color of the background.</value>
		public virtual Color BackgroundColor { get; set; }

		/// <summary>
		/// Gets or sets the color of the background when its active.
		/// </summary>
		/// <value>The background color active.</value>
		public virtual Color BackgroundColorActive { get; set; }

		/// <summary>
		/// Gets or sets the color of the text.
		/// </summary>
		/// <value>The color of the text.</value>
		public virtual Color TextColor { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="T:VAS.Drawing.CanvasObjects.ButtonObject"/> has
		/// a circular shape.
		/// </summary>
		/// <value><c>true</c> if circular; otherwise, <c>false</c>.</value>
		public bool Circular { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="T:VAS.Drawing.CanvasObjects.CanvasButtonObject"/> is of type Toggle.
		/// </summary>
		/// <value><c>true</c> if toggle; otherwise, <c>false</c>.</value>
		public bool Toggle { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="T:VAS.Drawing.CanvasObjects.CanvasButtonObject"/> is active.
		/// </summary>
		/// <value><c>true</c> if active; otherwise, <c>false</c>.</value>
		public virtual bool Active {
			get {
				return active;
			}
			set {
				bool changed = active != value;
				active = value;
				if (changed) {
					ReDraw ();
				}
			}
		}

		public virtual Area Area => new Area (Position, Width + SelectionSize / 2 + 1, Height + SelectionSize / 2 + 1);

		protected bool UseBackBufferSurface { get; set; }

		protected Color CurrentBackgroundColor => Active ? BackgroundColorActive : BackgroundColor;

		protected int SelectionSize => DrawsSelectionArea ? SELECTION_SIZE : 0;

		/// <summary>
		/// Forces a Click in this Button Object
		/// </summary>
		public void Click ()
		{
			Click (new Point (Position.X + 1, Position.Y + 1),
				ButtonModifier.None);
		}

		/// <summary>
		/// Forces a Click at a concrete Point position
		/// </summary>
		/// <param name="p">Position</param>
		/// <param name="modif">Button Modifier</param>
		public void Click (Point p, ButtonModifier modif)
		{
			if (IsClickInsideButton (p)) {
				ClickPressed (p, ButtonModifier.None, null);
				ClickReleased ();
			}
		}

		public override void ClickPressed (Point p, ButtonModifier modif, Selection selection)
		{
			if (IsClickInsideButton (p)) {
				Active = !Active;
				clicked = true;
			}
		}

		public override void ClickReleased ()
		{
			if (clicked) {
				if (!Toggle) {
					Active = !Active;
				}
				EmitClickEvent ();
				clicked = false;
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
			DrawBase (tk);
			DrawSelectionArea (tk);
			tk.End ();
		}

		protected void ResetBackbuffer ()
		{
			if (backBufferSurface != null) {
				backBufferSurface.Dispose ();
				backBufferSurface = null;
			}
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
		}

		protected virtual void DrawIcon (IDrawingToolkit tk)
		{
			Image icon;

			if (Active || IconInactive == null) {
				icon = Icon;
			} else {
				icon = IconInactive;
			}
			if (icon != null) {
				tk.FillColor = MaskColor;
				tk.DrawImage (new Point (Position.X + IconPadding, Position.Y + IconPadding),
				Width - IconPadding * 2, Height - IconPadding * 2, icon, ScaleMode.AspectFit,
				MaskColor != null);
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

		protected virtual void DrawBase (IDrawingToolkit tk)
		{
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
			tk.Begin ();
			if (Circular) {
				tk.ClipCircle (new Point (Position.X + Width / 2, Position.Y + Height / 2), Width);
			}
			if (UseBackBufferSurface) {
				tk.TranslateAndScale (new Point (-Position.X, -Position.Y),
						new Point (1, 1));
			}
			DrawButton (tk);
			DrawIcon (tk);
			DrawText (tk);
			DrawBase (tk);
			tk.End ();
		}

		bool IsClickInsideButton (Point p)
		{
			bool insideX = false;
			bool insideY = false;

			if (p.X >= Position.X && p.X <= Position.X + Width) {
				insideX = true;
			}
			if (p.Y >= Position.Y && p.Y <= Position.Y + Height) {
				insideY = true;
			}

			return insideX && insideY;
		}

	}
}

