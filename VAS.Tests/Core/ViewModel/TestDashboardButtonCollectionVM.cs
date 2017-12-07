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
using System.Linq;
using NUnit.Framework;
using VAS.Core.Common;
using VAS.Core.Store;
using VAS.Core.ViewModel;

namespace VAS.Tests.Core.ViewModel
{
	public class TestDashboardButtonCollectionVM
	{
		RangeObservableCollection<DashboardButton> model;
		AnalysisEventButton A, B, C;

		[SetUp]
		public void SetUp () {
			// load model
			EventType e = new AnalysisEventType { Tags = new RangeObservableCollection<Tag> { new Tag ("Success", "Test") }};
			A = new AnalysisEventButton { Name = "A", EventType = e };
			B = new AnalysisEventButton { Name = "B", EventType = e };
			C = new AnalysisEventButton { Name = "C", EventType = e };
			model = new RangeObservableCollection<DashboardButton> ();
			model.AddRange (new List<DashboardButton> { A, B, C });
			CreateLink (A, B);
			CreateLink (B, C);
		}

		[Test]
		public void SetModel_LinkToNotCreatedYetButtonVM_LinksShareSourceDestinationButtonVMs()
		{
			// Arrange
			DashboardButtonCollectionVM vm = new DashboardButtonCollectionVM ();

			// Act
			vm.Model = model;

			// Assert
			Assert.AreEqual (B, A.ActionLinks [0].DestinationButton);
			Assert.AreEqual (C, B.ActionLinks [0].DestinationButton);
		}

		[Test]
		public void RenameTag_TagsInstancesDifferent_UpdateAndShareSameInstanceBetweenLinkAndButton ()
		{
			// Arrange
			DashboardButtonCollectionVM vm = new DashboardButtonCollectionVM ();
			// same values but different instance
			A.ActionLinks [0].DestinationTags [0] = new Tag ("Success", "Test");
			vm.Model = model;

			// Act
			Tag tagToUpdate = B.AnalysisEventType.Tags [0];
			tagToUpdate.Value = "NewName";

			// Assert
			Assert.AreEqual ("NewName", A.ActionLinks [0].DestinationTags[0].Value);
		}

		void CreateLink(AnalysisEventButton source, AnalysisEventButton target) {
			ActionLink link = new ActionLink { SourceButton = source, DestinationButton = target };
			link.SourceTags.Add (source.AnalysisEventType.Tags.First());
			link.DestinationTags.Add (target.AnalysisEventType.Tags.First ());
			source.ActionLinks.Add (link);
		}
	}
}
