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
using System.Linq.Expressions;
using NUnit.Framework;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;

namespace VAS.Tests.MVVMC
{
	[TestFixture]
	public class TestPropertyBinding
	{
		[Test]
		public void TestUpdateProperty_WithoutConverter ()
		{
			var viewModel = new DummyPropertyViewModel ();
			var binding = new DummyPropertyBinding (vm => ((DummyPropertyViewModel)vm).Prop1);
			binding.ViewModel = viewModel;
			viewModel.Prop1 = "foo";

			Assert.AreEqual ("foo", binding.val);
		}

		[Test]
		public void TestUpdateProperty_WithConverter ()
		{
			var viewModel = new DummyPropertyViewModel ();
			var binding = new DummyPropertyBinding (vm => ((DummyPropertyViewModel)vm).Prop2, new Int32Converter ());
			binding.ViewModel = viewModel;
			viewModel.Prop2 = 32;

			Assert.AreEqual ("32", binding.val);
		}

		[Test]
		public void TestUpdateView_WithoutConverter ()
		{
			var viewModel = new DummyPropertyViewModel ();
			var binding = new DummyPropertyBinding (vm => ((DummyPropertyViewModel)vm).Prop1);
			binding.ViewModel = viewModel;
			binding.ViewChanged ("Foo");
			Assert.AreEqual ("Foo", viewModel.Prop1);
		}

		[Test]
		public void TestUpdateView_WithConverter ()
		{
			var viewModel = new DummyPropertyViewModel ();
			var binding = new DummyPropertyBinding (vm => ((DummyPropertyViewModel)vm).Prop2, new Int32Converter ());
			binding.ViewModel = viewModel;
			binding.ViewChanged ("32");
			Assert.AreEqual (32, viewModel.Prop2);
		}

		[Test]
		public void TestBind_WithInvalidConverter ()
		{
			Assert.Throws<InvalidCastException> (() => new DummyPropertyBinding (vm => ((DummyPropertyViewModel)vm).Prop2,
																				 new DateTimeConverter ()));
		}

		[Test]
		public void TestBind_WithPrivateSetter_WithoutConverter ()
		{
			DummyPropertyBinding binding = null;
			var viewModel = new DummyPropertyViewModel ();

			Assert.DoesNotThrow (() => { binding = new DummyPropertyBinding (vm => ((DummyPropertyViewModel)vm).Prop3); });

			binding.ViewModel = viewModel;
			binding.ViewChanged ("Foo");
			Assert.AreEqual ("Dog", viewModel.Prop3);
		}

		[Test]
		public void TestBind_WithPrivateSetter_WithConverter ()
		{
			DummyPropertyBinding binding = null;
			var viewModel = new DummyPropertyViewModel ();

			Assert.DoesNotThrow (() => {
				binding =
					new DummyPropertyBinding (vm => ((DummyPropertyViewModel)vm).Prop4, new Int32Converter ());
			});

			binding.ViewModel = viewModel;
			binding.ViewChanged ("32");
			Assert.AreEqual (3, viewModel.Prop4);
		}

		[Test]
		public void TestChangeViewModel_WithoutConverter ()
		{
			var viewModel1 = new DummyPropertyViewModel ();
			var viewModel2 = new DummyPropertyViewModel ();
			var binding = new DummyPropertyBinding (vm => ((DummyPropertyViewModel)vm).Prop1);
			binding.ViewModel = viewModel1;
			binding.ViewModel = viewModel2;

			binding.ViewChanged ("bar");

			Assert.IsNull (viewModel1.Prop1);
			Assert.AreEqual ("bar", viewModel2.Prop1);
		}

		[Test]
		public void TestChangeViewModel_WithConverter ()
		{
			var viewModel1 = new DummyPropertyViewModel ();
			var viewModel2 = new DummyPropertyViewModel ();
			var binding = new DummyPropertyBinding (vm => ((DummyPropertyViewModel)vm).Prop2, new Int32Converter ());
			binding.ViewModel = viewModel1;
			binding.ViewModel = viewModel2;

			binding.ViewChanged ("32");

			Assert.IsNull (viewModel1.Prop1);
			Assert.AreEqual (32, viewModel2.Prop2);
		}

		[Test]
		public void TestBindAnidatedProperties_WithoutConverter ()
		{
			var parent = new DummyPropertyViewModel ();
			var child = new DummyPropertyViewModel ();
			parent.Prop5 = child;

			var binding = new DummyPropertyBinding (vm => ((DummyPropertyViewModel)vm).Prop5.Prop1);
			binding.ViewModel = parent;
			child.Prop1 = "foo";
			Assert.AreEqual ("foo", binding.val);
		}

		[Test]
		public void TestBindAnidatedProperties_WithConverter ()
		{
			var parent = new DummyPropertyViewModel ();
			var child = new DummyPropertyViewModel ();
			parent.Prop5 = child;

			var binding = new DummyPropertyBinding (vm => ((DummyPropertyViewModel)vm).Prop5.Prop2, new Int32Converter ());
			binding.ViewModel = parent;
			child.Prop2 = 32;
			Assert.AreEqual ("32", binding.val);
		}

		[Test]
		public void Dispose_NullViewModel_NoException(){
			DummyPropertyBinding binding = null;
			var viewModel = new DummyPropertyViewModel ();

			binding = new DummyPropertyBinding (vm => ((DummyPropertyViewModel)vm).Prop4, new Int32Converter ());

			binding.ViewModel = viewModel;

			binding.ViewModel = null;
			Assert.DoesNotThrow(binding.Dispose);
		}
	}

	class DummyPropertyViewModel : ViewModelBase<BindableBase>
	{
		string prop1;
		int prop2;
		DummyPropertyViewModel prop5;

		public string Prop1 {
			get {
				return prop1;
			}
			set {
				prop1 = value;
				RaisePropertyChanged (nameof (Prop1), this);
			}
		}

		public int Prop2 {
			get {
				return prop2;
			}
			set {
				prop2 = value;
				RaisePropertyChanged (nameof (Prop2), this);
			}
		}

		public string Prop3 { get { return "Dog"; } }

		public int Prop4 { get { return 3; } }

		public DummyPropertyViewModel Prop5 {
			get {
				return prop5;
			}

			set {
				prop5 = value;
				prop5.PropertyChanged += (sender, e) => RaisePropertyChanged (e, prop5);
				RaisePropertyChanged (nameof (Prop5), this);
			}
		}
	}

	class DummyPropertyBinding : PropertyBinding<string>
	{
		public string val;

		public DummyPropertyBinding (Expression<Func<IViewModel, string>> propertyExpression) : base (propertyExpression)
		{
		}

		public DummyPropertyBinding (Expression<Func<IViewModel, object>> propertyExpression, TypeConverter converter) : base (propertyExpression, converter)
		{
		}

		public void ViewChanged (string val)
		{
			WritePropertyValue (val);
		}

		protected override void BindView ()
		{
		}

		protected override void UnbindView ()
		{
		}

		protected override void WriteViewValue (string val)
		{
			this.val = val;
		}
	}

}
