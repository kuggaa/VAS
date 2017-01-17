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
using System;
using System.Collections.ObjectModel;
using VAS.Core.Common;
using VAS.Core.MVVMC;
using VAS.Core.Store;

namespace VAS.Core.ViewModel
{
	/// <summary>
	/// ViewModel for <see cref="DashboardButton"/>.
	/// </summary>
	public class DashboardButtonVM : ViewModelBase<DashboardButton>
	{
		public virtual string View {
			get;
		}

		public virtual string Name {
			get {
				return Model.Name;
			}
			set {
				Model.Name = value;
			}
		}

		public Point Position {
			get {
				return Model.Position;
			}
			set {
				Model.Position = value;
			}
		}

		public int Width {
			get {
				return Model.Width;
			}
			set {
				Model.Width = value;
			}
		}

		public int Height {
			get {
				return Model.Height;
			}
			set {
				Model.Height = value;
			}
		}

		public Color BackgroundColor {
			get {
				return Model.BackgroundColor;
			}
			set {
				Model.BackgroundColor = value;
			}
		}

		public Color TextColor {
			get {
				return Model.TextColor;
			}
			set {
				Model.TextColor = value;
			}
		}

		public virtual HotKey HotKey {
			get {
				return Model.HotKey;
			}
			set {
				Model.HotKey = value;
			}
		}

		public virtual Image BackgroundImage {
			get {
				return Model.BackgroundImage;
			}
			set {
				Model.BackgroundImage = value;
			}
		}

		public bool ShowHotkey {
			get {
				return Model.ShowHotkey;
			}
			set {
				Model.ShowHotkey = value;
			}
		}

		public bool ShowSettingIcon {
			get {
				return Model.ShowSettingIcon;
			}
			set {
				Model.ShowSettingIcon = value;
			}
		}

		/// <summary>
		/// A list with all the outgoing links of this button
		/// </summary>
		public ObservableCollection<ActionLink> ActionLinks {
			get {
				return Model.ActionLinks;
			}
			set {
				Model.ActionLinks = value;
			}
		}

		public Color LightColor {
			get {
				return Model.LightColor;
			}
		}

		public Color DarkColor {
			get {
				return Model.DarkColor;
			}
		}

		public bool ShowIcon {
			get {
				return Model.ShowIcon;
			}
			set {
				Model.ShowIcon = value;
			}
		}
	}

	public class TimedDashboardButtonVM : DashboardButtonVM
	{
		public new TimedDashboardButton Model {
			get {
				return (TimedDashboardButton)base.Model;
			}
			set {
				base.Model = value;
			}
		}

		public TagMode TagMode {
			get {
				return Model.TagMode;
			}
			set {
				Model.TagMode = value;
			}
		}

		public Time Start {
			get {
				return Model.Start;
			}
			set {
				Model.Start = value;
			}
		}

		public Time Stop {
			get {
				return Model.Stop;
			}
			set {
				Model.Stop = value;
			}
		}
	}

	public class EventButtonVM : TimedDashboardButtonVM
	{
		public new EventButton Model {
			get {
				return (EventButton)base.Model;
			}
			set {
				base.Model = value;
				EventType = new EventTypeVM { Model = Model.EventType };
			}
		}

		public EventTypeVM EventType {
			get;
			private set;
		}
	}

	public class AnalysisEventButtonVM : EventButtonVM
	{
		public new AnalysisEventButton Model {
			get {
				return (AnalysisEventButton)base.Model;
			}
			set {
				base.Model = value;
			}
		}

		public override string View {
			get {
				return "AnalysisEventButtonView";
			}
		}

		public bool ShowSubcategories {
			get {
				return Model.ShowSubcategories;
			}
			set {
				Model.ShowSubcategories = value;
			}
		}

		public int TagsPerRow {
			get {
				return Model.TagsPerRow;
			}
			set {
				Model.TagsPerRow = value;
			}
		}
	}

	public class TagButtonVM : DashboardButtonVM
	{
		public new TagButton Model {
			get {
				return (TagButton)base.Model;
			}
			set {
				base.Model = value;
			}
		}

		public override string View {
			get {
				return "TagButtonView";
			}
		}

		public Tag Tag {
			get {
				return Model.Tag;
			}
			set {
				Model.Tag = value;
			}
		}
	}

	public class TimerButtonVM : DashboardButtonVM
	{
		public new TimerButton Model {
			get {
				return (TimerButton)base.Model;
			}
			set {
				base.Model = value;
			}
		}

		public override string View {
			get {
				return "TimerButtonView";
			}
		}

		public Timer Timer {
			get {
				return Model.Timer;
			}
			set {
				Model.Timer = value;
			}
		}
	}
}
