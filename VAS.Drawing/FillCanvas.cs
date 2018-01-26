//
//  Copyright (C) 2018 Fluendo S.A.
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
using System.Linq;
using VAS.Core.Common;
using VAS.Core.Interfaces.Drawing;

namespace VAS.Drawing
{
	/// <summary>
	// A Canvas that layouts its children to fill and expand to the size of the canvas.
	// It should be used with a single child that will be allocated to take all the space of the widget.
	/// </summary>
	public class FillCanvas : Canvas
	{
		public FillCanvas (IWidget widget) : base (widget)
		{
		}

		protected override void HandleSizeChangedEvent ()
		{
			base.HandleSizeChangedEvent ();
			foreach (var obj in this.Objects.OfType<FixedSizeCanvasObject> ()) {
				obj.Position = new Point (0, 0);
				obj.Width = widget.Width;
				obj.Height = widget.Height;
			}
		}
	}
}
