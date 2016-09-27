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

namespace VAS.Core.Interfaces.MVVMC
{
	public interface IViewModel: INotifyPropertyChanged
	{
	}

	public interface IViewModel<T> : IViewModel
	{
		T Model { set; get; }
	}

	public interface INestedViewModel
	{
		INotifyCollectionChanged GetNotifyCollection ();
	}

	public interface INestedViewModel<T,VMChilds> : INestedViewModel, IViewModel <T>, IEnumerable <VMChilds>
	{
		ObservableCollection<VMChilds> ViewModels { set; get; }
	}

	public interface IPlayerViewModel : IViewModel
	{
		IPlayerController Player { get; }

		PlayerViewOperationMode Mode  { get; set; }

		bool SupportsMultipleCameras { get; }

		bool PlayerAttached { set; }

		bool ShowDetachButton { set; }
	}

	public interface IAnalysisViewModel
	{
		IPlayerViewModel PlayerViewModel { get; }
	}
}

