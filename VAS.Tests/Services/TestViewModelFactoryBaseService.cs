//
//  Copyright (C) 2018 Fluendo S.A.
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
using NUnit.Framework;
using VAS.Core.Interfaces;
using VAS.Core.Store;
using VAS.Core.Store.Playlists;
using VAS.Core.ViewModel;
using VAS.Services;
using VAS.Core.Common;

namespace VAS.Tests.Services
{
	[TestFixture]
	public class TestViewModelFactoryBaseService
	{
		VMFactoryServiceTest factoryService;

		[SetUp]
		public void SetUp ()
		{
			factoryService = new VMFactoryServiceTest ();
		}

		[Test]
		public void ViewModelFactoryBaseService_TypeMappingsInitialized ()
		{
			Assert.IsTrue (factoryService.TypeMappings.ContainsKey (typeof (PlaylistPlayElement)));
			Assert.AreEqual (factoryService.TypeMappings [typeof (PlaylistPlayElement)], typeof (PlaylistPlayElementVM));
			Assert.IsTrue (factoryService.TypeMappings.ContainsKey (typeof (PlaylistVideo)));
			Assert.AreEqual (factoryService.TypeMappings [typeof (PlaylistVideo)], typeof (PlaylistVideoVM));
			Assert.IsTrue (factoryService.TypeMappings.ContainsKey (typeof (PlaylistImage)));
			Assert.AreEqual (factoryService.TypeMappings [typeof (PlaylistImage)], typeof (PlaylistImageVM));
			Assert.IsTrue (factoryService.TypeMappings.ContainsKey (typeof (PlaylistDrawing)));
			Assert.AreEqual (factoryService.TypeMappings [typeof (PlaylistDrawing)], typeof (PlaylistDrawingVM));
			Assert.IsTrue (factoryService.TypeMappings.ContainsKey (typeof (AnalysisEventButton)));
			Assert.AreEqual (factoryService.TypeMappings [typeof (AnalysisEventButton)], typeof (AnalysisEventButtonVM));
			Assert.IsTrue (factoryService.TypeMappings.ContainsKey (typeof (TagButton)));
			Assert.AreEqual (factoryService.TypeMappings [typeof (TagButton)], typeof (TagButtonVM));
			Assert.IsTrue (factoryService.TypeMappings.ContainsKey (typeof (TimerButton)));
			Assert.AreEqual (factoryService.TypeMappings [typeof (TimerButton)], typeof (TimerButtonVM));
			Assert.AreEqual (factoryService.TypeMappings.Count, 7);
		}

		[Test]
		public void ViewModelFactoryBaseService_AddTypeMappings ()
		{
			factoryService.AddTypeMapping (typeof (Period), typeof (PeriodVM));

			Assert.IsTrue (factoryService.TypeMappings.ContainsKey (typeof (Period)));
			Assert.AreEqual (factoryService.TypeMappings [typeof (Period)], typeof (PeriodVM));
			Assert.AreEqual (factoryService.TypeMappings.Count, 8);
		}

		[Test]
		public void ViewModelFactoryBaseService_CreateViewModel_NotInTypeMappings ()
		{
			var period = new Period ();
			var vm = factoryService.CreateViewModel<PeriodVM, Period> (period);

			Assert.IsTrue (vm is PeriodVM);
		}

		[Test]
		public void ViewModelFactoryBaseService_CreatePlaylistElementVM ()
		{
			var model = new PlaylistPlayElement (new TimelineEvent ());
			var vm = factoryService.CreateViewModel<PlaylistElementVM, IPlaylistElement> (model);

			Assert.IsTrue (vm is PlaylistPlayElementVM);
		}

		[Test]
		public void ViewModelFactoryBaseService_CreatePlaylistVideoVM ()
		{
			var model = new PlaylistVideo (new MediaFile ());
			var vm = factoryService.CreateViewModel<PlaylistElementVM, IPlaylistElement> (model);

			Assert.IsTrue (vm is PlaylistVideoVM);
		}

		[Test]
		public void ViewModelFactoryBaseService_CreatePlaylistImageVM ()
		{
			var model = new PlaylistImage (new Image (2, 2), new Time (1));
			var vm = factoryService.CreateViewModel<PlaylistElementVM, IPlaylistElement> (model);

			Assert.IsTrue (vm is PlaylistImageVM);
		}

		[Test]
		public void ViewModelFactoryBaseService_CreatePlaylistDrawingVM ()
		{
			var model = new PlaylistDrawing (new FrameDrawing ());
			var vm = factoryService.CreateViewModel<PlaylistElementVM, IPlaylistElement> (model);

			Assert.IsTrue (vm is PlaylistDrawingVM);
		}

		[Test]
		public void ViewModelFactoryBaseService_CreateAnalysisEventButtonVM ()
		{
			var model = new AnalysisEventButton ();
			var vm = factoryService.CreateViewModel<DashboardButtonVM, DashboardButton> (model);

			Assert.IsTrue (vm is AnalysisEventButtonVM);
		}

		[Test]
		public void ViewModelFactoryBaseService_CreateTagButtonVM ()
		{
			var model = new TagButton ();
			var vm = factoryService.CreateViewModel<DashboardButtonVM, DashboardButton> (model);

			Assert.IsTrue (vm is TagButtonVM);
		}

		[Test]
		public void ViewModelFactoryBaseService_CreateTimerButtonVM ()
		{
			var model = new TimerButton ();
			var vm = factoryService.CreateViewModel<DashboardButtonVM, DashboardButton> (model);

			Assert.IsTrue (vm is TimerButtonVM);
		}
	}

	/// <summary>
	/// VM Factory service test. Just to get the typeMappings in test
	/// </summary>
	class VMFactoryServiceTest : ViewModelFactoryBaseService
	{
		public new Dictionary<Type, Type> TypeMappings {
			get {
				return base.TypeMappings;
			}
		}
	}
}
