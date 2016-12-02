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
	}
}
