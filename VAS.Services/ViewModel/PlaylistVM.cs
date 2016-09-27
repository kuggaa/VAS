//
//   Copyright (C) 2016 Fluendo S.A.
//
using System.Collections.Generic;
using System.Linq;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
using VAS.Core.Store.Playlists;
using System;
using System.Collections.ObjectModel;
using VAS.Core.Interfaces;

namespace VAS.Services.ViewModel
{
	/// <summary>
	/// Playlist VM.
	/// </summary>
	public class PlaylistVM : ViewModelBase<Playlist> // FIXME: Implement INested... from RA-138
	{
		public CollectionViewModel<IPlaylistElement, PlaylistElementVM> ViewModels {
			get;
			set;
		} = new CollectionViewModel<IPlaylistElement, PlaylistElementVM>();

		public override Playlist Model {
			get {
				return base.Model;
			}
			set {
				base.Model = value;
				if (value != null) {
					ViewModels.Model = Model.Elements;
				}
			}
		}

		public string Name {
			get {
				return Model.Name;
			}
			set {
				Model.Name = value;
			}
		}

		public DateTime CreationDate {
			get {
				return Model.CreationDate;
			}
		}

		public DateTime LastModified {
			get {
				return Model.LastModified;
			}
			set {
				Model.LastModified = value;
			}
		}
	}
}

