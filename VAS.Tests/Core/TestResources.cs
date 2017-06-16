//
//  Copyright (C) 2015 andoni
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
using VAS.Core.Common;
using VAS.Core;
using VAS.Core.Interfaces;

namespace VAS.Tests.Core
{
	[TestFixture ()]
	public class TestResources
	{
		IResourcesLocator resources;

		[TestFixtureSetUp]
		public void Setup ()
		{
			//FIXME: RelativePrefix cannot be used
			App.Current.DataDir.Add ("./data/");
			resources = new ResourcesLocator ();
		}

		[Test ()]
		public void TestLoadIconResource ()
		{
			Image img = App.Current.ResourcesLocator.LoadImage ("vas-dark-bg.svg");
			Assert.IsNotNull (img);
		}

		[Test ()]
		public void TestLoadImageResource ()
		{
			Image img = App.Current.ResourcesLocator.LoadImage ("vas-longomatch.svg");
			Assert.IsNotNull (img);
		}

		[Test ()]
		public void TestLoadInvalidResource ()
		{
			Assert.Throws<System.IO.FileNotFoundException> (
				delegate {
					var img = App.Current.ResourcesLocator.LoadImage ("not-found.svg");
				});
		}
	}
}

