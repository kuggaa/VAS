﻿//
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

namespace VAS.Core.MVVMC
{
	/// <summary>
	/// A base class for objects implementing <see cref="IDisposable"/> using Microsoft's recommend pattern.
	/// Classes inheriting from <see cref="DisposableBase"/> only need to override the Dispose function.
	/// </summary>
	[Serializable]
	public class DisposableBase : IDisposable
	{
		~DisposableBase ()
		{
			Dispose (false);
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (Disposed)
				return;
			Disposed = true;
		}

		protected bool Disposed { get; private set; } = false;
	}
}
