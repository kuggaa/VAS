﻿//
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
using System.Threading.Tasks;
using NUnit.Framework;
using VAS.Core.MVVMC;

namespace VAS.Tests.MVVMC
{
	[TestFixture]
	public class TestCommand
	{
		[Test]
		public void TestCommandInitExecutable ()
		{
			Command command = new Command ((obj) => { });

			Assert.IsTrue (command.CanExecute ());
		}

		[Test]
		public void Execute_CommandBeingExecuted_NoExecutionDone ()
		{
			// Arrange
			int operationsExecuted = 0;
			Command command = new Command (x => {
				operationsExecuted++;
				int totalCount = 0;
				while (totalCount < 500) {
					totalCount++;		
				}
			});

			// Act
			Task.WaitAll (
				Task.Factory.StartNew (() => command.Execute ()),
				Task.Factory.StartNew (() => command.Execute ())
			);

			// Assert
			Assert.AreEqual (1, operationsExecuted);
		}
	}
}
