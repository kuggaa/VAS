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
using NUnit.Framework;
using VAS.Core.MVVMC;

namespace VAS.Tests.MVVMC
{
	class DummyBinding : Binding
	{
		public int viewModelBindCount = 0, viewBindCount = 0;

		protected override void BindViewModel ()
		{
			viewModelBindCount++;
		}

		protected override void UnbindViewModel ()
		{
			viewModelBindCount--;
		}

		protected override void BindView ()
		{
			viewBindCount++;
		}

		protected override void UnbindView ()
		{
			viewBindCount--;
		}
	}

	[TestFixture]
	public class TestBinding
	{
		[Test]
		public void TestSetViewModel ()
		{
			DummyBinding binding = new DummyBinding ();
			binding.ViewModel = new ViewModelBase<BindableBase> ();

			Assert.AreEqual (1, binding.viewModelBindCount);
			Assert.AreEqual (1, binding.viewBindCount);
		}

		[Test]
		public void TestViewNotBindedTwice ()
		{
			DummyBinding binding = new DummyBinding ();
			binding.ViewModel = new ViewModelBase<BindableBase> ();
			binding.ViewModel = new ViewModelBase<BindableBase> ();

			Assert.AreEqual (1, binding.viewBindCount);
		}

		[Test]
		public void TestUnbindViewInDispose ()
		{
			var viewModel = new ViewModelBase<BindableBase> ();

			DummyBinding binding = new DummyBinding ();
			binding.ViewModel = viewModel;
			binding.Dispose ();

			Assert.AreEqual (0, binding.viewBindCount);
		}

		[Test]
		public void TestBindNewViewModel ()
		{
			DummyBinding binding = new DummyBinding ();
			binding.ViewModel = new ViewModelBase<BindableBase> ();
			binding.ViewModel = new ViewModelBase<BindableBase> ();
			binding.Dispose ();

			Assert.AreEqual (1, binding.viewModelBindCount);
		}
	}
}
