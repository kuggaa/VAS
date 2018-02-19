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
using System.IO;
using System.Reflection;
using System.Collections.Generic;

namespace VAS.Tests.Core
{
	[TestFixture ()]
	public class TestResources
	{
		IResourcesLocator resources;

		[OneTimeSetUp]
		public void Setup ()
		{
			App.Current.DataDir.Add (Path.Combine(TestContext.CurrentContext.TestDirectory, "data"));
			resources = new ResourcesLocator ();
		}

		[Test ()]
		public void TestLoadIconResource ()
		{
			Image img = resources.LoadImage ("vas-dark-bg.svg");
			Assert.IsNotNull (img);
		}

		[Test ()]
		public void TestLoadImageResource ()
		{
			Image img = resources.LoadImage ("vas-longomatch.svg");
			Assert.IsNotNull (img);
		}

		[Test ()]
		public void TestLoadInvalidResource ()
		{
			Assert.Throws<System.IO.FileNotFoundException> (
				delegate {
					var img = resources.LoadImage ("not-found.svg");
				});
		}

		[Test ()]
		public void GetEmbeddedResourceFileStream_NotRegisteredAssembly_ReturnsImage ()
		{
			// Arrange
			DummyLocator locator = new DummyLocator ();

			// Action
			var stream = locator.GetEmbeddedResourceFileStream ("vas-dibujo.svg");

			// Assert
			Assert.IsNotNull (stream);
			Assert.AreEqual (1, locator.Assemblies.Count);
		}

		[Test ()]
		public void GetEmbeddedResourceFileStream_RegisteredAssembly_ReturnsImage ()
		{
			// Arrange
			DummyLocator locator = new DummyLocator ();
			locator.Register (Assembly.GetExecutingAssembly ());

			// Action
			var stream = locator.GetEmbeddedResourceFileStream ("vas-dibujo.svg");

			// Assert
			Assert.IsNotNull (stream);
			Assert.AreEqual (1, locator.Assemblies.Count);
		}

		class DummyLocator : ResourcesLocatorBase
		{
			public HashSet<Assembly> Assemblies {
				get => assemblies;
			}

			public override Image LoadEmbeddedImage (string resourceId, int width = 0, int height = 0)
			{
				throw new System.NotImplementedException ();
			}

			public override Image LoadImage (string name, int width = 0, int height = 0)
			{
				throw new System.NotImplementedException ();
			}
		}
	}
}

