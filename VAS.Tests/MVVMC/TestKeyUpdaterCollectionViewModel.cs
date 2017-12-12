//
//  Copyright (C) 2017 
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
using NUnit.Framework;
using VAS.Core.MVVMC;
using VAS.Core.Store;
using VAS.Core.ViewModel;

namespace VAS.Tests.MVVMC
{
	public class TestKeyUpdaterCollectionViewModel
	{
		KeyUpdaterCollectionViewModel<Tag, TagVM> tags;

		[SetUp]
		public void SetUp ()
		{
			tags = new KeyUpdaterCollectionViewModel<Tag, TagVM> ();
			TagVM vm1 = new TagVM { Model = new Tag ("success", "test") };
			TagVM vm2 = new TagVM { Model = new Tag ("failure", "test") };
			tags.ViewModels.AddRange (new List<TagVM> {vm1, vm2});
		}

		[Test]
		public void RemoveViewModel_HasCodeChanged_DictionaryRecreatedAndElementRemoved ()
		{
			// Arrange
			TagVM t = tags.ViewModels [0];
			t.Value = "value changed";

			// Act
			tags.ViewModels.Remove (t);

			// Assert
			Assert.AreEqual (1, tags.ViewModels.Count);
		}

		[Test]
		public void UpdateViewModelValue_HasCodeChanged_DictionaryRecreatedAndOrderKept ()
		{
			// Arrange
			TagVM t = tags.ViewModels [0];

			// Act
			t.Value = "value changed";

			// Assert
			Assert.AreEqual (2, tags.ViewModels.Count);
			Assert.AreEqual (t.Model, tags.ViewModels[0].Model);
		}

		[Test]
		public void RemoveElement_UpdateItAfterRemove_NotAddedToDictionary ()
		{
			// Arrange
			var t = tags.ViewModels [0];
			tags.ViewModels.Remove (t);

			// Act
			t.Value = "not update";

			// Assert
			Assert.AreEqual (1, tags.ViewModels.Count);
		}

		[Test]
		public void ResetViewModels_UpdateSubscription_KeyUpdatedAndPossibleToRemove ()
		{
			// Arrange
			TagVM newVM = new TagVM { Model = new Tag ("new tag", "test") };
			TagVM newVM2 = new TagVM { Model = new Tag ("new tag 2", "test") };

			// Act
			tags.ViewModels.Reset (new List<TagVM> { newVM, newVM2 });
			tags.ViewModels.Remove (newVM);

			// Assert
			Assert.AreEqual (1, tags.ViewModels.Count);
		}
	}
}
