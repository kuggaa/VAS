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
using VAS.Core.Events;
using VAS.Core.MVVMC;
using VAS.Core.Store;

namespace VAS.Core.ViewModel
{
	/// <summary>
	/// Class for the TagButton ViewModel
	/// </summary>
	public class TagButtonVM : DashboardButtonVM
	{
		Time currentTime;

		public TagButtonVM ()
		{
			Tag = new TagVM ();
			Toggle = new Command (() => App.Current.EventsBroker.Publish (new TagClickedEvent { ButtonVM = this }));
		}

		/// <summary>
		/// Gets the correctly Typed Model
		/// </summary>
		/// <value>The button.</value>
		public TagButton TypedModel {
			get {
				return (TagButton)base.Model;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="T:VAS.Core.ViewModel.TagButtonVM"/> is active.
		/// </summary>
		/// <value><c>true</c> if active; otherwise, <c>false</c>.</value>
		public bool Active { get; set; }

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
				Tag.Model = ((TagButton)value)?.Tag;
			}
		}

		/// <summary>
		/// Gets the view.
		/// </summary>
		/// <value>The view.</value>
		public override string View {
			get {
				return "TagButtonView";
			}
		}

		/// <summary>
		/// Gets or sets the tag.
		/// </summary>
		/// <value>The tag.</value>
		public TagVM Tag {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the toogle command which checks that one tag belonging to a group is selected at a time.
		/// </summary>
		/// <value>The toogle command.</value>
		public Command Toggle { get; set; }
	}
}
