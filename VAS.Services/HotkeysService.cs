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
using VAS.Core;
using VAS.Core.Events;
using VAS.Core.Hotkeys;
using VAS.Core.Interfaces;
using VAS.Core.Store;

namespace VAS.Services
{
	/// <summary>
	/// Hotkeys service, use this service to register hotkeys than later can be obtained by other classes.
	/// </summary>
	public class HotkeysService : IHotkeysService
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
			App.Current.EventsBroker.Subscribe<EditEvent<KeyConfig>> (HandleEditKeyConfig);
			App.Current.EventsBroker.Subscribe<SaveEvent<KeyConfig>> (HandleSaveEvent);
			return true;
		}

		/// <summary>
		/// Stop the service.
		/// </summary>
		public bool Stop ()
		{
			App.Current.EventsBroker.Unsubscribe<EditEvent<KeyConfig>> (HandleEditKeyConfig);
			App.Current.EventsBroker.Unsubscribe<SaveEvent<KeyConfig>> (HandleSaveEvent);
			return true;
		}

		/// <summary>
		/// Register the specified keyConfig.
		/// </summary>
		/// <param name="keyConfig">Key config.</param>
		public void Register (KeyConfig keyConfig)
		{
			if (keyConfigs.Contains (keyConfig)) {
				throw new InvalidOperationException ("A KeyConfig with the same name is already registered");
			}
			keyConfigs.Add (keyConfig);
			ApplyConfig (keyConfig);
		}

		/// <summary>
		/// Register the specified list of keyConfigs
		/// </summary>
		/// <param name="keyConfig">Key config list</param>
		public void Register (IEnumerable<KeyConfig> keyConfig)
		{
			if (keyConfig.Except (keyConfigs).Count () != keyConfig.Count ()) {
				throw new InvalidOperationException ("A KeyConfig with the same name is already registered");
			}
			keyConfigs.AddRange (keyConfig);
			ApplyConfig (keyConfig);
		}

		/// <summary>
		/// Gets KeyConfig by the name
		/// </summary>
		/// <returns>The KeyConfig</returns>
		/// <param name="name">Name.</param>
		public KeyConfig GetByName (string name)
		{
			return keyConfigs.FirstOrDefault ((arg) => arg.Name == name);
		}

		/// <summary>
		/// Gets the KeyConfig 
		/// </summary>
		/// <returns>The by category.</returns>
		/// <param name="category">Category.</param>
		public IEnumerable<KeyConfig> GetByCategory (string category)
		{
			return keyConfigs.Where ((arg) => arg.Category == category);
		}

		/// <summary>
		/// Gets all KeyConfigs registered
		/// </summary>
		/// <returns>All registered Hotkeys configuration</returns>
		public IEnumerable<KeyConfig> GetAll ()
		{
			return keyConfigs;
		}

		void ApplyConfig (KeyConfig kconfig)
		{
			var keyconfig = App.Current.Config.KeyConfigs.FirstOrDefault ((k) => k.Name == kconfig.Name);
			if (keyconfig != null) {
				kconfig.Key = keyconfig.Key;
			}
		}

		void ApplyConfig (IEnumerable<KeyConfig> kconfigs)
		{
			foreach (var kconfig in kconfigs) {
				ApplyConfig (kconfig);
			}
		}

		void SaveToConfig ()
		{
			App.Current.Config.KeyConfigs = keyConfigs;
			App.Current.Config.Save ();
		}

		void SaveToConfig (KeyConfig kconfig)
		{
			if (!App.Current.Config.KeyConfigs.Contains (kconfig)) {
				App.Current.Config.KeyConfigs.Add (kconfig);
			} else {
				var keyConfig = App.Current.Config.KeyConfigs.FirstOrDefault ((arg) => arg.Name == kconfig.Name);
				if (keyConfig != null) {
					keyConfig = kconfig;
				}
			}
			App.Current.Config.Save ();
		}

		void HandleSaveEvent (SaveEvent<KeyConfig> e)
		{
			SaveToConfig ();
		}

		void HandleEditKeyConfig (EditEvent<KeyConfig> e)
		{
			HotKey hotkey = App.Current.GUIToolkit.SelectHotkey (e.Object.Key);
			if (hotkey != null) {
				//Should always be only on keyconfig
				var kconfig = keyConfigs.FirstOrDefault (kc => kc.Key != e.Object.Key && kc.Key == hotkey && kc.Configurable);
				if (kconfig != null) {
					string key = System.Security.SecurityElement.Escape (hotkey.ToString ());
					string msg = Catalog.GetString ($"Shortcut already in use\n{key}  is in use by  {kconfig.Category}/{kconfig.Description}  " +
												   $"replacing it will leave  {kconfig.Description}  without a shortcut, are you sure? ");
					msg = System.Security.SecurityElement.Escape (msg);
					List<string> buttons = new List<string> { Catalog.GetString ("Replace"),
						Catalog.GetString("Cancel"), Catalog.GetString("Try another key")};
					int res = App.Current.Dialogs.ButtonsMessage (msg, buttons, 2);
					switch (res) {
					case 1:
						kconfig.Key = new HotKey ();
						e.Object.Key = hotkey;
						break;
					case 2:
						e.ReturnValue = false;
						return;
					case 3:
						HandleEditKeyConfig (e);
						return;
					}
				} else {
					e.Object.Key = hotkey;
				}
			}
			if (e.AutoSave) {
				SaveToConfig (e.Object);
			}
			e.ReturnValue = true;
		}
	}
}
