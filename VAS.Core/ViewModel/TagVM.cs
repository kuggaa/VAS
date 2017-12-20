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
using System.ComponentModel;
using VAS.Core.MVVMC;
using VAS.Core.Store;

namespace VAS.Core.ViewModel
{
	/// <summary>
	/// Tag ViewModel
	/// </summary>
	public class TagVM : ViewModelBase<Tag>
	{

		public TagVM () {
			HotKey = new HotKeyVM ();	
		}

		[PropertyChanged.DoNotCheckEquality]
		public override Tag Model {
			get {
				return base.Model;
			}
			set {
				base.Model = value;
				HotKey.Model = value?.HotKey;
			}
		}

		/// <summary>
		/// Gets or sets the value.
		/// </summary>
		/// <value>The value.</value>
		public string Value {
			get {
				return Model.Value;
			}
			set {
				Model.Value = value;
			}
		}

		/// <summary>
		/// Gets or sets the group.
		/// </summary>
		/// <value>The group.</value>
		public string Group {
			get {
				return Model.Group;
			}
			set {
				Model.Group = value;
			}
		}

		/// <summary>
		/// Gets the hot key.
		/// </summary>
		/// <value>The hot key.</value>
		public HotKeyVM HotKey {
			get;
		}

		public override bool Equals (object obj)
		{
			TagVM tag = obj as TagVM;
			if (tag == null)
				return false;
			return Model.Equals (tag.Model);
		}

		public override int GetHashCode ()
		{
			return Model.GetHashCode ();
		}
	}
}
