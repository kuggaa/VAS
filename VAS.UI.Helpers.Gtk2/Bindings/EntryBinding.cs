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

namespace VAS.UI.Helpers.Bindings
{
	public class EntryBinding : Binding<string>
	{
		Entry entry;

		public EntryBinding (Entry entry, string propertyName) : base (propertyName)
		{
			this.entry = entry;
		}

		protected override void BindView ()
		{
			entry.Changed += HandleEntryChanged;
		}

		protected override void UnbindView ()
		{
			entry.Changed -= HandleEntryChanged;
		}

		protected override void WriteViewValue (string val)
		{
			entry.Text = val;
		}

		void HandleEntryChanged (object sender, EventArgs e)
		{
			WritePropertyValue (entry.Text);
		}
	}
}
