//
//  Copyright (C) 2017 Fluendo S.A.
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
using VAS.Drawing.CanvasObjects;

namespace VAS.Drawing.CanvasObjects
{
	public class PinCanvasObject : FixedSizeCanvasObject, ICanvasSelectableObject
	{
		static Image missingLocationPinImage;
		static Image locationPinImage;
		static Image locationImage;

		static PinCanvasObject ()
		{
			missingLocationPinImage = Resources.LoadImage (StyleConf.LocationPinNotSet);
			locationPinImage = Resources.LoadImage (StyleConf.LocationPinMoving);
			locationImage = Resources.LoadImage (StyleConf.LocationSet);
		}

		public PinCanvasObject ()
		{
			MapLocation = null;
		}

		public Point MapLocation {
			get;
			set;
		}

		public Selection GetSelection (Point point, double precision, bool inMotion = false)
		{
			throw new NotImplementedException ();
		}

		public void Move (Selection s, Point dst, Point start)
		{
			throw new NotImplementedException ();
		}

		public override void Draw (IDrawingToolkit tk, Area area)
		{
			throw new NotImplementedException ();
		}
	}
}
