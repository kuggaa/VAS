//
//  Copyright (C) 2017 ${CopyrightHolder}
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
using Gtk;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;

namespace VAS.UI.Helpers.Bindings
{
	public class HScaleCommandBinding : CommandBinding
	{
		HScale hScale;
		bool skipCommand;
		double defaultValue, min, max, step, page;

		public HScaleCommandBinding (HScale scale, Func<IViewModel, Command> commandFunc, double defaultValue)
			: base (commandFunc, null)
		{
			hScale = scale;
			this.defaultValue = defaultValue;
			hScale.Value = defaultValue;
		}

		public HScaleCommandBinding (HScale scale, Func<IViewModel, Command> commandFunc, double defaultValue,
		                      double min, double max, double step, double page) : this (scale, commandFunc, defaultValue)
		{
			this.min = min;
			this.max = max;
			this.step = step;
			this.page = page;
		}

		protected override void BindView ()
		{
			UpdateHScale ();
			hScale.ValueChanged += HandleValueChanged;
		}

		protected override void UnbindView ()
		{
			hScale.ValueChanged -= HandleValueChanged;
		}

		protected override void BindViewModel ()
		{
			UnbindViewModel ();
			base.BindViewModel ();
			if (Command != null) {
				UpdateHScale ();
				Command.CanExecuteChanged += HandleCanExecuteChanged;
			}
		}

		protected override void UnbindViewModel ()
		{
			if (Command != null) {
				Command.CanExecuteChanged -= HandleCanExecuteChanged;
			}
			base.UnbindViewModel ();
		}

		void UpdateHScale ()
		{
			hScale.SetRange (min, max);
			hScale.SetIncrements (step, page);
			hScale.Sensitive = Command.CanExecute ();
		}

		void HandleCanExecuteChanged (object sender, EventArgs args)
		{
			UpdateHScale ();
		}

		void HandleValueChanged (object sender, EventArgs args)
		{
			if(skipCommand) {
				skipCommand = false;
				return;
			}
			Command.Execute (hScale.Value);

			var limitedCommand = Command as LimitationCommand;
			if (limitedCommand != null && !limitedCommand.Executed) {
				skipCommand = true;
				hScale.Value = defaultValue;
			}
		}
	}
}
