//
//  Copyright (C) 2017 Fluendo S.A.
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
using VAS.Core.Common;

namespace VAS.Core.Events
{

	/// <summary>
	/// Event to create a new dashboard button.
	/// </summary>
	public class CreateDashboardButtonEvent : Event
	{
		/// <summary>
		/// Gets or sets the name.
		/// </summary>
		/// <value>The name.</value>
		public string Name {
			get;
			set;
		}
	}

	/// <summary>
	/// Event to reset the field background used in a dashboard.
	/// </summary>
	public class ResetDashboardFieldEvent : Event
	{
		/// <summary>
		/// The field position to reset.
		/// </summary>
		/// <value>The field.</value>
		public FieldPositionType Field {
			get;
			set;
		}
	}

	/// <summary>
	/// Event to replace the field backgtround used in a dashboard.
	/// </summary>
	public class ReplaceDashboardFieldEvent
	{
		/// <summary>
		/// The field position to replace.
		/// </summary>
		/// <value>The field.</value>
		public FieldPositionType Field {
			get;
			set;
		}
	}
}
