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
using NUnit.Framework;
using VAS.Core.Common;
using VAS.Core.Store;
using VAS.Core.ViewModel;

namespace VAS.Tests.Core.ViewModel
{
	[TestFixture]
	public class TestTimelineEventVM
	{
		[Test]
		public void TestProperties ()
		{
			var model = new TimelineEvent {
				EventType = new EventType {
					Color = Color.Blue
				},
				Name = "Test",
				FieldPosition = new Coordinates (),
				Notes = "BlaBla",
			};
			model.Drawings.Add (new FrameDrawing ());
			var viewModel = new TimelineEventVM {
				Model = model
			};

			Assert.AreEqual ("Test", viewModel.Name);
			Assert.AreEqual (Color.Blue, viewModel.Color);
			Assert.AreEqual ("BlaBla", viewModel.Notes);
			Assert.AreEqual (true, viewModel.HasDrawings);
			Assert.AreEqual (true, viewModel.HasFieldPosition);
		}

		[Test]
		public void TestForwardProperties ()
		{
			int count = 0;
			var model = new TimelineEvent {
				Name = "Test",
			};
			model.Drawings.Add (new FrameDrawing ());
			var viewModel = new TimelineEventVM {
				Model = model
			};

			viewModel.PropertyChanged += (sender, e) => count++;
			model.Name = "Test2";

			Assert.AreEqual (4, count);
		}

		[Test]
		public void TimelineEventVM_CompareByName_ABiggerThanB ()
		{
			//Arrange
			var modelA = new TimelineEvent {
				Name = "Attack",
				Start = new Time (1000),
				Stop = new Time (2000),
				EventType = new EventType { SortMethod = SortMethodType.SortByName }
			};
			var viewModelA = new TimelineEventVM {
				Model = modelA
			};
			var modelB = new TimelineEvent {
				Name = "Defense",
				Start = new Time (1000),
				Stop = new Time (2000),
				EventType = new EventType { SortMethod = SortMethodType.SortByName }
			};
			var viewModelB = new TimelineEventVM {
				Model = modelB
			};

			//Assertion
			Assert.AreEqual (-1, viewModelA.CompareTo (viewModelB));
		}

		[Test]
		public void TimelineEventVM_CompareByName_BBiggerThanA ()
		{
			//Arrange
			var modelA = new TimelineEvent {
				Name = "Defense",
				Start = new Time (1000),
				Stop = new Time (2000),
				EventType = new EventType { SortMethod = SortMethodType.SortByName }
			};
			var viewModelA = new TimelineEventVM {
				Model = modelA
			};
			var modelB = new TimelineEvent {
				Name = "Attack",
				Start = new Time (1000),
				Stop = new Time (2000),
				EventType = new EventType { SortMethod = SortMethodType.SortByName }
			};
			var viewModelB = new TimelineEventVM {
				Model = modelB
			};

			//Assertion
			Assert.AreEqual (1, viewModelA.CompareTo (viewModelB));
		}

		[Test]
		public void TimelineEventVM_CompareByName_ANeverEqualToB ()
		{
			//Arrange
			var modelA = new TimelineEvent {
				Name = "Attack",
				Start = new Time (1000),
				Stop = new Time (2000),
				EventType = new EventType { SortMethod = SortMethodType.SortByName }
			};
			var viewModelA = new TimelineEventVM {
				Model = modelA
			};
			var modelB = new TimelineEvent {
				Name = "Attack",
				Start = new Time (1000),
				Stop = new Time (2000),
				EventType = new EventType { SortMethod = SortMethodType.SortByName }
			};
			var viewModelB = new TimelineEventVM {
				Model = modelB
			};

			//Assertion
			Assert.AreNotEqual (0, viewModelA.CompareTo (viewModelB));
		}

		[Test]
		public void TimelineEventVM_CompareByStartTime_ABiggerThanB ()
		{
			//Arrange
			var modelA = new TimelineEvent {
				Name = "Attack",
				Start = new Time (1000),
				Stop = new Time (2000),
				EventType = new EventType { SortMethod = SortMethodType.SortByStartTime }
			};
			var viewModelA = new TimelineEventVM {
				Model = modelA
			};
			var modelB = new TimelineEvent {
				Name = "Defense",
				Start = new Time (2000),
				Stop = new Time (4000),
				EventType = new EventType { SortMethod = SortMethodType.SortByStartTime }
			};
			var viewModelB = new TimelineEventVM {
				Model = modelB
			};

			//Assertion
			Assert.Less (viewModelA.CompareTo (viewModelB), 0);
		}

		[Test]
		public void TimelineEventVM_CompareByStartTime_BBiggerThanA ()
		{
			//Arrange
			var modelA = new TimelineEvent {
				Name = "Attack",
				Start = new Time (2000),
				Stop = new Time (4000),
				EventType = new EventType { SortMethod = SortMethodType.SortByStartTime }
			};
			var viewModelA = new TimelineEventVM {
				Model = modelA
			};
			var modelB = new TimelineEvent {
				Name = "Defense",
				Start = new Time (1000),
				Stop = new Time (2000),
				EventType = new EventType { SortMethod = SortMethodType.SortByStartTime }
			};
			var viewModelB = new TimelineEventVM {
				Model = modelB
			};

			//Assertion
			Assert.Greater (viewModelA.CompareTo (viewModelB), 0);
		}

		[Test]
		public void TimelineEventVM_CompareByStartTime_ANeverEqualToB ()
		{
			//Arrange
			var modelA = new TimelineEvent {
				Name = "Attack",
				Start = new Time (1000),
				Stop = new Time (2000),
				EventType = new EventType { SortMethod = SortMethodType.SortByStartTime }
			};
			var viewModelA = new TimelineEventVM {
				Model = modelA
			};
			var modelB = new TimelineEvent {
				Name = "Defense",
				Start = new Time (1000),
				Stop = new Time (4000),
				EventType = new EventType { SortMethod = SortMethodType.SortByStartTime }
			};
			var viewModelB = new TimelineEventVM {
				Model = modelB
			};

			//Assertion
			Assert.AreNotEqual (0, viewModelA.CompareTo (viewModelB));
		}

		[Test]
		public void TimelineEventVM_CompareByStopTime_ABiggerThanB ()
		{
			//Arrange
			var modelA = new TimelineEvent {
				Name = "Attack",
				Start = new Time (1000),
				Stop = new Time (2000),
				EventType = new EventType { SortMethod = SortMethodType.SortByStopTime }
			};
			var viewModelA = new TimelineEventVM {
				Model = modelA
			};
			var modelB = new TimelineEvent {
				Name = "Defense",
				Start = new Time (2000),
				Stop = new Time (4000),
				EventType = new EventType { SortMethod = SortMethodType.SortByStopTime }
			};
			var viewModelB = new TimelineEventVM {
				Model = modelB
			};

			//Assertion
			Assert.Less (viewModelA.CompareTo (viewModelB), 0);
		}

		[Test]
		public void TimelineEventVM_CompareByStopTime_BBiggerThanA ()
		{
			//Arrange
			var modelA = new TimelineEvent {
				Name = "Attack",
				Start = new Time (2000),
				Stop = new Time (4000),
				EventType = new EventType { SortMethod = SortMethodType.SortByStopTime }
			};
			var viewModelA = new TimelineEventVM {
				Model = modelA
			};
			var modelB = new TimelineEvent {
				Name = "Defense",
				Start = new Time (1000),
				Stop = new Time (2000),
				EventType = new EventType { SortMethod = SortMethodType.SortByStopTime }
			};
			var viewModelB = new TimelineEventVM {
				Model = modelB
			};

			//Assertion
			Assert.Greater (viewModelA.CompareTo (viewModelB), 0);
		}

		[Test]
		public void TimelineEventVM_CompareByStopTime_ANeverEqualToB ()
		{
			//Arrange
			var modelA = new TimelineEvent {
				Name = "Attack",
				Start = new Time (1000),
				Stop = new Time (4000),
				EventType = new EventType { SortMethod = SortMethodType.SortByStopTime }
			};
			var viewModelA = new TimelineEventVM {
				Model = modelA
			};
			var modelB = new TimelineEvent {
				Name = "Defense",
				Start = new Time (2000),
				Stop = new Time (4000),
				EventType = new EventType { SortMethod = SortMethodType.SortByStopTime }
			};
			var viewModelB = new TimelineEventVM {
				Model = modelB
			};

			//Assertion
			Assert.AreNotEqual (viewModelA.CompareTo (viewModelB), 0);
		}

		[Test]
		public void TimelineEventVM_CompareByDurationTime_ABiggerThanB ()
		{
			//Arrange
			var modelA = new TimelineEvent {
				Name = "Attack",
				Start = new Time (1000),
				Stop = new Time (2000),
				EventType = new EventType { SortMethod = SortMethodType.SortByDuration }
			};
			var viewModelA = new TimelineEventVM {
				Model = modelA
			};
			var modelB = new TimelineEvent {
				Name = "Defense",
				Start = new Time (2000),
				Stop = new Time (5000),
				EventType = new EventType { SortMethod = SortMethodType.SortByDuration }
			};
			var viewModelB = new TimelineEventVM {
				Model = modelB
			};

			//Assertion
			Assert.Less (viewModelA.CompareTo (viewModelB), 0);
		}

		[Test]
		public void TimelineEventVM_CompareByDurationTime_BBiggerThanA ()
		{
			//Arrange
			var modelA = new TimelineEvent {
				Name = "Attack",
				Start = new Time (1000),
				Stop = new Time (4000),
				EventType = new EventType { SortMethod = SortMethodType.SortByDuration }
			};
			var viewModelA = new TimelineEventVM {
				Model = modelA
			};
			var modelB = new TimelineEvent {
				Name = "Defense",
				Start = new Time (2000),
				Stop = new Time (4000),
				EventType = new EventType { SortMethod = SortMethodType.SortByDuration }
			};
			var viewModelB = new TimelineEventVM {
				Model = modelB
			};

			//Assertion
			Assert.Greater (viewModelA.CompareTo (viewModelB), 0);
		}

		[Test]
		public void TimelineEventVM_CompareByDurationTime_ANeverEqualToB ()
		{
			//Arrange
			var modelA = new TimelineEvent {
				Name = "Attack",
				Start = new Time (1000),
				Stop = new Time (2000),
				EventType = new EventType { SortMethod = SortMethodType.SortByDuration }
			};
			var viewModelA = new TimelineEventVM {
				Model = modelA
			};
			var modelB = new TimelineEvent {
				Name = "Defense",
				Start = new Time (2000),
				Stop = new Time (4000),
				EventType = new EventType { SortMethod = SortMethodType.SortByDuration }
			};
			var viewModelB = new TimelineEventVM {
				Model = modelB
			};

			//Assertion
			Assert.AreNotEqual (viewModelA.CompareTo (viewModelB), 0);
		}
	}
}
