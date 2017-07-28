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
using VAS.Core.Common;
using System.Reflection;

namespace VAS.Core.MVVMC
{
	public class ViewModelBase : BindableBase, IViewModel
	{
		/// <summary>
		/// Checks if sync is required from the name of the property that triggered a <see cref="PropertyChangedEventHandler"/>.
		/// </summary>
		/// <returns><c>true</c>, if sync was needed, <c>false</c> otherwise.</returns>
		/// <param name="propertyNameChanged">Property name changed.</param>
		/// <param name="propertyNameToCheck">Property name to check.</param>
		public bool NeedsSync (string propertyNameChanged, string propertyNameToCheck)
		{
			return NeedsSync (propertyNameChanged, propertyNameToCheck, null, null);
		}

		/// <summary>
		/// Checks if sync is required from the event args that triggered a <see cref="PropertyChangedEventHandler"/>.
		/// </summary>
		/// <returns><c>true</c>, if sync was needsed, <c>false</c> otherwise.</returns>
		/// <param name="eventArgs">Event arguments.</param>
		/// <param name="propertyNameToCheck">Property name to check.</param>
		public bool NeedsSync (PropertyChangedEventArgs eventArgs, string propertyNameToCheck)
		{
			return NeedsSync (eventArgs.PropertyName, propertyNameToCheck, null, null);
		}

		/// <summary>
		/// Checks if sync is required from the name of the property that triggered a <see cref="PropertyChangedEventHandler"/>
		/// and the sender of the event.
		/// </summary>
		/// <returns><c>true</c>, if sync was needed, <c>false</c> otherwise.</returns>
		/// <param name="sender">Sender.</param>
		/// <param name="senderToCheck">Sender to check.</param>
		public bool NeedsSync (string propertyNameChanged, string propertyNameToCheck, object sender, object senderToCheck)
		{
			if (!SendersAreEqual (sender, senderToCheck)) {
				return false;
			}
			if (propertyNameChanged == null || propertyNameChanged == propertyNameToCheck) {
				return true;
			}
			return false;
		}

		/// <summary>
		/// Force a sync of all properties as when the ViewModel is set for the first time in a View.
		/// </summary>
		public void Sync ()
		{
			RaisePropertyChanged (propertyName: null, sender: this);
		}

		protected override void ForwardPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			Log.Verbose ($"ViewModelBase {this} - Reception Forward PropertyName: {e.PropertyName} with sender: {sender}");
			if (sender is IViewModel) {
				Log.Verbose ($"ViewModelBase {this} - Forwarding PropertyName: {e.PropertyName} with sender: {sender}");
				base.ForwardPropertyChanged (sender, e);
			}
		}

		bool SendersAreEqual (object sender, object senderToCheck)
		{
			if (sender == null && senderToCheck == null) {
				return true;
			} else {
				return sender == senderToCheck;
			}
		}
	}

	public class ViewModelBase<T> : ViewModelBase, IViewModel<T> where T : class, INotifyPropertyChanged
	{
		T model;

		protected override void DisposeManagedResources ()
		{
			IgnoreEvents = true;
			base.DisposeManagedResources ();
			Model = null;
		}

		/// <summary>
		/// Gets or sets the model used by this ViewModel.
		/// We disable Foody's equality check since we work sometimes with
		/// copies of the template and replacing the model with the copies template after saving
		/// it does not update the model because of the ID-based equality check
		/// </summary>
		/// <value>The model.</value>
		[PropertyChanged.DoNotCheckEquality]
		public virtual T Model {
			get {
				return model;
			}
			set {
				model = value;
				if (!IgnoreEvents) {
					Sync ();
				}
			}
		}

		protected override void ForwardPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			Log.Verbose ($"ViewModelBase<T>{this} - Reception Forward PropertyName: {e.PropertyName} with sender: {sender}");
			if (!(sender is IViewModel) && Model != null) {
				if (sender == Model) {
					Log.Verbose ($"ViewModelBase<T> {this} - Changing Forward sender to {this}");
					sender = this;
				} else {
					//FIXME: This changes sender if sender is a Property of the Model,
					// But this logic should be done in the Model. Maybe a ModelBase should
					// be created in order to change the sender in its forwarder
					PropertyInfo [] props = Model.GetType ().GetProperties ();
					foreach (var info in props) {
						if (info.PropertyType.IsAssignableFrom (sender.GetType ())) {
							Log.Verbose ($"ViewModelBase<T> {this} - Changing Forward sender to {this}");
							sender = this;
							break;
						}
					}
				}
			}
			Log.Verbose ($"ViewModelBase<T> {this} - Forwarding PropertyName: {e.PropertyName} with sender: {sender}");
			base.ForwardPropertyChanged (sender, e);
		}
	}
}

