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
using System.Linq;
using NUnit.Framework;
using VAS.Core.MVVMC;
using VAS.Core.Store;
using VAS.Core.ViewModel;

namespace VAS.Tests.MVVMC
{
	[TestFixture]
	public class TestNestedViewModel
	{
		NestedViewModel<ViewModelBase<BindableBase>> viewModel;
		ViewModelBase<BindableBase> child1, child2;

		[SetUp]
		public void SetUp ()
		{
			viewModel = new NestedViewModel<ViewModelBase<BindableBase>> ();
			child1 = new ViewModelBase<BindableBase> ();
			child2 = new ViewModelBase<BindableBase> ();
			viewModel.ViewModels.Add (child1);
			viewModel.ViewModels.Add (child2);
		}

		[Test]
		public void TestEnumerator ()
		{
			Assert.AreEqual (2, viewModel.Count ());
		}

		[Test]
		public void TestSelection ()
		{
			int count = 0;
			string propName = "";
			viewModel.PropertyChanged += (sender, e) => { count++; propName = e.PropertyName; };

			viewModel.Select (child1);

			Assert.AreEqual (1, count);
			Assert.AreEqual ("Selection", propName);
			Assert.AreSame (viewModel.Selection [0], child1);
		}

		[Test]
		public void TestReplaceSelection ()
		{
			int count = 0;
			string propName = "";
			viewModel.Select (child1);
			viewModel.PropertyChanged += (sender, e) => { count++; propName = e.PropertyName; };

			viewModel.Selection.Replace (viewModel.ViewModels);

			Assert.AreEqual (1, count);
			Assert.AreEqual ("Selection", propName);
			Assert.AreSame (viewModel.Selection [0], child1);
			Assert.AreSame (viewModel.Selection [1], child2);
		}

		[Test]
		public void SelectionReplace_Samelist_DoNotReplace ()
		{
			int count = 0;
			viewModel.Selection.Replace (viewModel.ViewModels);
			viewModel.PropertyChanged += (sender, e) => count++;

			viewModel.Selection.Replace (viewModel.ViewModels);

			Assert.AreEqual (0, count);
			Assert.AreSame (viewModel.Selection [0], child1);
			Assert.AreSame (viewModel.Selection [1], child2);
		}

		[Test]
		public void TestReplaceSelectionWithNull ()
		{
			int count = 0;
			string propName = "";
			viewModel.Select (child1);
			viewModel.PropertyChanged += (sender, e) => { count++; propName = e.PropertyName; };

			viewModel.Selection.Replace (null);

			Assert.AreEqual (1, count);
			Assert.AreEqual ("Selection", propName);
			Assert.AreEqual (0, viewModel.Selection.Count);
		}

		[Test]
		public void TestVisibleEvents ()
		{
			// Arrange
			var childVM1 = new TimelineEventVM ();
			var childVM2 = new TimelineEventVM ();
			var childVM3 = new TimelineEventVM ();
			var viewModel = new NestedViewModel<TimelineEventVM> ();
			viewModel.ViewModels.Add (childVM1);
			viewModel.ViewModels.Add (childVM2);
			viewModel.ViewModels.Add (childVM3);
			// Assume
			Assert.AreEqual (3, viewModel.VisibleChildrenCount);

			// Act
			childVM1.Visible = false;
			childVM3.Visible = false;

			// Assert
			Assert.AreEqual (1, viewModel.VisibleChildrenCount);
		}

	}
}
