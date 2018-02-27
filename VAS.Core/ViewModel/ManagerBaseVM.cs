//
//  Copyright (C) 2018 
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
using VAS.Core.MVVMC;
using System.ComponentModel;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.Interfaces;
using System.Windows.Input;
using VAS.Core.Events;
using VAS.Core.Filters;

namespace VAS.Core.ViewModel
{
	public abstract class ManagerBaseVM<TModel, TViewModel> : LimitedCollectionViewModel<TModel, TViewModel>
		where TViewModel : IViewModel<TModel>, new()
		where TModel : IStorable
	{
		public ManagerBaseVM ()
		{
			SearchCommand = new Command<string> (textFilter => App.Current.EventsBroker.Publish (
				new SearchEvent { TextFilter = textFilter }));
			ShowMenu = new Command<Tuple<IView, IViewModel>> ((t) => {
				MenuVM vm = CreateMenu (t.Item2);
				t.Item1.SetViewModel (vm);
			});

			FilterText = string.Empty;
		}

		public TViewModel LoadedItem { get; set; }

		/// <summary>
		/// Gets the project menu.
		/// </summary>
		/// <value>The project menu.</value>
		[PropertyChanged.DoNotNotify]
		public ICommand ShowMenu { get; private set; }

		/// <summary>
		/// Publishes SearchEvent<TModel> with text filter as parameter.
		/// </summary>
		/// <value>The search projects command.</value>
		[PropertyChanged.DoNotNotify]
		public Command<string> SearchCommand { get; protected set; }

		/// <summary>
		/// Command to delete a template.
		/// </summary>
		/// <value>The delete command.</value>
		[PropertyChanged.DoNotNotify]
		public Command<TViewModel> DeleteCommand { get; protected set; }

		/// <summary>
		/// Gets or sets the filter text.
		/// </summary>
		/// <value>The filter text.</value>
		public string FilterText { get; set; }

		/// <summary>
		/// Gets or sets the empty card View Model to be used when the user has no projects created.
		/// </summary>
		/// <value>The empty card.</value>
		public EmptyCardVM EmptyCard { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the current search filter returned no results.
		/// </summary>
		/// <value><c>true</c> if no results; otherwise, <c>false</c>.</value>
		public bool NoResults { get; set; }

		protected virtual MenuVM CreateMenu (IViewModel viewModel)
		{
			return null;
		}
	}
}
