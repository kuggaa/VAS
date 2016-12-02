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
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;

namespace VAS.Core.ViewModel
{
	/// <summary>
	/// Generic base class for <see cref="ITemplate"/> ViewModel.
	/// </summary>
	public abstract class TemplateViewModel<T> : ViewModelBase<T>, IViewModel<T> where T : ITemplate<T>
	{
		/// <summary>
		/// Gets the name of the template.
		/// </summary>
		/// <value>The name.</value>
		public string Name {
			get {
				return Model?.Name;
			}
			set {
				Model.Name = value;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the template is editable.
		/// </summary>
		/// <value><c>true</c> if editable; otherwise, <c>false</c>.</value>
		public bool Editable {
			get {
				return Model?.Static == false;
			}
		}

		/// <summary>
		/// Gets or sets the icon used for the template.
		/// </summary>
		/// <value>The icon.</value>
		public abstract Image Icon {
			get;
			set;
		}

		/// <summary>
		/// Gets a value indicating whether the template has been edited.
		/// </summary>
		/// <value><c>true</c> if edited; otherwise, <c>false</c>.</value>
		public bool Edited {
			get {
				return Model?.IsChanged == true;
			}
		}
	}
}
