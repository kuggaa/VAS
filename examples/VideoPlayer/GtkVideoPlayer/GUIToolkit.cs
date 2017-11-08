// 
//  Copyright (C) 2011 Andoni Morales Alastruey
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
using System.Threading.Tasks;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.MVVMC;
using VAS.Core.Store;
using VAS.UI;

namespace VideoPlayer
{
	public sealed class GUIToolkit : GUIToolkitBase
	{
		static readonly GUIToolkit instance = new GUIToolkit ();

		public static GUIToolkit Instance {
			get {
				return instance;
			}
		}

		GUIToolkit ()
		{
			Scanner.ScanViews (App.Current.ViewLocator);
		}

		// Old stuff that will be soon removed from the interface
		#region Crap
		public override void ExportFrameSeries (TimelineEvent play, string snapshotsDir)
		{
			throw new NotImplementedException ();
		}

		public override Project ChooseProject (List<Project> projects)
		{
			throw new NotImplementedException ();
		}

		public override void ShowProjectStats (Project project)
		{
			throw new NotImplementedException ();
		}

		public override string RemuxFile (string inputFile, string outputFile, VideoMuxerType muxer)
		{
			throw new NotImplementedException ();
		}

		public override EndCaptureResponse EndCapture (bool isCapturing)
		{
			throw new NotImplementedException ();
		}

		public override bool SelectMediaFiles (MediaFileSet fileSet)
		{
			throw new NotImplementedException ();
		}

		public override Task<bool> CreateNewTemplate<T> (IList<T> availableTemplates, string defaultName,
														 string countText, string emptyText,
														 CreateEvent<T> evt)
		{
			throw new NotImplementedException ();
		}
		#endregion
	}
}

