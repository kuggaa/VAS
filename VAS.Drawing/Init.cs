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
using VAS.Core.MVVMC;
using VAS.Core.Store.Drawables;
using VAS.Drawing.CanvasObjects.Blackboard;

namespace VAS.Drawing
{
	public static class DrawingInit
	{
		public static void ScanViews ()
		{
			Scanner.ScanViews (App.Current.ViewLocator);
			// FIXME: ScanViews is Init in VAS-51
			CanvasFromDrawableObjectRegistry.AddMapping (typeof (Counter), typeof (CounterObject), "VAS.Drawing");
			CanvasFromDrawableObjectRegistry.AddMapping (typeof (Cross), typeof (CrossObject), "VAS.Drawing");
			CanvasFromDrawableObjectRegistry.AddMapping (typeof (Ellipse), typeof (EllipseObject), "VAS.Drawing");
			CanvasFromDrawableObjectRegistry.AddMapping (typeof (Line), typeof (LineObject), "VAS.Drawing");
			CanvasFromDrawableObjectRegistry.AddMapping (typeof (Quadrilateral), typeof (QuadrilateralObject), "VAS.Drawing");
			CanvasFromDrawableObjectRegistry.AddMapping (typeof (Rectangle), typeof (RectangleObject), "VAS.Drawing");
			CanvasFromDrawableObjectRegistry.AddMapping (typeof (Text), typeof (TextObject), "VAS.Drawing");
		}
	}
}
