//
//  Copyright (C) 2017 Fluendo S.A.
using System;
using VAS.Core.Hotkeys;
using VAS.Core.Interfaces.GUI;
using VAS.Core.MVVMC;
using VAS.UI.Helpers.Bindings;
using VideoPlayer.State;
using VideoPlayer.ViewModel;

namespace VideoPlayer
{
	[System.ComponentModel.ToolboxItem (true)]
	[View (FileSelectionState.NAME)]
	public partial class FileSelectionView : Gtk.Bin, IPanel<FileSelectionVM>
	{
		FileSelectionVM viewModel;

		public FileSelectionView ()
		{
			this.Build ();
			Bind ();

		}

		public string Title => "Select a new file...";

		public FileSelectionVM ViewModel {
			get {
				return viewModel;
			}
			set {
				viewModel = value;
				this.GetBindingContext ().UpdateViewModel (viewModel);
			}
		}

		public KeyContext GetKeyContext ()
		{
			return new KeyContext ();
		}

		public void OnLoad ()
		{
		}

		public void OnUnload ()
		{
		}

		public void SetViewModel (object viewModel)
		{
			ViewModel = (FileSelectionVM)viewModel;
		}

		void Bind ()
		{
			var ctx = this.GetBindingContext ();
			ctx.Add (savebutton.Bind (vm => ((FileSelectionVM)vm).OpenFile));
			ctx.Add (messageLabel.Bind (vm => ((FileSelectionVM)vm).Message));
		}
	}
}
