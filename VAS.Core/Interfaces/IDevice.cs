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
using System.Threading.Tasks;

namespace VAS.Core.Interfaces
{
	/// <summary>
	/// Interface to be used as a service to retrieve device information, such as ID, screen size, etc...
	/// or to perform device specific operations.
	/// </summary>
	public interface IDevice
	{
		/// <summary>
		/// Gets a unique ID for the device.
		/// </summary>
		/// <value>The identifier.</value>
		Guid ID { get; }

		/// <summary>
		/// Gets the version of the application in a device specific way.
		/// </summary>
		/// <value>The version.</value>
		Version Version { get; }

		/// <summary>
		/// Gets the build version of the application in a device specific way.
		/// It can be something like 1.3.7.262-28bf-dirty
		/// </summary>
		/// <value>The build version.</value>
		string BuildVersion { get; }

		/// <summary>
		/// Shares the files if sharing files is allowed in the platform.
		/// </summary>
		/// <param name="filePaths">File paths.</param>
		/// <param name="emailEnabled">If set to <c>true</c> email enabled.</param>
		void ShareFiles (string [] filePaths, bool emailEnabled);

		/// <summary>
		/// Return if camera and micro recording is allowed. 
		/// If permissions are denied by default, tries to request for permissions.
		/// </summary>
		/// <returns>The capture permission allowed.</returns>
		Task<bool> CheckCapturePermissions ();

		/// <summary>
		/// Return if external storage access is allowed. 
		/// If permissions are denied by default, tries to request for permissions.
		/// If the device doesn't support external storage, alwais returns false.
		/// </summary>
		/// <returns>The external storage permission allowed.</returns>
		Task<bool> CheckExternalStoragePermission ();
	}
}
