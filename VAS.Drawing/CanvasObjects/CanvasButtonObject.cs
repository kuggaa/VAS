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

namespace VAS.Drawing.CanvasObjects
{
	/// <summary>
	/// Abstract class for managing click logic in Canvas Button objects.
	/// </summary>
	public abstract class CanvasButtonObject : FixedSizeCanvasObject
	{
		bool active;
		bool clicked;

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="T:VAS.Drawing.CanvasObjects.CanvasButtonObject"/> is of type Toggle.
		/// </summary>
		/// <value><c>true</c> if toggle; otherwise, <c>false</c>.</value>
		public bool Toggle {
			get;
			set;
		}

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
				ClickPressed (p, ButtonModifier.None);
				ClickReleased ();
			}
		}

		public override void ClickPressed (Point p, ButtonModifier modif)
		{
			if (IsClickInsideButton (p)) {
				if (!Toggle) {
					Active = !Active;
				}
				clicked = true;
			}
		}

		public override void ClickReleased ()
		{
			if (clicked) {
				Active = !Active;
				EmitClickEvent ();
				clicked = false;
			}
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