//
//  Copyright (C) 2016 Fluendo S.A.
using System;
using VAS.Core.Interfaces;
using VAS.Core.MVVMC;
using VAS.Core.Common;
using VAS.Core.Store;

namespace VAS.Services.ViewModel
{
	public class PlaylistElementVM : ViewModelBase<IPlaylistElement>
	{
		public string Description {
			get {
				return Model.Description;
			}
		}

		public Image Miniature {
			get {
				return Model.Miniature;
			}
		}

		bool Selected {
			get;
			set;
		}

		Time Duration {
			get {
				return Model.Duration;
			}
		}
	}
}
