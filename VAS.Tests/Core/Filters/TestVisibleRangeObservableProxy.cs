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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using VAS.Core.Common;
using VAS.Core.Filters;
using VAS.Core.Interfaces.GUI;
using VAS.Core.MVVMC;

namespace VAS.Tests.Core.Filters
{
	[TestFixture]
	public class TestVisibleRangeObservableProxy
	{
		public class VisibleItem : ViewModelBase, IVisible
		{
			bool visible;

			public bool Visible {
				get {
					return visible;
				}

				set {
					visible = value;
					OnPropertyChanged (new System.ComponentModel.PropertyChangedEventArgs (nameof (Visible)));
				}
			}
		}

		[Test ()]
		public void AddItems_SomeVisiblesOthersNot_GetsOnlyVisibleItems ()
		{
			///Arrange
			var visibleItemsList = new ObservableCollection<VisibleItem> ();

			for (int i = 0; i < 10; i++) {
				visibleItemsList.Add (new VisibleItem () {
					Visible = (i % 2 == 0)
				});
			}

			///Act

			var target = new VisibleRangeObservableProxy<VisibleItem> (visibleItemsList);

			///Assert

			Assert.AreEqual (5, target.Count);
		}

		[Test ()]
		public void ApplyPropertyChanges_ModifySomeVisiblesSourceItemsToNotVisible_GetsOnlyVisibleItems ()
		{
			///Arrange
			var visibleItemsList = new ObservableCollection<VisibleItem> ();

			for (int i = 0; i < 10; i++) {
				visibleItemsList.Add (new VisibleItem () {
					Visible = (i % 2 == 0)
				});
			}

			///Act

			var target = new VisibleRangeObservableProxy<VisibleItem> (visibleItemsList);

			foreach (var item in visibleItemsList) {
				item.Visible = false;
			}
			target.ApplyPropertyChanges ();

			foreach (var item in visibleItemsList) {
				item.Visible = true;
			}
			target.ApplyPropertyChanges ();

			///Assert

			Assert.AreEqual (10, target.Count);
		}

		[Test ()]
		public void ApplyPropertyChanges_ModifySomeVisiblesSourceItemsToNotVisibleAndDeleteSomeOriginalViewModels_GetsOnlyVisibleItems ()
		{
			///Arrange
			var visibleItemsList = new ObservableCollection<VisibleItem> ();

			for (int i = 0; i < 10; i++) {
				visibleItemsList.Add (new VisibleItem () {
					Visible = (i % 2 == 0)
				});
			}

			///Act

			var target = new VisibleRangeObservableProxy<VisibleItem> (visibleItemsList);

			foreach (var item in visibleItemsList) {
				item.Visible = false;
			}
			target.ApplyPropertyChanges ();

			foreach (var item in visibleItemsList) {
				item.Visible = true;
			}
			target.ApplyPropertyChanges ();

			visibleItemsList.RemoveAt (0);

			///Assert

			Assert.AreEqual (9, target.Count);
		}

		[Test ()]
		public void ApplyPropertyChanges_ModifySomeVisiblesSourceItemsToNotVisibleAndAddSomeToOriginalViewModelsList_GetsOnlyVisibleItems ()
		{
			///Arrange
			var visibleItemsList = new ObservableCollection<VisibleItem> ();

			for (int i = 0; i < 10; i++) {
				visibleItemsList.Add (new VisibleItem () {
					Visible = (i % 2 == 0)
				});
			}

			///Act

			var target = new VisibleRangeObservableProxy<VisibleItem> (visibleItemsList);

			foreach (var item in visibleItemsList) {
				item.Visible = false;
			}
			target.ApplyPropertyChanges ();

			foreach (var item in visibleItemsList) {
				item.Visible = true;
			}
			target.ApplyPropertyChanges ();

			visibleItemsList.Add (new VisibleItem () {
				Visible = true
			});

			///Assert

			Assert.AreEqual (11, target.Count);
		}

		[Test ()]
		public void ApplyPropertyChanges_AddSomeVisiblesOthersNotAndDeleteOneItemOnOriginalList_GetsOnlyVisibleItems ()
		{
			///Arrange
			var visibleItemsList = new ObservableCollection<VisibleItem> ();

			for (int i = 0; i < 10; i++) {
				visibleItemsList.Add (new VisibleItem () {
					Visible = (i % 2 == 0)
				});
			}

			///Act

			var target = new VisibleRangeObservableProxy<VisibleItem> (visibleItemsList);
			visibleItemsList.RemoveAt (0);

			///Assert

			Assert.AreEqual (4, target.Count);
		}

		[Test ()]
		public void ApplyPropertyChanges_AddSomeVisiblesOthersNotAndClearOriginalList_GetsNoItems ()
		{
			///Arrange
			var visibleItemsList = new ObservableCollection<VisibleItem> ();

			for (int i = 0; i < 10; i++) {
				visibleItemsList.Add (new VisibleItem () {
					Visible = (i % 2 == 0)
				});
			}

			///Act

			var target = new VisibleRangeObservableProxy<VisibleItem> (visibleItemsList);
			visibleItemsList.Clear ();
			target.ApplyPropertyChanges ();

			///Assert

			Assert.AreEqual (0, target.Count);
		}

		[Test ()]
		public void ApplyPropertyChanges_AddSomeVisiblesOthersNotAndReplaceOriginalList_GetsNewVisibleItems ()
		{
			///Arrange
			var visibleItemsList = new RangeObservableCollection<VisibleItem> ();

			for (int i = 0; i < 10; i++) {
				visibleItemsList.Add (new VisibleItem () {
					Visible = (i % 2 == 0)
				});
			}

			///Act

			var target = new VisibleRangeObservableProxy<VisibleItem> (visibleItemsList);


			var newVisibleItemsList = new RangeObservableCollection<VisibleItem> ();

			for (int i = 0; i < 5; i++) {
				newVisibleItemsList.Add (new VisibleItem () {
					Visible = (i % 2 == 0)
				});
			}

			visibleItemsList.Reset (newVisibleItemsList);
			target.ApplyPropertyChanges ();

			///Assert

			Assert.AreEqual (3, target.Count);
		}
	}
}
