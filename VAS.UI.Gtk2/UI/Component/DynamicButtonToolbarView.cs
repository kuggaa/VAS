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
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Gtk;
using VAS.Core.Common;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
using VAS.Core.ViewModel;
using VAS.UI.Helpers;

namespace VAS.UI.Component
{
	/// <summary>
	/// Dynamic button toolbar view, creates buttons dynamically in order to bind them to the collection of commands
	/// exposed in the viewmodel
	/// </summary>
    [System.ComponentModel.ToolboxItem(true)]
	public partial class DynamicButtonToolbarView : Gtk.Bin, IView<DynamicButtonToolbarVM>
	{
		DynamicButtonToolbarVM viewModel;

		public DynamicButtonToolbarView()
		{
			this.Build();
		}

		public override void Dispose()
		{
			Dispose(true);
			base.Dispose();
		}

		protected virtual void Dispose(bool disposing)
		{
			if (Disposed)
			{
				return;
			}
			if (disposing)
			{
				Destroy();
			}
			Disposed = true;
		}

		protected override void OnDestroyed()
		{
			Log.Verbose($"Destroying {GetType()}");
			ViewModel = null;
			base.OnDestroyed();

			Disposed = true;
		}

		protected bool Disposed { get; private set; } = false;

		public DynamicButtonToolbarVM ViewModel
		{
			get
			{
				return viewModel;
			}

			set
			{
				if (viewModel != null)
				{
					viewModel.ToolbarCommands.CollectionChanged -= HandleToolbarCommandsCollectionChanged;
				}
				viewModel = value;
				if (viewModel != null)
				{
					viewModel.ToolbarCommands.CollectionChanged += HandleToolbarCommandsCollectionChanged;
				}
			}
		}

		public void SetViewModel(object viewModel)
		{
			ViewModel = (DynamicButtonToolbarVM)viewModel;
		}

		void Clear()
		{
			foreach (var button in hbox.Children.ToList())
			{
				button.Destroy();
				hbox.Remove(button);
			}
		}

		void AddButtons(IEnumerable<Command> commands)
		{
			foreach (Command command in commands)
			{
				AddButton(command);
			}
			ShowAll ();
		}

		void AddButton(Command command)
		{
			Button button = ButtonHelper.CreateButton();
			button.BindManually(command);
			hbox.PackStart(button);
		}

		void HandleToolbarCommandsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			//This always sends reset commands
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Reset:
					Clear();
					AddButtons(ViewModel.ToolbarCommands);
					break;
			}
		}
	}
}