//
//  Copyright (C) 2017 
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
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using VAS.Core;
using VAS.Core.Events;
using VAS.Core.Store;
using VAS.Core.ViewModel;
using VAS.Services.Controller;

namespace VAS.Tests.Services
{
	[TestFixture]
	public class TestMediaFileSetController
	{
		MediaFileSetController controller;
		Mock<IDialogs> dialogMock;

		[OneTimeSetUp]
		public void OneTimeSetUp ()
		{
			controller = new MediaFileSetController ();
			dialogMock = new Mock<IDialogs> ();
			App.Current.Dialogs = dialogMock.Object;
		}

		[SetUp]
		public async Task SetUp ()
		{
			await controller.Start ();
		}

		[TearDown]
		public async Task TearDown ()
		{
			await controller.Stop ();
		}

		[Test]
		public async Task Replace_FileContained_Replaced ()
		{
			dialogMock.Setup (d => d.OpenMediaFile (It.IsAny<object> ())).Returns (new MediaFile { FilePath = "new" });

			MediaFileSet fileSet = new MediaFileSet ();
			MediaFileSetVM fileSetVM = new MediaFileSetVM { Model = fileSet };
			fileSet.Add (new MediaFile { FilePath = "old" });

			MediaFileVM newFileVM = await App.Current.EventsBroker.PublishWithReturn<ReplaceMediaFileEvent, MediaFileVM> (new ReplaceMediaFileEvent {
				OldFileSet = fileSetVM,
				OldFile = fileSetVM.ViewModels.First (),
			});

			Assert.IsNotNull (newFileVM);
			Assert.IsNotNull (newFileVM.Model);
			Assert.AreEqual ("new", newFileVM.FilePath);
			Assert.AreEqual ("new", newFileVM.Model.FilePath);
			Assert.AreEqual (newFileVM, fileSetVM.ViewModels.First ());
			Assert.AreEqual (1, fileSetVM.Count ());
		}

		[Test]
		public async Task Replace_FileNotContained_Added ()
		{
			dialogMock.Setup (d => d.OpenMediaFile (It.IsAny<object> ())).Returns (new MediaFile { FilePath = "new" });

			MediaFileSet fileSet = new MediaFileSet ();
			MediaFileSetVM fileSetVM = new MediaFileSetVM { Model = fileSet };
			fileSet.Add (new MediaFile { FilePath = "old" });

			MediaFileVM newFileVM = await App.Current.EventsBroker.PublishWithReturn<ReplaceMediaFileEvent, MediaFileVM> (new ReplaceMediaFileEvent {
				OldFileSet = fileSetVM,
				OldFile = new MediaFileVM { Model = new MediaFile { FilePath = "not contained" } },
			});

			Assert.IsNotNull (newFileVM);
			Assert.IsNotNull (newFileVM.Model);
			Assert.AreEqual ("new", newFileVM.FilePath);
			Assert.AreEqual ("new", newFileVM.Model.FilePath);
			Assert.AreEqual (2, fileSetVM.Count ());
		}

		[Test]
		public async Task Replace_FileNotSelected_NothingChanged ()
		{
			dialogMock.Setup (d => d.OpenMediaFile (It.IsAny<object> ())).Returns<MediaFile> (null);

			MediaFileSet fileSet = new MediaFileSet ();
			MediaFileSetVM fileSetVM = new MediaFileSetVM { Model = fileSet };
			fileSet.Add (new MediaFile { FilePath = "old" });

			MediaFileVM newFileVM = await App.Current.EventsBroker.PublishWithReturn<ReplaceMediaFileEvent, MediaFileVM> (new ReplaceMediaFileEvent {
				OldFileSet = fileSetVM,
				OldFile = fileSetVM.ViewModels.First (),
			});

			Assert.IsNotNull (newFileVM);
			Assert.IsNotNull (newFileVM.Model);
			Assert.AreSame (fileSetVM.ViewModels.First (), newFileVM);
			Assert.AreEqual (fileSetVM.ViewModels.First ().Model, newFileVM.Model);
			Assert.AreEqual (1, fileSetVM.Count ());
		}
	}
}
