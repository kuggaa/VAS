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
using Gtk;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
using Atk;


namespace VAS.UI.Helpers.Bindings
{
	public static class BinginsExtensions
	{
		public static BindingContext GetBindingContext (this Widget widget)
		{
			if (widget.Data.ContainsKey ("Bindings")) {
				return widget.Data ["Bindings"] as BindingContext;
			}
			var context = new BindingContext ();
			widget.Data ["Bindings"] = context;
			return context;
		}

		public static LabelBinding Bind (this Label label, string propertyName)
		{
			return new LabelBinding (label, propertyName);
		}

		public static TextViewBinding Bind (this TextView textView, string propertyName)
		{
			return new TextViewBinding (textView, propertyName);
		}

		public static EntryBinding Bind (this Entry entry, string propertyName)
		{
			return new EntryBinding (entry, propertyName);
		}
	}
}
