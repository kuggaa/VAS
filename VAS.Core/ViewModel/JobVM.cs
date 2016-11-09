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
using VAS.Core.Common;
using VAS.Core.MVVMC;

namespace VAS.Core.ViewModel
{
	/// <summary>
	/// A ViewModel for a <see cref="Job"/>.
	/// </summary>
	public class JobVM : ViewModelBase<Job>
	{

		/// <summary>
		/// Gets the name of the <see cref="Job"/>.
		/// </summary>
		/// <value>The name.</value>
		public string Name {
			get {
				return Model?.Name;
			}
		}

		/// <summary>
		/// Gets the progress of the <see cref="Job"/>.
		/// </summary>
		/// <value>The progress.</value>
		public double Progress {
			get {
				return Model != null ? Model.Progress : 0;
			}
			set {
				if (Model != null) {
					Model.Progress = value;
				}
			}
		}

		/// <summary>
		/// Gets or sets the state of the <see cref="Job"/>.
		/// </summary>
		/// <value>The state.</value>
		public JobState State {
			get {
				return Model != null ? Model.State : JobState.None;
			}
			set {
				if (Model != null) {
					Model.State = value;
				}
			}
		}
	}
}