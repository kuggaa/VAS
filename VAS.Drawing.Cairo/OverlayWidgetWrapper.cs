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
using Gtk;

namespace VAS.Drawing.Cairo
{
	/// <summary>
	/// An IWidget implementation to be used in container widgets such as bins or event boxes that need to perform
	/// a custom background drawing and chain the expose the children.
	/// </summary>
	public class OverlayWidgetWrapper : WidgetWrapper
	{
		public OverlayWidgetWrapper (Widget widget) : base (widget)
		{
		}

		/// <summary>
		/// Force a draw operation on the widget. This function should only be called in the Expose's event before
		/// propagating the event to the children.
		/// </summary>
		public void ForceDraw ()
		{
			Draw (null);
		}
	}
}