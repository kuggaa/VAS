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
using VAS.Core.Common;
using VAS.Core.MVVMC;

namespace VAS.Tests.MVVMC
{

	public class DummyModelChild : BindableBase
	{
	}

	public class DummyModelWithChilren : BindableBase
	{
		public RangeObservableCollection<DummyModelChild> Children {
			get;
			set;
		} = new RangeObservableCollection<DummyModelChild> ();
	}

	public class DummyModelChildVM : ViewModelBase<DummyModelChild>
	{
	}

	public class DummyNestedSubVM : NestedSubViewModel<DummyModelWithChilren, DummyNestedSubVM, DummyModelChild, DummyModelChildVM>
	{
		public override RangeObservableCollection<DummyModelChild> ChildModels {
			get {
				return Model.Children;
			}
		}
	}

	[TestFixture]
	public class TestNestedSubViewModel
	{
		DummyModelChild child1, child2, child3;
		DummyModelWithChilren model;
		DummyNestedSubVM viewModel;

		[SetUp]
		public void SetUp ()
		{
			child1 = new DummyModelChild ();
			child2 = new DummyModelChild ();
			child3 = new DummyModelChild ();
			model = new DummyModelWithChilren ();
			model.Children.Add (child1);
			model.Children.Add (child2);
			model.Children.Add (child3);
			viewModel = new DummyNestedSubVM ();
			viewModel.Model = model;
		}

		[Test]
		public void TestSetModel ()
		{
			Assert.AreEqual (3, viewModel.Count ());
			Assert.AreSame (child1, viewModel.ViewModels [0].Model);
			Assert.AreSame (child2, viewModel.ViewModels [1].Model);
			Assert.AreSame (child3, viewModel.ViewModels [2].Model);
		}

		[Test]
		public void TestReplaceModel ()
		{
			child1 = new DummyModelChild ();
			child2 = new DummyModelChild ();
			model = new DummyModelWithChilren ();
			model.Children.Add (child1);
			model.Children.Add (child2);
			viewModel.Model = model;

			Assert.AreEqual (2, viewModel.Count ());
			Assert.AreSame (child1, viewModel.ViewModels [0].Model);
			Assert.AreSame (child2, viewModel.ViewModels [1].Model);
		}

		[Test]
		public void TestAddChild ()
		{
			var child4 = new DummyModelChild ();
			model.Children.Add (child4);

			Assert.AreEqual (4, viewModel.Count ());
			Assert.AreSame (child4, viewModel.ViewModels [3].Model);
		}

		[Test]
		public void TestRemoveChild ()
		{
			model.Children.Remove (child1);

			Assert.AreEqual (2, viewModel.Count ());
			Assert.AreSame (child2, viewModel.ViewModels [0].Model);
			Assert.AreSame (child3, viewModel.ViewModels [1].Model);
		}
	}
}
