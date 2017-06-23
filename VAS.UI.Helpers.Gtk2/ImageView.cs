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
using Gdk;
using Gtk;
using VAS.Core.Common;
using Image = VAS.Core.Common.Image;
using Point = VAS.Core.Common.Point;

namespace VAS.UI.Helpers
{
	/// <summary>
	/// A widget to draw <see cref="Image"/> that replaces <see cref="Gtk.Image"/>. This widget allows the user to
	/// define the /// desired size of the <see cref="Widget"/> regardless of the size of the <see cref="Image"/>.
	/// The <see cref="Image"/> is scalled acordingly to respect DAR or centered the without scalling of its
	/// smaller than the allocated space.
	/// </summary>
	[System.ComponentModel.ToolboxItem (true)]
	public class ImageView : SkiaDrawingArea
	{
		Image image;

		public ImageView ()
		{
		}

		public ImageView (Image image) : this ()
		{
			this.image = image;
		}

		public Image Image {
			get { return image; }
			set {
				image = value;
				QueueDraw ();
				QueueResize ();
			}
		}

		/// <summary>
		/// Sets the size of the widget regardless of the <see cref="Image"/> size and queues a resize to
		/// force a new size request.
		/// </summary>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		public void SetSize (int width, int height)
		{
			WidthRequest = width;
			HeightRequest = height;
			QueueResize ();
		}

		/// <summary>
		/// Sets the size of the widget regardless of the <see cref="Image"/> size and queues a resize to
		/// force a new size request.
		/// </summary>
		/// <param name="size">Size of the widget.</param>
		public void SetSize (int size)
		{
			SetSize (size, size);
		}

		protected override void OnSizeRequested (ref Requisition requisition)
		{
			requisition.Width = Xpad * 2;
			requisition.Height = Ypad * 2;
			// If both WidthRequest and HeightRequest are set, we use them to determine the size of the widget.
			if (WidthRequest > 0 && HeightRequest > 0) {
				requisition.Width += WidthRequest;
				requisition.Height += HeightRequest;
			}
			// If not, we use the size of the image to determine the size of the widget
			else if (image != null) {
				requisition.Width += image.Width;
				requisition.Height += image.Height;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the parent of this <see cref="ImageView"/> is unisensitive
		/// </summary>
		/// <value><c>true</c> if is parent unsensitive; otherwise, <c>false</c>.</value>
		bool IsParentUnsensitive {
			get {
				var parent = Parent;
				if (parent != null) {
					if (!parent.Sensitive)
						return true;
					// Handle correctly image in buttons
					parent = parent.Parent.Parent as Button;
					if (parent != null && !parent.Sensitive)
						return true;
				}
				return false;
			}
		}

		protected override bool OnExposeEvent (EventExpose evnt)
		{
			if (image != null) {
				var alloc = Allocation;
				alloc.Inflate (-Xpad, -Ypad);
				using (var ctx = App.Current.DrawingToolkit.CreateContextFromNativeWindow (evnt.Window)) {
					var width = WidthRequest != -1 ? WidthRequest : image.Width;
					var height = HeightRequest != -1 ? HeightRequest : image.Height;
					var point = new Point (alloc.X, alloc.Y);

					// If the image is smaller than the allocated size, center it without scalling it
					if (alloc.Width > width && alloc.Height > height) {
						point.X += (alloc.Width - width) / 2;
						point.Y += (alloc.Height - height) / 2;
					}
					// Otherwise use the whole allocated space and let DrawImage scale correctly to keep DAR
					else {
						width = alloc.Width;
						height = alloc.Height;
					}

					App.Current.DrawingToolkit.Context = ctx;
					var r = evnt.Area;
					App.Current.DrawingToolkit.Clip (new Area (new Point (r.X, r.Y), r.Width, r.Height));
					var alpha = IsParentUnsensitive ? 0.4f : 1f;
					App.Current.DrawingToolkit.DrawImage (point, width, height, image, ScaleMode.AspectFit, alpha: alpha);
				}
			}
			return true;
		}
	}
}
