//
//  Copyright (C) 2017 FLUENDO S.A.
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
using VAS.Core.Interfaces.Drawing;
using VAS.Core.Interfaces.MVVMC;
using VAS.Drawing;
using VAS.Drawing.Cairo;
using VAS.Drawing.CanvasObjects;
using Point = VAS.Core.Common.Point;

namespace VAS.UI.Component
{
	public class CanvasObjectCellRenderer<TView, TViewModel> : CellRenderer
		where TView : ICanvasObjectView<TViewModel>
		where TViewModel : IViewModel
	{
		TViewModel viewModel;
		TView view;

		public CanvasObjectCellRenderer ()
		{
			view = (TView)Activator.CreateInstance (typeof (TView));
		}

		public TViewModel ViewModel {
			get { return viewModel; }
			set {
				viewModel = value;
				view.SetViewModel (viewModel);
			}
		}

		public override void GetSize (Widget widget, ref Gdk.Rectangle cell_area, out int x_offset, out int y_offset, out int width, out int height)
		{
			base.GetSize (widget, ref cell_area, out x_offset, out y_offset, out width, out height);
			width = Width;
			height = Height;
		}

		protected override void Render (Drawable window, Widget widget, Rectangle background_area,
								Rectangle cell_area, Rectangle expose_area, CellRendererState flags)
		{
			var tk = App.Current.DrawingToolkit;
			using (IContext context = new CairoContext (window)) {
				tk.Context = context;

				if (viewModel != null) {

					var fixedCanvas = view as FixedSizeCanvasObject;
					if (fixedCanvas != null) {
						fixedCanvas.Width = background_area.Width;
						fixedCanvas.Height = background_area.Height;
						fixedCanvas.Position = new Point (background_area.X, background_area.Y);
						fixedCanvas?.Draw (tk, new Area (new Point (background_area.X, background_area.Y), background_area.Width, background_area.Height));

					} else {
						view?.Draw (tk, new Area (new Point (background_area.X, background_area.Y), background_area.Width, background_area.Height));
					}

				}

				tk.Context = null;
			}
		}
	}
}
