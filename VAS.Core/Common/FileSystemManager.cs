//
//  Copyright (C) 2017 FLUENDO S.A.
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
using VAS.Core.Common;
using VAS.Core.Interfaces;

namespace VAS.Core
{
	/// <summary>
	/// File system service, access to the file system alwasys trough this service
	/// Extend this service to add the necessary functionality
	/// </summary>
	public class FileSystemManager : IFileSystemManager
	{
		/// <summary>
		/// Exists the file in the specified path.
		/// </summary>
		/// <param name="path">Path.</param>
		public bool FileExists (string path)
		{
			return File.Exists (path);
		}

		/// <summary>
		/// Exists the directory in the specified path.
		/// </summary>
		/// <param name="path">Path.</param>
		public bool DirectoryExists (string path)
		{
			return Directory.Exists (path);
		}

		/// <summary>
		/// Copy a whole directory to the specified path.
		/// </summary>
		/// <param name="sourcePath">Source dir name.</param>
		/// <param name="destPath">Destination dir name.</param>
		/// <param name="copySubDirs">If set to <c>true</c> copy sub dirs.</param>
		public void CopyDirectory (string sourcePath, string destPath, bool copySubDirs)
		{
			// Get the subdirectories for the specified directory.
			DirectoryInfo dir = new DirectoryInfo (sourcePath);

			if (!dir.Exists) {
				throw new DirectoryNotFoundException ("Source directory does not exist or could not be found: " + sourcePath);
			}

			DirectoryInfo [] dirs = dir.GetDirectories ();
			// If the destination directory doesn't exist, create it.
			if (!Directory.Exists (destPath)) {
				Directory.CreateDirectory (destPath);
			}

			// Get the files in the directory and copy them to the new location.
			FileInfo [] files = dir.GetFiles ();
			foreach (FileInfo file in files) {
				string temppath = Path.Combine (destPath, file.Name);
				file.CopyTo (temppath, false);
			}

			// If copying subdirectories, copy them and their contents to new location.
			if (copySubDirs) {
				foreach (DirectoryInfo subdir in dirs) {
					string temppath = Path.Combine (destPath, subdir.Name);
					CopyDirectory (subdir.FullName, temppath, copySubDirs);
				}
			}
		}

		/// <summary>
		/// Gets the path of the specified directory name.
		/// </summary>
		/// <param name="sourceDirName">Source dir name.</param>
		public string GetDataDirPath (string dirname)
		{
			return Utils.GetDataDirPath (dirname);
		}
	}
}
