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
using VAS.Core.Events;
using VAS.Core.MVVMC;
using VAS.Core.Resources;
using VAS.Services.State;
using VAS.Services.ViewModel;

namespace VAS.Services.Controller
{
	[Controller (DrawingToolState.NAME)]
	public class DrawingToolController : ControllerBase<DrawingToolVM>
	{
		protected override void ConnectEvents ()
		{
			base.ConnectEvents ();
			App.Current.EventsBroker.SubscribeAsync<SaveEvent<Image>> (HandleSaveDrawing);
			App.Current.EventsBroker.SubscribeAsync<ExportEvent<Image>> (HandleExportDrawing);
		}

		protected override void DisconnectEvents ()
		{
			base.DisconnectEvents ();
			App.Current.EventsBroker.UnsubscribeAsync<SaveEvent<Image>> (HandleSaveDrawing);
			App.Current.EventsBroker.UnsubscribeAsync<ExportEvent<Image>> (HandleExportDrawing);
		}

		protected virtual async Task HandleSaveDrawing (SaveEvent<Image> e)
		{
			ViewModel.Drawing.RegionOfInterest = ViewModel.Blackboard.RegionOfInterest;
			if (!ViewModel.TimelineEvent.Drawings.Contains (ViewModel.Drawing)) {
				ViewModel.TimelineEvent.Drawings.Add (ViewModel.Drawing);
			}
			ViewModel.Drawing.Miniature = ViewModel.Blackboard.Save ();
			ViewModel.Drawing.Miniature.ScaleInplace (Constants.MAX_THUMBNAIL_SIZE,
				Constants.MAX_THUMBNAIL_SIZE);
			ViewModel.TimelineEvent.Model.UpdateMiniature ();
			await App.Current.EventsBroker.Publish (new DrawingSavedToProjectEvent ());
			await App.Current.StateController.MoveBack ();
		}

		protected virtual async Task HandleExportDrawing (ExportEvent<Image> e)
		{
			string proposed_filename = String.Format ("{0}-{1}.png", App.Current.SoftwareName,
										   DateTime.Now.ToShortDateString ().Replace ('/', '-'));
			string filename = App.Current.Dialogs.SaveFile (Strings.SaveAs,
								  proposed_filename, App.Current.SnapshotsDir,
								  "PNG Images", new string [] { "*.png" });
			if (filename != null) {
				System.IO.Path.ChangeExtension (filename, ".png");
				ViewModel.Blackboard.Save (filename);
				await App.Current.StateController.MoveBack ();
			}
		}
	}
}
