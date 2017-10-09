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
using System.ComponentModel;
using VAS.Core.Common;
using VAS.Core.Hotkeys;
using VAS.Core.Interfaces.Drawing;
using VAS.Core.MVVMC;
using VAS.Core.Store;
using VAS.Core.Store.Drawables;
using VAS.Core.Store.Playlists;
using VAS.Core.ViewModel;
using VAS.Drawing.CanvasObjects.Timeline;

namespace VAS.Drawing.Widgets
{

	[View ("EventEditionTimeruleView")]
	public class EventEditionTimeruleView : SelectionCanvas, ICanvasView<VideoPlayerVM>
	{
		const int BIG_LINE_HEIGHT = 8;
		const int SMALL_LINE_HEIGHT = 3;
		const int MSECONDS_STEP = 20;
		int fontSize;
		VideoPlayerVM viewModel;
		TimeNodeEditorView nodeView;
		TimeNodeVM loadedEventVM;
		KeyContext moveHandlersContext;

		public EventEditionTimeruleView (IWidget widget) : base (widget)
		{
			Accuracy = 20;
			FontSize = StyleConf.TimelineRulePlayerFontSize;
			loadedEventVM = new TimeNodeVM ();
			nodeView = new TimeNodeEditorView {
				SelectionMode = NodeSelectionMode.Borders,
			};
			nodeView.ViewModel = loadedEventVM;
			AddObject (nodeView);

			// FIXME: This should be handled in the VideoPlayerController once we start using VM's for the loaded event
			// and it could be used in the timeline too, not just here. Right now it's not possible because
			// the SelectedHandle property is in the VM and hence not accessible from the VideoPlayerController.
			moveHandlersContext = new KeyContext ();
			KeyAction actionRight = new KeyAction (new KeyConfig {
				Name = "",
				Key = App.Current.Keyboard.ParseName ("Right"),
			}, () => HandleKeyPressed (true));
			KeyAction actionLeft = new KeyAction (new KeyConfig {
				Name = "",
				Key = App.Current.Keyboard.ParseName ("Left"),
			}, () => HandleKeyPressed (false));
			moveHandlersContext.AddAction (actionRight);
			moveHandlersContext.AddAction (actionLeft);
		}

		public VideoPlayerVM ViewModel {
			get {
				return viewModel;
			}
			set {
				if (viewModel != null) {
					viewModel.PropertyChanged -= HandlePropertyChangedEventHandler;
				}
				viewModel = value;
				if (viewModel != null) {
					viewModel.PropertyChanged += HandlePropertyChangedEventHandler;
				}
			}
		}

		int FontSize {
			get {
				return fontSize;
			}
			set {
				int theight, twidth;
				fontSize = value;
				tk.MeasureText ("99:99:99", out twidth, out theight, "", fontSize, FontWeight.Normal);
				TextWidth = twidth;
			}
		}

		int TextWidth {
			get;
			set;
		}

		public void SetViewModel (object viewModel)
		{
			ViewModel = (VideoPlayerVM)viewModel;
		}

		public override void Draw (IContext context, Area area)
		{
			double editorStartX, editorStopX, nodeStartX, nodeStopX, start, height, width, startWidth;
			double interval = 0, secondsPerPixel, totalSeconds, timeSpacing, intervalLot;

			if (loadedEventVM.Model == null || ViewModel.EditEventDurationTimeNode.Model == null) {
				return;
			}
			height = widget.Height;
			width = widget.Width;
			totalSeconds = ViewModel.EditEventDurationTimeNode.Duration.TotalSeconds;
			secondsPerPixel = totalSeconds / width;
			//Calculate the timeSpacing in pixels
			foreach (int i in Constants.MARKER) {
				int pixels = (int)Math.Ceiling (Constants.MINIMUM_TIME_SPACING * (totalSeconds / i));
				if (pixels <= width) {
					if (ViewModel.EditEventDurationTimeNode.Duration.TotalSeconds > 0) {
						timeSpacing = width / (totalSeconds / i);
						interval = i;
					}
					break;
				}
			}

			editorStartX = Utils.TimeToPos (ViewModel.EditEventDurationTimeNode.Start, secondsPerPixel);
			nodeStartX = Utils.TimeToPos (loadedEventVM.Start, secondsPerPixel);
			start = editorStartX - (editorStartX % interval);
			editorStopX = Utils.TimeToPos (ViewModel.EditEventDurationTimeNode.Stop, secondsPerPixel);
			nodeStopX = Utils.TimeToPos (loadedEventVM.Stop, secondsPerPixel);
			intervalLot = ((interval / secondsPerPixel) / 10);
			startWidth = nodeStartX - editorStartX;

			Begin (context);

			tk.LineWidth = 0;
			BackgroundColor = App.Current.Style.Text_DarkColor;
			DrawBackground ();

			tk.FillColor = App.Current.Style.PaletteBackground;
			tk.DrawRectangle (new Point (nodeStartX - editorStartX, 0), nodeStopX - nodeStartX, height);

			tk.StrokeColor = App.Current.Style.PaletteWidgets;
			tk.FillColor = App.Current.Style.PaletteWidgets;
			tk.LineWidth = Constants.TIMELINE_LINE_WIDTH;
			tk.FontSlant = FontSlant.Normal;
			tk.FontSize = FontSize;
			tk.DrawLine (new Point (0, height), new Point (width, height));

			//Draw a big line each interval start point
			for (double i = start; i <= editorStopX; i += interval / secondsPerPixel) {
				double pos = i - editorStartX;
				tk.DrawLine (new Point (pos, height), new Point (pos, height - BIG_LINE_HEIGHT));
				tk.FontAlignment = FontAlignment.Center;
				string timeText = new Time { TotalSeconds = (int)(i * secondsPerPixel) }.ToSecondsString ();
				tk.DrawText (new Point (pos - TextWidth / 2, 2), TextWidth, height - BIG_LINE_HEIGHT - 2, timeText);

				//Draw 9 small lines to separate each interval in 10 partitions
				for (int j = 1; j < 10; j++) {
					double position = pos + intervalLot * j;
					tk.DrawLine (new Point (position, height), new Point (position, height - SMALL_LINE_HEIGHT));
				}
			}

			nodeView.ScrollX = editorStartX;
			nodeView.SecondsPerPixel = secondsPerPixel;
			nodeView.Draw (tk, null);
			End ();
		}

		protected override void StartMove (Selection sel)
		{
			base.StartMove (sel);
			ViewModel.Pause ();
		}

		protected override void StopMove (bool moved)
		{
			if (moved) {
				ViewModel.EditEventDurationCommand.Execute (true);
			}
		}

		void HandlePropertyChangedEventHandler (object sender, PropertyChangedEventArgs e)
		{
			if (!Moving && sender == loadedEventVM &&
				(e.PropertyName == nameof (TimeNodeVM.Start) || e.PropertyName == nameof (TimeNodeVM.Stop))) {
				widget?.ReDraw ();
			} else if (e.PropertyName == nameof (VideoPlayerVM.EditEventDurationModeEnabled)) {
				if (ViewModel.EditEventDurationModeEnabled) {
					App.Current.KeyContextManager.AddContext (moveHandlersContext);
					nodeView.MaxTime = ViewModel.EditEventDurationTimeNode.Stop;
					widget?.ReDraw ();
				} else {
					App.Current.KeyContextManager.RemoveContext (moveHandlersContext);
				}
			} else if (e.PropertyName == nameof (VideoPlayerVM.LoadedElement)) {
				if (ViewModel.LoadedElement is PlaylistPlayElement) {
					loadedEventVM.Model = (ViewModel.LoadedElement as PlaylistPlayElement).Play;
				} else {
					loadedEventVM.Model = ViewModel.LoadedElement as TimeNode;
				}
				if (loadedEventVM.Model != null) {
					widget?.ReDraw ();
				}
			}
		}

		void HandleKeyPressed (bool isRight)
		{
			Time time = loadedEventVM.SelectedGrabber == SelectionPosition.Left ? loadedEventVM.Start : loadedEventVM.Stop;
			ViewModel.Pause ();

			if (isRight) {
				time.MSeconds += MSECONDS_STEP;
			} else {
				time.MSeconds -= MSECONDS_STEP;
			}
			widget?.ReDraw ();
		}
	}
}
