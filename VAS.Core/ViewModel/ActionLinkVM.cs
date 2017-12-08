//
//  Copyright (C) 2017 FLUENDO S.A.
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
using System.Collections.ObjectModel;
using VAS.Core.Common;
using VAS.Core.MVVMC;
using VAS.Core.Store;

namespace VAS.Core.ViewModel
{
	/// <summary>
	/// ViewModel of the Action link.
	/// </summary>
	public class ActionLinkVM : ViewModelBase<ActionLink>
	{
		DashboardButtonVM sourceButtonVM;
		DashboardButtonVM destinationButtonVM;

		public ActionLinkVM ()
		{
			SourceTags = new KeyUpdaterCollectionViewModel<Tag, TagVM> ();
			DestinationTags = new KeyUpdaterCollectionViewModel<Tag, TagVM> ();
		}

		public override ActionLink Model {
			get {
				return base.Model;
			}
			set {
				base.Model = value;
				SourceTags.Model = value?.SourceTags;
				DestinationTags.Model = value?.DestinationTags;
			}
		}

		/// <summary>
		/// The type of action that will be performed in the destination.
		/// </summary>
		public LinkAction Action {
			get { return Model.Action; }
			set { Model.Action = value; }
		}

		/// <summary>
		/// The type of action that will be performed in the destination
		/// for team tagged in the source event.
		/// </summary>
		public TeamLinkAction TeamAction {
			get { return Model.TeamAction; }
			set { Model.TeamAction = value; }
		}

		/// <summary>
		/// The source button of the link
		/// </summary>
		public DashboardButtonVM SourceButton {
			get { return sourceButtonVM; }
			set {
				sourceButtonVM = value;
				Model.SourceButton = sourceButtonVM.Model;
			}
		}

		/// <summary>
		/// A list of tags that needs to match in the source
		/// </summary>
		public CollectionViewModel<Tag,TagVM> SourceTags {
			get;
		}

		/// <summary>
		/// The destination button of the link
		/// </summary>
		public DashboardButtonVM DestinationButton {
			get { return destinationButtonVM; }
			set {
				destinationButtonVM = value;
				Model.DestinationButton = destinationButtonVM.Model;
			}
		}

		/// <summary>
		/// A list of tags that needs to be set in the destination
		/// </summary>
		public KeyUpdaterCollectionViewModel<Tag, TagVM> DestinationTags {
			get;
		}

		/// <summary>
		/// If <c>true</c>, players tagged in the source event will be copied
		/// </summary>
		public bool KeepPlayerTags {
			get { return Model.KeepPlayerTags; }
			set { Model.KeepPlayerTags = value; }
		}

		/// <summary>
		/// If <c>true</c>, generic tags will be copied.
		/// </summary>
		public bool KeepGenericTags {
			get { return Model.KeepGenericTags; }
			set { Model.KeepGenericTags = value; }
		}

		/// <summary>
		/// Gets the name of the action link
		/// </summary>
		/// <value>The action link name.</value>
		public string Name {
			get { return string.Format ("{0} -> {1}", SourceButton?.Name, DestinationButton?.Name); }
		}
	}
}
