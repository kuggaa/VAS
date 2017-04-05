//
//  Copyright (C) 2015 Fluendo S.A.
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
using VAS.Core.Interfaces.Drawing;
using VAS.Core.Store;
using VAS.Core.Store.Drawables;

namespace VAS.Drawing.CanvasObjects.Dashboard
{
	/// <summary>
	/// Represents an <see cref="ActionLink"/> in the canvas.
	/// </summary>
	public class ActionLinkView : CanvasObject, ICanvasSelectableObject
	{
		readonly Line line;
		const int selectionSize = 4;
		Point stop;

		public ActionLinkView (LinkAnchorView source,
								 LinkAnchorView destination,
								 ActionLink link)
		{
			Link = link;
			Source = source;
			Destination = destination;
			if (destination == null) {
				stop = source.Out;
			} else {
				stop = destination.In;
			}
			line = new Line ();
			line.Start = source.Out;
			line.Stop = stop;
		}

		/// <summary>
		/// Gets or sets the source.
		/// </summary>
		/// <value>The source.</value>
		public LinkAnchorView Source {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the link.
		/// </summary>
		/// <value>The link.</value>
		public ActionLink Link {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the destination.
		/// </summary>
		/// <value>The destination.</value>
		public LinkAnchorView Destination {
			get;
			set;
		}

		/// <summary>
		/// Gets the area.
		/// </summary>
		/// <value>The area.</value>
		public virtual Area Area {
			get {
				line.Start = Source.Out;
				if (Destination != null) {
					line.Stop = Destination.In;
				} else {
					line.Stop = stop;
				}
				Area area = line.Area;
				area.Start.X -= selectionSize + 2;
				area.Start.Y -= selectionSize + 2;
				area.Width += selectionSize * 2 + 4;
				area.Height += selectionSize * 2 + 4;
				return area;
			}
		}

		/// <summary>
		/// Gets the selection.
		/// </summary>
		/// <returns>The selection.</returns>
		/// <param name="point">Point.</param>
		/// <param name="precision">Precision.</param>
		/// <param name="inMotion">If set to <c>true</c> in motion.</param>
		public Selection GetSelection (Point point, double precision, bool inMotion = false)
		{
			Selection sel = line.GetSelection (point, precision, inMotion);
			if (sel != null) {
				sel.Drawable = this;
			}
			return sel;
		}

		/// <summary>
		/// Move the specified selection.
		/// </summary>
		/// <param name="s">S.</param>
		/// <param name="dst">Dst.</param>
		/// <param name="start">Start.</param>
		public void Move (Selection s, Point dst, Point start)
		{
			line.Move (s, dst, start);
			stop = line.Stop;
		}

		/// <summary>
		/// Cans the link.
		/// </summary>
		/// <returns><c>true</c>, if link was caned, <c>false</c> otherwise.</returns>
		/// <param name="dest">Destination.</param>
		public bool CanLink (LinkAnchorView dest)
		{
			/* Check if the link is possible between the 2 types of anchors */
			if (!Source.CanLink (dest)) {
				return false;
			}

			// FIXME: View using Model
			/* Check if this link will result into a duplicated link */
			foreach (ActionLink link in Source.Button.ButtonVM.Model.ActionLinks) {
				if (link.DestinationButton == dest.Button.ButtonVM.Model &&
					link.SourceTags.SequenceEqualSafe (Source.Tags) &&
					link.DestinationTags.SequenceEqualSafe (dest.Tags)) {
					return false;
				}
			}
			return true;
		}

		/// <summary>
		/// Draw the specified tk and area.
		/// </summary>
		/// <param name="tk">Tk.</param>
		/// <param name="area">Area.</param>
		public override void Draw (IDrawingToolkit tk, Area area)
		{
			Color lineColor;
			int lineWidth = 4;

			if (!UpdateDrawArea (tk, area, Area)) {
				return;
			}

			if (Selected) {
				lineColor = StyleConf.ActionLinkSelected;
			} else if (Highlighted) {
				lineColor = StyleConf.ActionLinkPrelight;
			} else {
				lineColor = StyleConf.ActionLinkNormal;
			}

			tk.Begin ();
			tk.FillColor = lineColor;
			tk.StrokeColor = lineColor;
			tk.LineWidth = lineWidth;
			tk.LineStyle = LineStyle.Normal;
			tk.DrawLine (line.Start, line.Stop);
			tk.FillColor = tk.StrokeColor = App.Current.Style.PaletteActive;
			tk.DrawCircle (line.Stop, 2);
			tk.End ();
		}
	}
}

