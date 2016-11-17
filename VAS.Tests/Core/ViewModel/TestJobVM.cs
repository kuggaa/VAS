//
//  Copyright (C) 2016 Fluendo S.A.
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
using VAS.Core.ViewModel;
using VAS.Core.Common;

namespace VAS.Tests.Core.ViewModel
{
	[TestFixture]
	public class TestJobVM
	{

		[Test]
		public void TestModel ()
		{
			var encSettings = new EncodingSettings {
				OutputFile = "test.mp4",
			};
			var model = new Job (encSettings) {
			};
			var viewModel = new JobVM {
				Model = model
			};

			Assert.AreSame (model, viewModel.Model);
		}

		[Test]
		public void TestProperties ()
		{
			var encSettings = new EncodingSettings {
				OutputFile = "test.mp4",
			};
			var model = new Job (encSettings) {
				Progress = 0,
				State = JobState.Running,
			};
			var viewModel = new JobVM {
				Model = model
			};

			Assert.AreEqual ("test.mp4", viewModel.Name);
			Assert.AreEqual (0, viewModel.Progress);
			Assert.AreEqual (JobState.Running, viewModel.State);
		}

		[Test]
		public void TestPropertyForwarding ()
		{
			int count = 0;
			var encSettings = new EncodingSettings {
				OutputFile = "test.mp4",
			};
			var model = new Job (encSettings) {
				Progress = 0,
				State = JobState.Running,
			};
			var viewModel = new JobVM {
				Model = model,
			};
			viewModel.PropertyChanged += (sender, e) => {
				count++;
			};

			model.State = JobState.Error;
			Assert.AreEqual (1, count);
		}


	}
}
