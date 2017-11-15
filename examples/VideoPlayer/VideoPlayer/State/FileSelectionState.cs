//
//  Copyright (C) 2017 Fluendo S.A.
using System;
using System.Dynamic;
using VAS.Services.State;
using VideoPlayer.ViewModel;

namespace VideoPlayer.State
{
	public class FileSelectionState : ScreenState<FileSelectionVM>
	{
		public const string NAME = "File Selection";

		public override string Name => NAME;

		protected override void CreateViewModel (dynamic data)
		{
			ViewModel = new FileSelectionVM ();
		}
	}
}
