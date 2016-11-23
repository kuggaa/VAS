// 
//  Copyright (C) 2011 Andoni Morales Alastruey
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
using System.IO;
using VAS.Core.Interfaces;
using VAS.Core.MVVMC;
using VAS.Core.Serialization;

namespace VAS.Core.Common
{
	public static class Cloner
	{
		public static T Clone<T> (this T source, SerializationType type = SerializationType.Binary)
		{
			T retStorable;

			if (Object.ReferenceEquals (source, null))
				return default (T);

			// Binary deserialization fails in mobile platforms because of
			// https://bugzilla.xamarin.com/show_bug.cgi?id=37300
#if OSTYPE_ANDROID || OSTYPE_IOS
			type = SerializationType.Json;
#endif

			if (source is BindableBase) {
				type = SerializationType.Json;
			}

			if (type == SerializationType.Json) {
				retStorable = Serializer.Instance.JsonClone (source);
			} else {
				using (Stream s = new MemoryStream ()) {
					Serializer.Instance.Save<T> (source, s, type);
					s.Seek (0, SeekOrigin.Begin);
					retStorable = Serializer.Instance.Load<T> (s, type);
				}
			}
			if (source is IStorable) {
				(retStorable as IStorable).Storage = (source as IStorable).Storage;
			}
			return retStorable;
		}
	}
}
