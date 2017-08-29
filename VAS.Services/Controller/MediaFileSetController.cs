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
using VAS.Core.Events;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
using VAS.Core.Store;
using VAS.Core.ViewModel;

namespace VAS.Services.Controller
{
	/// <summary>
	/// Media file set controller.
	/// </summary>
	public class MediaFileSetController : ControllerBase
	{

		protected override void DisposeManagedResources ()
		{
			base.DisposeManagedResources ();
		}

		public override async Task Start ()
		{
			await base.Start ();
			App.Current.EventsBroker.Subscribe<ReplaceMediaFileEvent> (HandleReplaceMediaFile);
		}

		public override async Task Stop ()
		{
			await base.Stop ();
			App.Current.EventsBroker.Unsubscribe<ReplaceMediaFileEvent> (HandleReplaceMediaFile);
		}

		void HandleReplaceMediaFile (ReplaceMediaFileEvent evt)
		{
			MediaFile file = App.Current.Dialogs.OpenMediaFile ();
			if (file == null) {
				evt.ReturnValue = evt.OldFile;
				return;
			}
			MediaFileVM fileVM = new MediaFileVM { Model = file };
			Replace (evt.OldFileSet, evt.OldFile, fileVM);
			evt.ReturnValue = fileVM;
		}

		public override void SetViewModel (IViewModel viewModel)
		{
		}

		// Copied and migrated from MediaFileSet.cs
		bool Replace (MediaFileSetVM fileSet, MediaFileVM oldFile, MediaFileVM newFile)
		{
			bool found = false;

			if (fileSet.Contains (oldFile)) {
				if (newFile != null && oldFile != null) {
					newFile.Name = oldFile.Name;
					newFile.Offset = oldFile.Offset;
				}

				fileSet.ViewModels [fileSet.ViewModels.IndexOf (oldFile)] = newFile;
				found = true;
			} else {
				fileSet.ViewModels.Add (newFile);
			}

			return found;
		}
	}
}
