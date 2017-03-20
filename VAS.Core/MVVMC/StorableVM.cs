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
using System.ComponentModel;
using Newtonsoft.Json;
using VAS.Core.Interfaces;

namespace VAS.Core.MVVMC
{
	/// <summary>
	/// Base class for <see cref="StorableBase"/>'s ViewModel that synchronizes values with the Model taking in
	/// account if the Model is preloaded or not to avoid fully loading the storable when properties are not needed.
	/// </summary>
	public class StorableVM<TModel> : ViewModelBase<TModel>
		where TModel : IStorable, INotifyPropertyChanged
	{
		[JsonIgnore]
		[PropertyChanged.DoNotCheckEquality]
		public override TModel Model {
			get {
				return base.Model;
			}
			set {
				base.Model = value;
				SyncPreloadedModel ();
				if (Model == null || Model.IsLoaded) {
					SyncLoadedModel ();
				}
			}
		}

		/// <summary>
		/// Synchronizes the loaded Model properties with the ViewModel.
		/// </summary>
		protected virtual void SyncLoadedModel ()
		{
		}

		/// <summary>
		/// Synchronizes the preloaded Model properties with the ViewModel.
		/// </summary>
		protected virtual void SyncPreloadedModel ()
		{
		}

		protected override void ForwardPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (NeedsSync (e.PropertyName, nameof (IStorable.IsLoaded), sender, Model)) {
				SyncLoadedModel ();
			}
			base.ForwardPropertyChanged (sender, e);
		}
	}
}
