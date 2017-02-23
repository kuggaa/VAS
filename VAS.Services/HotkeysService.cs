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
using System.Linq;
using VAS.Core.Hotkeys;
using VAS.Core.Interfaces;

namespace VAS.Services
{
	/// <summary>
	/// Hotkeys service, use this service to register hotkeys than later can be obtained by other classes.
	/// </summary>
	public class HotkeysService : IService
	{
		List<KeyConfig> keyConfigs;

		public HotkeysService ()
		{
			keyConfigs = new List<KeyConfig> ();
		}

		/// <summary>
		/// Gets the level of the service. Services are started in ascending level order and stopped in descending level order.
		/// </summary>
		/// <value>The level.</value>
		public int Level {
			get {
				return 40;
			}
		}

		/// <summary>
		/// Gets the name of the service
		/// </summary>
		/// <value>The name of the service.</value>
		public string Name {
			get {
				return "Hotkeys";
			}
		}

		/// <summary>
		/// Start the service.
		/// </summary>
		public bool Start ()
		{
			return true;
		}

		/// <summary>
		/// Stop the service.
		/// </summary>
		public bool Stop ()
		{
			return true;
		}

		public void Register (KeyConfig keyConfig)
		{
			//check for keyConfigs with same name to throw exception
			keyConfigs.Add (keyConfig);
		}

		public void Register (IEnumerable<KeyConfig> keyConfig)
		{
			//check for keyConfigs with same name to throw exception
			keyConfigs.AddRange (keyConfig);
		}

		public KeyConfig GetByName (string name)
		{
			return keyConfigs.FirstOrDefault ((arg) => arg.Name == name);
		}
	}
}
