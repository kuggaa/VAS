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
using Gtk;
using VAS.Core.Common;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.ViewModel;
using VAS.UI.Helpers;

namespace VAS.UI.Menus
{
	/// <summary>
	/// Menu base class that creates a Menu based on a MenuVM passed to the view
	/// </summary>
	public class MenuBase : Menu, IView<MenuVM>
	{
		MenuVM menuVM;

		public override void Dispose ()
		{
			Dispose (true);
			base.Dispose ();
		}

		protected virtual void Dispose (bool disposing)
		{
			if (Disposed) {
				return;
			}
			if (disposing) {
				Destroy ();
			}
			Disposed = true;
		}

		protected override void OnDestroyed ()
		{
			Log.Verbose ($"Destroying {GetType ()}");
			base.OnDestroyed ();

			Disposed = true;
		}

		protected bool Disposed { get; private set; } = false;

		public MenuVM ViewModel {
			get {
				return menuVM;
			}
			set {
				if (menuVM != null) {
					RemoveMenu ();
				}
				menuVM = value;
				if (menuVM != null) {
					CreateMenu (menuVM);
				}
			}
		}

		public void SetViewModel (object viewModel)
		{
			ViewModel = (MenuVM)viewModel;
		}

		public void ShowMenu ()
		{
			if (ViewModel == null) {
				throw new InvalidOperationException ("A MenuVM is not currently set");
			}
			ShowAll ();
			Popup ();
		}

		void CreateMenu (MenuVM menuvm, Menu menu = null)
		{
			if (menu == null) {
				menu = this;
			}
			foreach (var menuNode in menuvm.ViewModels) {
				if (menuNode.Command != null) {
					menu.Append (menuNode.CreateMenuItem ());
				} else if (menuNode.Submenu != null) {
					var item = new MenuItem (menuNode.Name);
					item.Submenu = new Menu ();
					CreateMenu (menuNode.Submenu, (Menu)item.Submenu);
					menu.Append (item);
				}
			}
		}

		void RemoveMenu ()
		{
			foreach (var item in Children) {
				Remove (item);
			}
		}
	}
}
