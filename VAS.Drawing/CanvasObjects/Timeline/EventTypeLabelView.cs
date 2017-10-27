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

using System.ComponentModel;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Interfaces.Drawing;
using VAS.Core.MVVMC;
using VAS.Core.Store.Drawables;
using VAS.Core.ViewModel;

namespace VAS.Drawing.CanvasObjects.Timeline
{

	/// <summary>
	/// A label for the event types timeline row.
	/// </summary>
	[View ("EventTypeLabelView")]
	public class EventTypeLabelView : LabelView, ICanvasObjectView<EventTypeTimelineVM>
	{
		const int TIMELINE_BUTTON_MARGIN = 3;

		EventTypeTimelineVM viewModel;
		TimelineButtonView playButton;

		public EventTypeLabelView ()
		{
			playButton = new TimelineButtonView ();
			playButton.Icon = App.Current.ResourcesLocator.LoadImage (StyleConf.PlayButton);
			playButton.ClickedEvent += PlayButtonClickedEvent;
			playButton.RedrawEvent += HandleButtonRedrawEvent;
		}

		public override double RequiredWidth {
			get {
				var width = base.RequiredWidth;
				return width + RectSize + TIMELINE_BUTTON_MARGIN;
			}
		}

		public override Color Color {
			get {
				return ViewModel.EventTypeVM.Color;
			}
		}

		public override string Name {
			get {
				return ViewModel.EventTypeVM.Name;
			}
		}

		public EventTypeTimelineVM ViewModel {
			get {
				return viewModel;
			}
			set {
				if (viewModel != null) {
					viewModel.PropertyChanged -= HandlePropertyChanged;
				}
				viewModel = value;
				if (viewModel != null) {
					viewModel.PropertyChanged += HandlePropertyChanged;
					playButton.Insensitive = viewModel.VisibleChildrenCount == 0;
				}
			}
		}

		public void SetViewModel (object viewModel)
		{
			ViewModel = (EventTypeTimelineVM)viewModel;
		}

		public override void Draw (IDrawingToolkit tk, Area area)
		{
			double hs, vs;

			hs = StyleConf.TimelineLabelHSpacing;
			vs = StyleConf.TimelineLabelVSpacing;

			tk.Begin ();
			tk.FillColor = BackgroundColor;
			tk.StrokeColor = BackgroundColor;
			tk.LineWidth = 0;
			tk.DrawRectangle (new Point (0, scrolledY), Width, Height);

			/* Draw a rectangle with the category color */
			tk.FillColor = Color;
			tk.StrokeColor = Color;
			tk.DrawRectangle (new Point (hs, scrolledY + vs), RectSize, RectSize);

			/* Draw category name */
			tk.FontSlant = FontSlant.Normal;
			tk.FontWeight = DEFAULT_FONT_WEIGHT;
			tk.FontSize = DEFAULT_FONT_SIZE;
			tk.FillColor = App.Current.Style.TextBase;
			tk.FontAlignment = FontAlignment.Left;
			tk.StrokeColor = App.Current.Style.TextBase;
			tk.DrawText (new Point (TextOffset, scrolledY), Width - TextOffset, Height, Name);
			//Draw playButton
			playButton.Draw (tk, area);
			tk.End ();
		}

		public override Selection GetSelection (Point point, double precision, bool inMotion = false)
		{
			Selection selection = playButton.GetSelection (point, precision, inMotion);
			if (selection == null) {
				selection = base.GetSelection (point, precision, inMotion);
			}
			return selection;
		}

		protected override void HandleSizeChanged ()
		{
			base.HandleSizeChanged ();
			playButton.Width = (int)RectSize;
			playButton.Height = (int)RectSize;
			playButton.Position = new Point (Width - RectSize - TIMELINE_BUTTON_MARGIN,
											 scrolledY + TIMELINE_BUTTON_MARGIN);
		}

		void PlayButtonClickedEvent (ICanvasObject co)
		{
			ViewModel.LoadEventType ();
		}

		void HandlePropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof (TimelineEventVM.Visible) ||
				e.PropertyName == $"Collection_{nameof (EventTypeTimelineVM.ViewModels)}") {
				playButton.Insensitive = ViewModel.VisibleChildrenCount == 0;
				ReDraw ();
			} else if (e.PropertyName == nameof (EventTypeVM.Name)) {
				ReDraw ();
			}
		}

		void HandleButtonRedrawEvent (ICanvasObject co, Area area)
		{
			EmitRedrawEvent (co as CanvasObject, area);
		}
	}
}
