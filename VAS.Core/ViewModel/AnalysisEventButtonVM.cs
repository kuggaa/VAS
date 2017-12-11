//
//  Copyright (C) 2016 Fluendo S.A.
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
using System.Collections.Generic;
using System.Linq;
using VAS.Core.Common;
using VAS.Core.MVVMC;
using VAS.Core.Store;

namespace VAS.Core.ViewModel
{
	/// <summary>
	/// Class for the AnalysisEventButton ViewModel.
	/// </summary>
	public class AnalysisEventButtonVM : EventButtonVM
	{
		/// <summary>
		/// Gets the correctly Typed Model
		/// </summary>
		/// <value>The button.</value>
		public new AnalysisEventButton TypedModel {
			get {
				return (AnalysisEventButton)base.Model;
			}
		}

		/// <summary>
		/// Gets or sets the model.
		/// </summary>
		/// <value>The model.</value>
		[PropertyChanged.DoNotCheckEquality]
		public override DashboardButton Model {
			get {
				return TypedModel;
			}
			set {
				base.Model = value;
			}
		}

		/// <summary>
		/// Gets the subcategories of the event
		/// </summary>
		/// <value>The tags.</value>
		public CollectionViewModel<Tag, TagVM> Tags {
			get => EventType.Tags;
		}

		/// <summary>
		/// Gets the view.
		/// </summary>
		/// <value>The view.</value>
		public override string View {
			get {
				return "AnalysisEventButtonView";
			}
		}

		/// <summary>
		/// Gets or sets a value indicating to showing or not the subcategories.
		/// </summary>
		/// <value><c>true</c> if show subcategories; otherwise, <c>false</c>.</value>
		public bool ShowSubcategories {
			get {
				return TypedModel.ShowSubcategories;
			}
			set {
				TypedModel.ShowSubcategories = value;
			}
		}

		/// <summary>
		/// Gets or sets the tags per row.
		/// </summary>
		/// <value>The tags per row.</value>
		public int TagsPerRow {
			get {
				return TypedModel.TagsPerRow;
			}
			set {
				TypedModel.TagsPerRow = value;
			}
		}

		public RangeObservableCollection<TagVM> SelectedTags {
			get {
				return Tags.Selection;
			}
		}

		public Dictionary<string, List<TagVM>> TagsByGroup {
			get {
				return Tags.ViewModels.GroupBy (t => t.Group).ToDictionary (g => g.Key, g => g.ToList ());
			}
		}

		public bool IsCategoryClicked {
			get;
			set;
		}

		public override void Click ()
		{
			base.Click ();
			this.SelectedTags.Clear ();
		}

		public void ToggleIsCategoryClicked ()
		{
			IsCategoryClicked = !IsCategoryClicked;
		}

		protected override IEnumerable<Tag> GetTags ()
		{
			return SelectedTags.Select (t => t.Model);
		}
	}
}
