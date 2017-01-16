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

		public bool ShowIcon {
			get {
				return Model.ShowIcon;
			}
			set {
				Model.ShowIcon = value;
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
	}

	public class AnalysisEventButtonVM : DashboardButtonVM
	{
		AnalysisEventButton model;

		public new AnalysisEventButton Model {
			get {
				return (AnalysisEventButton)base.Model;
			}
			set {
				base.Model = value;
				model = value;
			}
		}

		public override string View {
			get {
				return "AnalysisEventButtonView";
			}
		}

		public TagMode TagMode {
			get {
				return model.TagMode;
			}
			set {
				model.TagMode = value;
			}
		}
	}

	public class TagButtonVM : DashboardButtonVM
	{
		TagButton model;

		public new TagButton Model {
			get {
				return (TagButton)base.Model;
			}
			set {
				base.Model = value;
				model = value;
			}
		}

		public override string View {
			get {
				return "TagButtonView";
			}
		}
	}

	public class TimerButtonVM : DashboardButtonVM
	{
		TimerButton model;

		public new TimerButton Model {
			get {
				return (TimerButton)base.Model;
			}
			set {
				base.Model = value;
				model = value;
			}
		}

		public override string View {
			get {
				return "TimerButtonView";
			}
		}
	}
}
