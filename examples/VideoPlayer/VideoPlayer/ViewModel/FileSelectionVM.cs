//
//  Copyright (C) 2017 Fluendo S.A.
using System;
using VAS.Core;
using VAS.Core.MVVMC;
using VAS.Core.ViewModel;

namespace VideoPlayer.ViewModel
{
	public class FileSelectionVM : ViewModelBase
	{
		public FileSelectionVM ()
		{
			File = new MediaFileVM ();
			OpenFile = new Command (() => App.Current.EventsBroker.Publish (new OpenFileEvent ())) {
				Text = Catalog.GetString ("Open"),
				ToolTipText = Catalog.GetString ("Open a new file"),
				Icon = App.Current.ResourcesLocator.LoadIcon ("vas-open")

			};
			Message = Catalog.GetString ("Drag&Drop a file or open a new file");
		}

		/// <summary>
		/// Gets or sets the command to open a new file.
		/// </summary>
		public Command OpenFile { get; set; }

		public string Message { get; set; }

		public MediaFileVM File { get; private set; }
	}
}
