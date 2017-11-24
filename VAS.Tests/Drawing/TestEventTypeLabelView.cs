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
using NUnit.Framework;
using VAS.Core.Common;
using VAS.Core.Resources.Styles;
using VAS.Core.ViewModel;
using VAS.Drawing.CanvasObjects.Timeline;

namespace VAS.Tests.Drawing
{
	[TestFixture]
	public class TestEventTypeLabelView
	{
		EventTypeLabelView eventTypeLabel;
		Point buttonPos;

		[SetUp]
		public void SetUp ()
		{
			eventTypeLabel = new EventTypeLabelView ();
			eventTypeLabel.Width = 100;
			eventTypeLabel.Height = 40;
			buttonPos = new Point (eventTypeLabel.Width - (eventTypeLabel.Height - Sizes.TimelineLabelVSpacing * 2) - 3 + 1,
								  3 + 1);
			eventTypeLabel.SetViewModel (new EventTypeTimelineVM ());
		}

		[TearDown]
		public void TearDown ()
		{
			eventTypeLabel.Dispose ();
		}

		[Test]
		public void TestButtonInitilizationInsensitive ()
		{
			var button = eventTypeLabel.GetSelection (buttonPos, 0).Drawable as TimelineButtonView;

			Assert.IsTrue (button.Insensitive);
		}

		[Test]
		public void TestButtonChangesToSensitiveWhenEventsAreAdded ()
		{
			var button = eventTypeLabel.GetSelection (buttonPos, 0).Drawable as TimelineButtonView;

			eventTypeLabel.ViewModel.ViewModels.Add (new TimelineEventVM ());

			Assert.IsFalse (button.Insensitive);
		}

		[Test]
		public void TestButtonChangesToInsensitiveWhenEventsAreRemoved ()
		{
			var button = eventTypeLabel.GetSelection (buttonPos, 0).Drawable as TimelineButtonView;

			eventTypeLabel.ViewModel.ViewModels.Add (new TimelineEventVM ());
			eventTypeLabel.ViewModel.ViewModels.Add (new TimelineEventVM ());

			Assert.IsFalse (button.Insensitive);

			//Remove All Elements
			eventTypeLabel.ViewModel.ViewModels.Clear ();

			Assert.IsTrue (button.Insensitive);
		}

		[Test]
		public void TestButtonChangesSensitivenessWhenEventsVisibilityChanges ()
		{
			var button = eventTypeLabel.GetSelection (buttonPos, 0).Drawable as TimelineButtonView;

			eventTypeLabel.ViewModel.ViewModels.Add (new TimelineEventVM ());
			eventTypeLabel.ViewModel.ViewModels.Add (new TimelineEventVM ());

			Assert.IsFalse (button.Insensitive);

			//Now Make Events not Visible
			eventTypeLabel.ViewModel.ViewModels [0].Visible = false;
			eventTypeLabel.ViewModel.ViewModels [1].Visible = false;

			Assert.IsTrue (button.Insensitive);

			//Finally Make Only One Event Visible
			eventTypeLabel.ViewModel.ViewModels [1].Visible = true;

			Assert.IsFalse (button.Insensitive);
		}
	}
}
