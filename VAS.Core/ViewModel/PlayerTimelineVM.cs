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
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
using VAS.Core.Store;

namespace VAS.Core.ViewModel
{
	/// <summary>
	/// A timeline ViewModel to list all the <see cref="TimelineEventVM"/>  of a given <see cref="Player"/>.
	/// </summary>
	public class PlayerTimelineVM : NestedViewModel<TimelineEventVM>, IViewModel<Player>
	{
		public PlayerTimelineVM ()
		{
		}

		public PlayerTimelineVM (PlayerVM playerVM)
		{
			Player = playerVM;
		}

		protected override void DisposeManagedResources ()
		{
			base.DisposeManagedResources ();
			Player = null;
		}

		public Player Model {
			get {
				return Player.Model;
			}
			set {
				Player.Model = value;
			}
		}

		/// <summary>
		/// Gets or sets the player of the timeline.
		/// </summary>
		/// <value>The player.</value>
		public PlayerVM Player {
			get;
			protected set;
		}
	}
}
