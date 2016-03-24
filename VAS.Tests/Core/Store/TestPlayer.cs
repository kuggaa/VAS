//
//  Copyright (C) 2014 Andoni Morales Alastruey
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
using VAS.Core.Store;


namespace VAS.Tests.Core.Store
{
	[TestFixture ()]
	public class TestPlayer
	{
		[Test ()]
		public void TestSerialization ()
		{
			Utils.PlayerDummy player = new Utils.PlayerDummy {
				Name = "andoni",
				Nationality = "spanish",
				Mail = "test@test",
				Color = Color.Red
			};
			Utils.CheckSerialization (player);

			Player newPlayer = Utils.SerializeDeserialize (player);
			Assert.AreEqual (player.Name, newPlayer.Name);
			Assert.AreEqual (player.Nationality, newPlayer.Nationality);
			Assert.AreEqual (player.Mail, newPlayer.Mail);
			Assert.IsNull (newPlayer.Color);
		}

		[Test ()]
		public void TestToString ()
		{
			Utils.PlayerDummy player = new Utils.PlayerDummy { Name = "andoni", LastName = "morales" };
			Assert.AreEqual ("andoni morales", player.ToString ());
			player.NickName = "ylatuya";
			Assert.AreEqual ("ylatuya", player.ToString ());
		}

		[Test ()]
		public void TestPhoto ()
		{
			Utils.PlayerDummy player = new Utils.PlayerDummy { Name = "andoni", Nationality = "spanish" };
			player.Photo = Utils.LoadImageFromFile ();
			Utils.CheckSerialization (player);
			Assert.AreEqual (player.Photo.Width, 16);
			Assert.AreEqual (player.Photo.Height, 16);
		}
	}
}

