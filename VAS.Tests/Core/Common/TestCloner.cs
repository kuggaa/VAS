//
//  Copyright (C) 2015 Fluendo S.A.
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
using Moq;
using NUnit.Framework;
using VAS.Core.Common;
using VAS.Core.Interfaces;
using VAS.Core.Store;
using VAS.Core.Store.Templates;

namespace VAS.Tests.Core.Common
{
	[TestFixture]
	public class TestCloner
	{
		[Test ()]
		public void TestClone_IsBindableBase_ChangeCollectionProperty ()
		{
			// Arrange
			Team team = new Team ();
			var clonedTeam = team.Clone ();

			team.IsChanged = false;

			// Action
			team.List.Add (new Utils.PlayerDummy ());

			// Assert
			Assert.IsTrue (team.IsChanged);
		}
	}
}
