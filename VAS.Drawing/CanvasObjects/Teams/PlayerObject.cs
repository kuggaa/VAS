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
using VAS.Core.Common;
using VAS.Core.Interfaces.Drawing;
using VAS.Core.Store;
using VAS.Core.Store.Drawables;
using VAS.Drawing.CanvasObjects;

namespace VAS.Drawing.CanvasObjects.Teams
{
	public class PlayerObject: CanvasButtonObject, ICanvasSelectableObject
	{
		protected static ISurface DefaultPhoto;
		static bool surfacesCached = false;
		Player player;

		public PlayerObject ()
		{
			Init ();
		}

		public PlayerObject (Player player, Point position = null)
		{
			Player = player;
			Init (position);
		}

		public Player Player {
			get {
				return player;
			}
			set {
				if (player != null) {
					player.PropertyChanged -= HandlePropertyChanged;
				}
				player = value;
				if (player != null) {
					player.PropertyChanged += HandlePropertyChanged;
				}
			}
		}

		public int Size {
			set;
			get;
		}

		public bool DrawPhoto {
			get;
			set;
		}

		Color Color {
			get {
				return Player.Color;
			}
		}

		public override bool Active {
			get {
				return player.Tagged;
			}
			set {
				player.Tagged = value;
			}
		}

		public virtual void LoadSurfaces ()
		{
			if (!surfacesCached) {
				DefaultPhoto = App.Current.DrawingToolkit.CreateSurfaceFromResource (StyleConf.PlayerPhoto, false);
				surfacesCached = true;
			}
		}

		public virtual Selection GetSelection (Point point, double precision, bool inMotion = false)
		{
			if (point.X >= Position.X && point.X <= Position.X + Width) {
				if (point.Y >= Position.Y && point.Y <= Position.Y + Height) {
					return new Selection (this, SelectionPosition.All, 0);
				}
			}
			return null;
		}

		public void Move (Selection sel, Point p, Point start)
		{
		}

		public override void Draw (IDrawingToolkit tk, Area area)
		{
			Point zero, start, p;
			double size, scale;
			ISurface arrowin, arrowout;

			if (Player == null)
				return;

			zero = new Point (0, 0);
			size = StyleConf.PlayerSize;
			scale = (double)Width / size;


			tk.Begin ();
			start = new Point (Size / 2, Size / 2);
			tk.TranslateAndScale (Center - start, new Point (scale, scale));

			if (!UpdateDrawArea (tk, area, new Area (zero, size, size))) {
				tk.End ();
				return;
			}

			/* Background */
			tk.FillColor = App.Current.Style.PaletteBackgroundDark;
			tk.LineWidth = 0;
			tk.DrawRectangle (zero, StyleConf.PlayerSize, StyleConf.PlayerSize);
			
			/* Image */
			if (Player.Photo != null) {
				tk.DrawImage (zero, size, size, Player.Photo, ScaleMode.AspectFit);
			} else {
				tk.DrawSurface (zero, StyleConf.PlayerSize, StyleConf.PlayerSize, DefaultPhoto, ScaleMode.AspectFit);
			}

			/* Bottom line */
			p = new Point (0, size - StyleConf.PlayerLineWidth);
			tk.FillColor = Color;
			tk.DrawRectangle (p, size, 3);

			if (Active) {
				Color c = Color.Copy ();
				c.A = (byte)(c.A * 60 / 100);
				tk.FillColor = c;
				tk.DrawRectangle (zero, size, size);
			}
			
			tk.End ();
		}

		void Init (Point pos = null)
		{
			if (pos == null) {
				pos = new Point (0, 0);
			}
			Position = pos;
			DrawPhoto = true;
			Size = (int)PlayersIconSize.Medium;
			Toggle = true;
			LoadSurfaces ();
		}

		void HandlePropertyChanged (object sender, PropertyChangedEventArgs args)
		{
			ReDraw ();
		}
	}
}

