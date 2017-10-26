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
using System.Linq.Expressions;
using Gtk;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
using VAS.UI;
using VAS.UI.Helpers;
using VAS.UI.Helpers.Bindings;

namespace VAS.Bindings
{
	/// <summary>
	/// This class provides a command binding to an <see cref="SliderView"/> that is shown by a button.
	/// Button click event is autowired to <see cref="SliderView.Show"/> method.
	/// Additionally provides <see cref="Vas.Core.MVVMC.OneWayPropertyBinding"/> from a property on <see cref="IViewModel"/> 
	/// that performs a callback when it is changed
	/// </summary>
	public class SliderViewCommandBinding : CommandBinding
	{

		SliderView sliderView;
		Button showButton;
		OneWayPropertyBinding<double> propBinding;
		Func<IViewModel, Command<double>> commandFunc;


		public SliderViewCommandBinding (SliderView sliderView, Button showButton, Func<IViewModel, Command<double>> commandFunc,
										Expression<Func<IViewModel, double>> propertyExpression, Action<double> updateViewAction) : base (commandFunc, null)
		{
			this.sliderView = sliderView;
			this.commandFunc = commandFunc;
			this.showButton = showButton;
			WireShowButtonClickEvent (showButton);

			if (propertyExpression != null && updateViewAction != null)
				propBinding = new OneWayPropertyBinding<double> (propertyExpression, updateViewAction);
		}

		protected override void BindViewModel ()
		{
			base.BindViewModel ();
			if (propBinding != null) propBinding.ViewModel = ViewModel;
		}

		protected override void BindView ()
		{
			sliderView.ValueChanged += HandleValueChanged;
		}

		protected override void UnbindView ()
		{
			sliderView.ValueChanged -= HandleValueChanged;
		}

		protected override void UpdateView ()
		{
			showButton.Configure (Command.Icon, Command.Text, Command.ToolTipText, null);
			showButton.Sensitive = Command.CanExecute ();
		}

		protected override void HandleCanExecuteChanged (object sender, EventArgs args)
		{
			sliderView.Sensitive = Command.CanExecute ();
		}

		void WireShowButtonClickEvent (Button button)
		{
			button.Clicked += (sender, e) => {
				sliderView.Show ();
			};
			button.Bind (commandFunc);
		}

		void HandleValueChanged (double level)
		{
			if (Command == null) return;
			if (!Command.CanExecute ()) return;
			Command.Execute (level);
		}
	}
}
