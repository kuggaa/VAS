//
//  Copyright (C) 2017 Fluendo S.A.
using System;
using VAS.Core.Hotkeys;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
using VideoPlayer.State;
using VideoPlayer.ViewModel;
using Xamarin.Forms;

namespace FormsGtkVideoPlayer.View
{
	[View(FileSelectionState.NAME)]
	public partial class FileSelectionView : ContentPage, IPanel<FileSelectionVM>
	{
		public FileSelectionView()
		{
			InitializeComponent();
		}

		public void Dispose()
		{
		}

		public FileSelectionVM ViewModel { get; set; }

		public void SetViewModel(object viewModel)
		{
			ViewModel = (FileSelectionVM)viewModel;
			BindingContext = ViewModel;
		}

		public void OnLoad()
		{
		}

		public void OnUnload()
		{
		}

		public KeyContext GetKeyContext()
		{
			return new KeyContext();
		}
	}
}
