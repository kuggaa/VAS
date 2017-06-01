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
using System.Linq.Expressions;
using VAS.Core.Common;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;

namespace VAS.UI.Helpers.Bindings
{
	/// <summary>
	/// Property binding for images.
	/// </summary>
	public class ImageBinding : PropertyBinding<Image>
	{
		Gtk.Image image;
		int width, height;

		public ImageBinding (Gtk.Image image, Expression<Func<IViewModel, Image>> propertyExpression,
							 int width = 0, int height = 0) : base (propertyExpression)
		{
			this.image = image;
			this.width = width;
			this.height = height;
		}

		protected override void BindView ()
		{
		}

		protected override void UnbindView ()
		{
		}

		protected override void WriteViewValue (Image val)
		{
			if (width != 0 && height != 0) {
				image.Pixbuf = val?.Scale (width, height).Value;
			} else {
				image.Pixbuf = val?.Value;
			}
		}
	}
}
