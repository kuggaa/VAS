//
//  Copyright (C) 2017 Fluendo
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
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Store;
using VAS.Core.ViewModel;

namespace VAS.Tests.Core.ViewModel
{
	public class TestDashboardButtonVM
	{
		[Test]
		public void Click_EventButtonVM_NotifiesOk ()
		{
			// Arrange
			EventButton button = new EventButton ();
			EventButtonVM sut = new EventButtonVM { Model = button };
			sut.CurrentTime = new Time ();
			bool notified = false;
			List<Tag> tags = null;
			App.Current.EventsBroker.Subscribe<NewTagEvent> ((e) => {
				notified = true;
				tags = e.Tags.ToList();
			});

			// Act
			sut.Click ();

			// Assert
			Assert.IsTrue (notified);
			Assert.AreEqual (0, tags.Count ());
		}

		[Test]
		public void Click_AnalysisEventButtonVM_NotifiesOk ()
		{
			// Arrange
			AnalysisEventButton button = new AnalysisEventButton { EventType = new AnalysisEventType () };
			AnalysisEventButtonVM sut = new AnalysisEventButtonVM { Model = button };
			sut.CurrentTime = new Time ();
			sut.SelectedTags.Add (new TagVM { Model = new Tag ("hola") });
			bool notified = false;
			List<Tag> tags = null;
			App.Current.EventsBroker.Subscribe<NewTagEvent> ((e) => {
				notified = true;
				tags = e.Tags.ToList ();
			});

			// Act
			sut.Click ();

			// Assert
			Assert.IsTrue (notified);
			Assert.AreEqual (1, tags.Count ());
			Assert.AreEqual (0, sut.SelectedTags.Count ());
		}
	}
}
