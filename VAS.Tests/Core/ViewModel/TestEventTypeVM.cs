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
using System;
using NUnit.Framework;
using VAS.Core.Common;
using VAS.Core.Store;
using VAS.Core.ViewModel;

namespace VAS.Tests.Core.ViewModel
{
	[TestFixture]
	public class TestEventTypeVM
	{
		[Test]
		public void TestModelProxy ()
		{
			var model = new EventType ();
			var viewModel = new EventTypeVM ();
			viewModel.Model = model;

			Assert.AreSame (viewModel.Model, model);
		}

		[Test]
		public void TestPropertyForwarding ()
		{
			int count = 0;
			var model = new EventType ();
			var viewModel = new EventTypeVM ();
			viewModel.Model = model;
			viewModel.PropertyChanged += (sender, e) => {
				count++;
			};

			model.Name = "T";
			Assert.AreEqual (1, count);
		}

		[Test]
		public void TestProperties ()
		{
			var model = new EventType {
				Name = "Test1",
				Color = Color.Red
			};
			var viewModel = new EventTypeVM {
				Model = model,
			};

			Assert.AreEqual ("Test1", viewModel.Name);
			Assert.AreEqual (Color.Red, viewModel.Color);
		}

		[Test]
		public void TagsModification_NoTags_PropertyChangedEmitted ()
		{
			bool collectionChanged = false;
			bool propertyChanged = false;
			string propertyName = "";
			var model = new AnalysisEventType {
				Name = "Test1",
				Color = Color.Red
			};
			var viewModel = new EventTypeVM {
				Model = model,
			};

			model.Tags.CollectionChanged += (sender, e) => collectionChanged = true;
			viewModel.PropertyChanged += (sender, e) => {
				propertyChanged = true;
				propertyName = e.PropertyName;
			};

			model.Tags.Add (new Tag ("tag", "group"));

			Assert.IsTrue (collectionChanged);
			Assert.IsTrue (propertyChanged);
			Assert.AreEqual ("Collection_" + nameof (model.Tags), propertyName);
		}
	}
}
