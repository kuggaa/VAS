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
using VAS.Core.MVVMC;
using Gtk;
using VAS.Core.Interfaces.MVVMC;
using Atk;

namespace VAS.UI.Helpers.Bindings
{
	public class TextViewBinding : Binding<string>
	{
		TextView textView;
		bool inChange;

		public TextViewBinding (TextView textView, string propertyName) : base (propertyName)
		{
			this.textView = textView;
		}

		protected override void BindView ()
		{
			textView.Buffer.Changed += HandleBufferChanged;
		}

		protected override void UnbindView ()
		{
			textView.Buffer.Changed -= HandleBufferChanged;
		}

		protected override void WriteViewValue (string val)
		{
			inChange = true;
			textView.Buffer.Clear ();
			textView.Buffer.InsertAtCursor (val);
			inChange = false;
		}

		void HandleBufferChanged (object sender, EventArgs e)
		{
			if (!inChange) {
				WritePropertyValue (textView.Buffer.Text);
			}
		}

	}
}
