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
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Newtonsoft.Json;
using VAS.Core.Interfaces;

namespace VAS.Core.MVVMC
{
	/// <summary>
	/// Base class for bindable objects that implements INotifyPropertyChanged and uses Fody
	/// to automatically raise property changed events.
	/// </summary>
	[Serializable]
	public class BindableBase : DisposableBase, INotifyPropertyChanged, IChanged
	{
		Dictionary<INotifyCollectionChanged, string> collectionToPropertyName;

		// Don't serialize observers when cloning this object
		[field: NonSerialized]
		public event PropertyChangedEventHandler PropertyChanged;

		bool forwarding;

		public BindableBase ()
		{
			collectionToPropertyName = new Dictionary<INotifyCollectionChanged, string> ();
		}

		#region IChanged implementation

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public virtual bool IsChanged {
			get;
			set;
		}

		#endregion

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="VAS.Core.MVVMC.BindableBase"/> ignore events.
		/// While IgnoreEvents is true, all PropertyChanged events will not be emited
		/// </summary>
		/// <value><c>true</c> if ignore events; otherwise, <c>false</c>.</value>
		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public bool IgnoreEvents {
			get;
			set;
		}

		/// <summary>
		/// Raises the property changed event.
		/// </summary>
		/// <param name="propertyName">Property name.</param>
		/// <param name="sender">Sender of the event.</param>
		protected void RaisePropertyChanged (string propertyName, object sender = null)
		{
			if (Disposed) {
				return;
			}
			RaisePropertyChanged (new PropertyChangedEventArgs (propertyName), sender);
		}

		/// <summary>
		/// Raises the property changed event.
		/// </summary>
		/// <param name="args">Event args</param>
		/// <param name="sender">Sender of the event</param>
		protected virtual void RaisePropertyChanged (PropertyChangedEventArgs args, object sender = null)
		{
			if (Disposed) {
				return;
			}
			IsChanged = true;

			if (IgnoreEvents) {
				return;
			}
			if (PropertyChanged != null) {
				if (sender == null) {
					sender = this;
				}
				PropertyChanged (sender, args);
			}
		}

		/// <summary>
		/// Connects to a child's <see cref="INotifyPropertyChanged.PropertyChanged"/> or
		/// <see cref="INotifyCollectionChanged.CollectionChanged"/> events to keep track of changes and propagate
		/// the event upstrem.
		/// 
		/// Project -> Timeline -> TimelineEvent -> Start.
		/// 
		/// A change in Start is propagated up to the Project setting the IsChanged flag in all the objects in the chain.
		/// 
		/// This function is automatically injected with Fody at the beginning of all property setters where the
		/// type is a <see cref="BindableBase"/> or an <see cref="ObservableCollection"/>.
		///
		/// </summary>
		/// <param name="oldValue">Old value set.</param>
		/// <param name="newValue">New value to set.</param>
		protected void ConnectChild (object oldValue, object newValue, string propertyName = null)
		{
			// ObservableCollection also implements INotifyPropertyChanged so check first for INotifyCollectionChanged
			if (oldValue is INotifyCollectionChanged || newValue is INotifyCollectionChanged) {
				Connect (oldValue as INotifyCollectionChanged, newValue as INotifyCollectionChanged, propertyName);
			} else if (oldValue is INotifyPropertyChanged || newValue is INotifyPropertyChanged) {
				Connect (oldValue as INotifyPropertyChanged, newValue as INotifyPropertyChanged);
			} else if (oldValue != null && oldValue != null) {
				// This should never happen since ConnectChild should only be called for properties setting a
				// BindableBase or an ObaservableCollection<T>
				throw new NotSupportedException ();
			}
		}

		/// <summary>
		/// Connect childs of type <see cref="BindableBase"/>.
		/// </summary>
		/// <param name="oldValue">Old value.</param>
		/// <param name="newValue">New value.</param>
		void Connect (INotifyPropertyChanged oldValue, INotifyPropertyChanged newValue)
		{
			// Disconnect the old value
			if (oldValue != null) {
				oldValue.PropertyChanged -= ForwardPropertyChanged;
			}
			// Connection the new value
			if (newValue != null) {
				newValue.PropertyChanged += ForwardPropertyChanged;
			}
		}

		/// <summary>
		/// Connect childs of type <see cref="ObservableCollection"/>
		/// </summary>
		/// <param name="oldValue">Old value.</param>
		/// <param name="newValue">New value.</param>
		void Connect (INotifyCollectionChanged oldValue, INotifyCollectionChanged newValue, string propertyName)
		{
			if (newValue != null) {
			}
			// Disconnect the old collection and all its children
			if (oldValue != null) {
				oldValue.CollectionChanged -= CollectionChanged;
				foreach (var element in (oldValue as IEnumerable).OfType<INotifyPropertyChanged> ()) {
					element.PropertyChanged -= ForwardPropertyChanged;
				}
				collectionToPropertyName.Remove (oldValue);
			}
			// Connect the new collection and all its children
			if (newValue != null) {
				newValue.CollectionChanged += CollectionChanged;
				foreach (var element in (newValue as IEnumerable).OfType<INotifyPropertyChanged> ()) {
					element.PropertyChanged += ForwardPropertyChanged;
				}
				collectionToPropertyName [newValue] = propertyName;
			}
		}

		protected virtual void CollectionChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.OldItems != null) {
				// Keep track to new items added to the collection and start observing them
				foreach (var element in e.OldItems.OfType<INotifyPropertyChanged> ()) {
					element.PropertyChanged -= ForwardPropertyChanged;
				}
			}
			if (e.NewItems != null) {
				// Keep track to items removed from the collection and stop observing them
				foreach (var element in e.NewItems.OfType<INotifyPropertyChanged> ()) {
					element.PropertyChanged += ForwardPropertyChanged;
				}
			}
			RaisePropertyChanged ($"Collection_{collectionToPropertyName [sender as INotifyCollectionChanged]}", this);
		}

		protected virtual void ForwardPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			// Break potential infinite loop for objects with circular dependencies.
			if (forwarding) {
				return;
			}
			forwarding = true;
			RaisePropertyChanged (e, sender);
			forwarding = false;
		}
	}
}

