//
//  Copyright (C) 2007-2009 Andoni Morales Alastruey
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
using Newtonsoft.Json;
using VAS.Core.Interfaces;
using VAS.Core.MVVMC;

namespace VAS.Core.Store
{
	[Serializable]
	public class Tag : BindableBase
	{
		public static Tag EmptyTag = new Tag (Catalog.GetString ("Empty"));

		public Tag (string value, string grp = "Default")
		{
			Group = grp;
			Value = value;
			HotKey = new HotKey ();
			HotKey.IsChanged = false;
		}

		public string Group {
			set;
			get;
		}

		public string Value {
			get;
			set;
		}

		public HotKey HotKey {
			get;
			set;
		}

		// FIXME: In the future we should consider re-overriding the GetHashCode method using
		// immutable properties. Another solution would be not overriding the Equals method
		public override bool Equals (object obj)
		{
			Tag tag = obj as Tag;
			if (tag == null)
				return false;
			return Value == tag.Value && Group == tag.Group;
		}

		public override string ToString ()
		{
			return String.Format ("{0} ({1})", Value, Group);
		}
	}
}
