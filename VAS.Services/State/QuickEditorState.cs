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
using System.Threading.Tasks;
using VAS.Core.Common;
using VAS.Core.ViewModel;
using VAS.Services.ViewModel;

namespace VAS.Services.State
{
	/// <summary>
	/// State for the Quick Editor that allows editing a video file quickly without creating an analysis project.
	/// </summary>
	public class QuickEditorState : ScreenState<QuickEditorVM>
	{
		public const string NAME = "QuickEditor";

		public override string Name => NAME;

		public async override Task<bool> LoadState (dynamic data)
		{
			Log.Debug ($"Loading state {Name}");
			await Initialize (data);
			return true;
		}

		protected override void CreateViewModel (dynamic data)
		{
			ViewModel = new QuickEditorVM ();
			if (data is MediaFileVM file) {
				ViewModel.VideoFile = file;
			}
		}

		protected override void CreateControllers (dynamic data)
		{
			var playerController = new VideoPlayerController ();
			playerController.SetViewModel (ViewModel.VideoPlayer);
			Controllers.Add (playerController);
		}
	}
}
