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
using System.ComponentModel;
using Newtonsoft.Json;
using VAS.Core.Interfaces;

namespace VAS.Core.MVVMC
{
	/// <summary>
	/// Base class for bindable objects that implements INotifyPropertyChanged and uses Fody
	/// to automatically raise property changed events.
	/// </summary>
	[Serializable]
	[PropertyChanged.ImplementPropertyChanged]
	public class BindableBase: INotifyPropertyChanged, IChanged
	{
		// Don't serialize observers when cloning this object
		[field:NonSerialized]
		public event PropertyChangedEventHandler PropertyChanged;

		#region IChanged implementation

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public virtual bool IsChanged {
			get;
			set;
		}

		#endregion

		/// <summary>
		/// Raises the property changed event.
		/// </summary>
		/// <param name="sender">Sender of the event</param>
		/// <param name="e">Event args</param>
		protected void RaisePropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (PropertyChanged != null) {
				PropertyChanged (sender, e);
			}
			IsChanged = true;
		}
	}
}

