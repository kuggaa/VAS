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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using VAS.Core.Common;
using VAS.Core.ViewModel;

namespace VAS.Core.Interfaces.MVVMC
{
	public interface IViewModel : INotifyPropertyChanged, IDisposable
	{
	}

	public interface IViewModel<T> : IViewModel
	{
		T Model { set; get; }
	}

	/// <summary>
	/// Interface to get the INotifyCollectionChanged Interface
	/// </summary>
	public interface INestedViewModel
	{
		INotifyCollectionChanged GetNotifyCollection ();

		INotifyCollectionChanged GetSelectionNotifyCollection ();
	}

	/// <summary>
	/// Interface to get the Child View Models
	/// </summary>
	public interface INestedViewModel<VMChilds> : INestedViewModel, IViewModel, IEnumerable<VMChilds>
	{
		RangeObservableCollection<VMChilds> ViewModels { get; }

		RangeObservableCollection<VMChilds> Selection { get; }

		void SelectionReplace (IEnumerable<VMChilds> selection);
	}

	public interface IPreferencesVM : IViewModel
	{
		string View { get; }
		string Name { get; }
		bool AutoSave { get; set; }
		void Cancel ();
		void Save ();
	}
}

