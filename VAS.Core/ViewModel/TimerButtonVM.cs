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
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Interfaces;
using VAS.Core.Store;
using Timer = VAS.Core.Store.Timer;

namespace VAS.Core.ViewModel
{
	/// <summary>
	/// Class for the TimerButton ViewModel.
	/// </summary>
	public class TimerButtonVM : DashboardButtonVM, ITimed
	{
		TimeNode currentNode;
		Time currentTime;

		public TimerButtonVM ()
		{
			currentTime = new Time (0);
		}

		/// <summary>
		/// Gets or sets the model.
		/// </summary>
		/// <value>The model.</value>
		public TimerButton TypedModel {
			get {
				return (TimerButton)base.Model;
			}
		}

		[PropertyChanged.DoNotCheckEquality]
		public override DashboardButton Model {
			get {
				return TypedModel;
			}
			set {
				base.Model = value;
			}
		}

		/// <summary>
		/// Gets the view.
		/// </summary>
		/// <value>The view.</value>
		public override string View {
			get {
				return "TimerButtonView";
			}
		}

		/// <summary>
		/// Gets or sets the timer.
		/// </summary>
		/// <value>The timer.</value>
		public Timer Timer {
			get {
				return TypedModel.Timer;
			}
			set {
				TypedModel.Timer = value;
			}
		}

		[PropertyChanged.DoNotNotify]
		public Time CurrentTime {
			get {
				return currentTime;
			}

			set {
				if (value != null && currentNode != null &&
					Mode != DashboardMode.Edit && currentTime != null) {
					if (value + Constants.PLAYBACK_TOLERANCE < currentNode.Start) {
						Click (true);
					} else if (currentTime.TotalSeconds != value.TotalSeconds) {
						TimerTime = value - currentNode.Start;
					}
				}
				currentTime = value;
			}
		}

		public Time TimerTime {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="T:VAS.Core.ViewModel.TimerButtonVM"/> is active.
		/// </summary>
		/// <value><c>true</c> if active; otherwise, <c>false</c>.</value>
		public bool Active { get; set; }

		public void Click (bool cancel)
		{
			//Cancel
			if (cancel || Mode == DashboardMode.Edit) {
				currentNode = null;
				TimerTime = null;
				return;
			}
			//Start
			if (!Start (null)) {
				Stop (null);
			}
		}

		/// <summary>
		/// Start the timer operation.
		/// </summary>
		/// <param name="buttons">Buttons.</param>
		public bool Start (List<DashboardButtonVM> buttons)
		{
			if (currentNode != null) {
				return false;
			}

			currentNode = new TimeNode { Name = Name, Start = CurrentTime };
			Active = true;
			TimerTime = new Time (0);
			App.Current.EventsBroker.Publish (new TimeNodeStartedEvent {
				DashboardButtons = buttons,
				TimerButton = this,
				TimeNode = currentNode
			});

			return true;
		}

		/// <summary>
		/// Stops the timer operation
		/// </summary>
		/// <param name="buttons">Buttons.</param>
		public void Stop (List<DashboardButtonVM> buttons)
		{
			if (currentNode == null) {
				return;
			}

			if (currentNode.Start.MSeconds != CurrentTime.MSeconds) {
				currentNode.Stop = CurrentTime;
				Timer.Nodes.Add (currentNode);
			}

			Active = false;
			currentNode = null;
			TimerTime = null;
			App.Current.EventsBroker.Publish (new TimeNodeStoppedEvent {
				DashboardButtons = buttons,
				TimerButton = this,
				TimeNode = currentNode
			});
		}
	}
}
