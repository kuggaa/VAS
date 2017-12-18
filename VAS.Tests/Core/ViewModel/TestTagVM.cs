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
using VAS.Core.Store;
using VAS.Core.ViewModel;

namespace VAS.Tests.Core.ViewModel
{
	[TestFixture]
	public class TestTagVM
	{
		Tag tag;

		[SetUp]
		public void SetUp ()
		{
			tag = new Tag ("test");
			tag.HotKey = new HotKey {
				Key = 1,
			};
		}

		[Test]
		public void TagVM_SetModel_SetsHotKeyModel ()
		{
			TagVM tagVM = new TagVM ();

			tagVM.Model = tag;

			Assert.IsNotNull (tagVM.HotKey.Model);
			Assert.AreEqual (1, tagVM.HotKey.Key);
		}

		[Test]
		public void TagVM_ModifyHotkey_ForwardsPropertyChange ()
		{
			string propName = null;
			int times = 0;
			Tag tag = new Tag ("test");
			tag.HotKey = new HotKey {
				Key = 1,
			};
			TagVM tagVM = new TagVM ();
			tagVM.Model = tag;
			tagVM.PropertyChanged += (s, e) => {
				propName = e.PropertyName;
				times++;
			};

			tag.HotKey.Key = 2;

			//FIXME: It should be once when the forwarding is fixed
			Assert.AreEqual (2, times);
			Assert.AreEqual ("Key", propName);
			Assert.AreEqual (tagVM.HotKey.Key, tag.HotKey.Key);
		}
	}
}
