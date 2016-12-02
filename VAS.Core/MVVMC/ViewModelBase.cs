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
using VAS.Core.Interfaces.MVVMC;
using System.ComponentModel;

namespace VAS.Core.MVVMC
{
	public class ViewModelBase<T> : BindableBase, IViewModel<T> where T : INotifyPropertyChanged
	{
		/// <summary>
		/// Gets or sets the model used by this ViewModel.
		/// We disable Foody's equality check since we work sometimes with
		/// copies of the template and replacing the model with the copies template after saving
		/// it does not update the model because of the ID-based equality check 
		/// </summary>
		/// <value>The model.</value>
		[PropertyChanged.DoNotCheckEquality]
		public virtual T Model {
			set;
			get;
		}
	}
}

