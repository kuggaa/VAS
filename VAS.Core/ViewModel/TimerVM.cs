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
using System.Collections.ObjectModel;
using VAS.Core.Common;
using VAS.Core.MVVMC;
using VAS.Core.Store;

namespace VAS.Core.ViewModel
{
	/// <summary>
	/// ViewModel for <see cref="Timer"/> objects.
	/// </summary>
	public class TimerVM : NestedSubViewModel<Timer, TimerVM, TimeNode, TimeNodeVM>
	{

		/// <summary>
		/// Gets or sets the name of the timer.
		/// </summary>
		/// <value>The name.</value>
		public virtual string Name {
			get {
				return Model.Name;
			}
			set {
				Model.Name = value;
			}
		}

		public override RangeObservableCollection<TimeNode> ChildModels {
			get {
				return Model.Nodes;
			}
		}
	}
}
