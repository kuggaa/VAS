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
using System.Windows.Input;
using Gtk;
using VAS.Core.Interfaces.MVVMC;
using System.Reflection;
using System.ComponentModel;
using VAS.Core.Store.Drawables;
using System.Diagnostics.Contracts;

namespace VAS.UI.Helpers.Gtk2
{
	public static class Bindings
	{

		static void BindWidget (Widget widget, ICommand command, object parameter)
		{
			widget.Sensitive = command.CanExecute (null);
			EventHandler handler = (sender, e) => {
				widget.Sensitive = command.CanExecute (null);
			};
			command.CanExecuteChanged += handler;
			widget.Destroyed += (sender, e) => {
				command.CanExecuteChanged -= handler;
			};
		}

		static public void Bind (this Button button, ICommand command, object parameter)
		{
			Contract.Requires (button != null);
			Contract.Requires (command != null);

			BindWidget (button, command, parameter);
			button.Clicked += (sender, e) => {
				command.Execute (parameter);
			};
		}

		static public void Bind (this Entry entry, IViewModel viewModel, string propertyName)
		{
			Contract.Requires (entry != null);
			Contract.Requires (viewModel != null);
			Contract.Requires (propertyName != null);

			PropertyInfo property = viewModel.GetType ().GetProperty (propertyName);

			if (property == null) {
				throw new ArgumentException ("Property " + propertyName + " not found in view model " + viewModel);
			}

			if (property.PropertyType != typeof (string)) {
				throw new ArgumentException ("Binded property must be of type String");
			}

			EventHandler textChanged = (sender, e) => {
				property.SetMethod.Invoke (viewModel, new object [] { entry.Text });
			};
			PropertyChangedEventHandler propertyChanged = (sender, e) => {
				if (e.PropertyName == propertyName) {
					entry.Text = (string)property.GetMethod.Invoke (sender, null);
				}
			};
			viewModel.PropertyChanged += propertyChanged;
			entry.Changed += textChanged;

			entry.DestroyEvent += (o, args) => {
				viewModel.PropertyChanged -= propertyChanged;
			};
		}

		static public void Bind (this TextView textView, IViewModel viewModel, string propertyName)
		{
			Contract.Requires (textView != null);
			Contract.Requires (viewModel != null);
			Contract.Requires (propertyName != null);

			TextBuffer buffer = textView.Buffer;
			PropertyInfo property = viewModel.GetType ().GetProperty (propertyName);

			if (property == null) {
				throw new ArgumentException ("Property " + propertyName + " not found in view model " + viewModel);
			}

			if (property.PropertyType != typeof (string)) {
				throw new ArgumentException ("Binded property must be of type String");
			}

			EventHandler textChanged = (sender, e) => {
				property.SetMethod.Invoke (viewModel, new object [] {
					buffer.GetText (buffer.StartIter, buffer.EndIter, true)});
			};
			PropertyChangedEventHandler propertyChanged = (sender, e) => {
				if (e.PropertyName == propertyName) {
					buffer.Text = (string)property.GetMethod.Invoke (sender, null);
				}
			};
			viewModel.PropertyChanged += propertyChanged;
			buffer.Changed += textChanged;

			textView.DestroyEvent += (o, args) => {
				viewModel.PropertyChanged -= propertyChanged;
			};
		}
	}
}
