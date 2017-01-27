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

using System.Collections.Generic;
using System.Collections.Specialized;
using VAS.Core.Common;
using VAS.Core.Interfaces.Drawing;
using VAS.Core.MVVMC;
using VAS.Core.ViewModel;

namespace VAS.Drawing.CanvasObjects.Timeline
{
	/// <summary>
	/// A timeline that renders project timers and allows interacting with them.
	/// </summary>
	[View ("TimerTimelineView")]
	public class TimerTimelineView : TimelineView, ICanvasObjectView<TimerVM>
	{
		TimerVM viewModel;
		Dictionary<TimeNodeVM, TimeNodeView> viewModelToView;

		public TimerTimelineView ()
		{
			viewModelToView = new Dictionary<TimeNodeVM, TimeNodeView> ();
		}

		/// <summary>
		///  Color used to drag the nodes in the timeline
		/// </summary>
		/// <value>The color of the line.</value>
		public Color LineColor {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a value indicating whether the timer nodes in 
		/// this <see cref="T:VAS.Drawing.CanvasObjects.Timeline.TimerTimelineView"/> will display the name of the timer.
		/// </summary>
		/// <value><c>true</c> if show name; otherwise, <c>false</c>.</value>
		public bool ShowName {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the dragging mode of the nodes in this timeline.
		/// </summary>
		/// <value>The dragging mode.</value>
		public NodeDraggingMode DraggingMode {
			get;
			set;
		}

		public TimerVM ViewModel {
			get {
				return viewModel;
			}
			set {
				if (viewModel != null) {
					viewModel.GetNotifyCollection ().CollectionChanged -= HandleTimerCollectionChanged;
				}
				viewModel = value;
				ClearObjects ();
				if (viewModel != null) {
					viewModel.GetNotifyCollection ().CollectionChanged += HandleTimerCollectionChanged;
					Reload ();
				}
			}
		}

		public void SetViewModel (object viewModel)
		{
			ViewModel = (TimerVM)viewModel;
		}

		void AddTimeNode (TimeNodeVM timeNodeVM)
		{
			TimerTimeNodeView to = new TimerTimeNodeView ();
			to.ViewModel = timeNodeVM;
			to.Timer = ViewModel;
			to.OffsetY = OffsetY;
			to.Height = Height;
			to.SecondsPerPixel = SecondsPerPixel;
			to.MaxTime = Duration;
			to.DraggingMode = DraggingMode;
			to.ShowName = ShowName;
			to.LineColor = LineColor;
			AddNode (to);
			viewModelToView.Add (timeNodeVM, to);
		}

		void RemoveTimeNode (TimeNodeVM timeNodeVM)
		{
			RemoveObject (viewModelToView [timeNodeVM], true);
			viewModelToView.Remove (timeNodeVM);
		}

		void Reload ()
		{
			ClearObjects ();
			viewModelToView.Clear ();
			foreach (TimeNodeVM t in viewModel) {
				AddTimeNode (t);
			}
		}

		void HandleTimerCollectionChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action) {
			case NotifyCollectionChangedAction.Add: {
					foreach (TimeNodeVM timeNodeVM in e.NewItems) {
						AddTimeNode (timeNodeVM);
					}
					break;
				}
			case NotifyCollectionChangedAction.Remove: {
					foreach (TimeNodeVM timenodeVM in e.OldItems) {
						RemoveTimeNode (timenodeVM);
					}
					break;
				}
			case NotifyCollectionChangedAction.Reset: {
					Reload ();
					break;
				}
			}
		}
	}
}
