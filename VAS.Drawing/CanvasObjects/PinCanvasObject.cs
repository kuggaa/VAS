﻿//
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
using VAS.Core.MVVMC;
using VAS.Core.Store.Drawables;
using VAS.Core.ViewModel;

namespace VAS.Drawing.CanvasObjects
{
	public class PinCanvasObject : FixedSizeCanvasObject, ICanvasSelectableObject, ICanvasObjectView<PointVM>
	{
		static Image missingLocationPinImage;
		static Image locationPinImageMoving;
		static Image locationPinImageSet;

		bool moving;
		bool modified;

		static PinCanvasObject ()
		{
			missingLocationPinImage = Resources.LoadImage (StyleConf.LocationPinNotSet);
			locationPinImageMoving = Resources.LoadImage (StyleConf.LocationPinMoving);
			locationPinImageSet = Resources.LoadImage (StyleConf.LocationSet);
		}

		public PinCanvasObject ()
		{
			Width = missingLocationPinImage.Width;
			Height = missingLocationPinImage.Height;
			ViewModel = null;
		}

		/// <summary>
		/// ViewModel for the Point being modified
		/// </summary>
		/// <value>The view model.</value>
		public PointVM ViewModel { get; set; }

		public override Point Position {
			get {
				return base.Position;
			}
			set {
				base.Position = value;
				if (ViewModel != null) {
					ViewModel.Model = value;
				}
			}
		}

		public Selection GetSelection (Point point, double precision, bool inMotion = false)
		{
			// Always select the pin, regardless of where it's clicked.
			return new Selection (this, SelectionPosition.All, 0);
		}

		public void Move (Selection s, Point dst, Point start)
		{
			Position = dst;
		}

		public override void Draw (IDrawingToolkit tk, Area area)
		{
			if (!Visible) {
				return;
			}
			if (Position == null || ViewModel == null) {
				return;
			}

			if (!modified) {
				tk.DrawImage (Position - new Point (missingLocationPinImage.Width / 2, missingLocationPinImage.Height / 2),
							  missingLocationPinImage.Width, missingLocationPinImage.Height,
							  missingLocationPinImage, ScaleMode.AspectFit);
			} else {
				if (moving) {
					Point p = Position - new Point (locationPinImageMoving.Width / 2, locationPinImageMoving.Height);
					p.Y += locationPinImageSet.Height / 2;
					tk.DrawImage (p,
							  locationPinImageMoving.Width, locationPinImageMoving.Height,
							  locationPinImageMoving, ScaleMode.AspectFit);
				} else {
					tk.DrawImage (Position - new Point (locationPinImageSet.Width / 2, locationPinImageSet.Height / 2),
								  locationPinImageSet.Width, locationPinImageSet.Height,
								  locationPinImageSet, ScaleMode.AspectFit);
				}
			}
		}

		public override void ClickPressed (Point p, ButtonModifier modif)
		{
			base.ClickPressed (p, modif);
			Position = p;
			moving = true;
			modified = true;
		}

		public override void ClickReleased ()
		{
			base.ClickReleased ();
			moving = false;
			ReDraw ();
		}

		public void SetViewModel (object viewModel)
		{
			ViewModel = (PointVM)viewModel;
		}
	}
}
