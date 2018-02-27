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
using NUnit.Framework;
using VAS.Core.Store;
using VAS.Core.Store.Templates;
using VAS.Core.ViewModel;
using VAS.Services.ViewModel;
namespace VAS.Tests.Core.ViewModel
{
	[TestFixture]
	public class TestTemplateManagerVM
	{
		TemplatesManagerViewModel<Dashboard, DashboardVM, DashboardButton, DashboardButtonVM> manager;

		[SetUp]
		public void SetUp ()
		{
			manager = new TemplatesManagerViewModel<Dashboard, DashboardVM, DashboardButton, DashboardButtonVM> ();
		}

		[Test]
		public void DeleteCommand_LoadedTemplateEditable_Executable ()
		{
			manager.LoadedItem.Model = new Utils.DashboardDummy ();

			Assert.IsTrue (manager.DeleteCommand.CanExecute ());
		}

		[Test]
		public void DeleteCommand_LoadedTemplateNotEditable_NotExecutable ()
		{
			manager.LoadedItem.Model = new Utils.DashboardDummy ();
			manager.LoadedItem.Model.Static = true;

			Assert.IsFalse (manager.DeleteCommand.CanExecute ());
		}

		[Test]
		public void DeleteCommand_LoadedTemplateEmpty_NotExecutable ()
		{
			Assert.IsFalse (manager.DeleteCommand.CanExecute ());
		}
	}
}
