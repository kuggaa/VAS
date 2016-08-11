//
//  Copyright (C) 2016 Fluendo S.A.
using System;
using System.Collections.Generic;
using Gtk;
using System.Linq;

namespace VAS.UI.Dialog
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class ChooseOptionDialog : Gtk.Dialog
	{
		public ChooseOptionDialog (Window parent)
		{
			TransientFor = parent;
			this.Build ();
		}

		public Dictionary<string, object> Options {
			set {
				RadioButton first = null;
				SelectedOption = value.Values.FirstOrDefault ();
				foreach (string desc in value.Keys) {
					first = new RadioButton (first, desc);
					first.Toggled += (sender, e) => {
						if ((sender as RadioButton).Active) {
							SelectedOption = value [desc];
						}
					};
					first.ShowAll ();
					optionsbox.PackStart (first, false, true, 0);
				}
			}
		}

		public object SelectedOption {
			get;
			protected set;
		}
	}
}

