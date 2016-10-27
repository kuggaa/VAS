//
//  Copyright (C) 2016 
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
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Microsoft.HockeyApp;

namespace VAS.KPI
{
	/// <summary>
	/// HockeyPlatformHelper for Mono Net45.
	/// </summary>
	public class HockeyPlatformHelperMono : IHockeyPlatformHelper
	{

		private const string FILE_PREFIX = "HA__SETTING_";
		IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore (IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);

		/// <summary>
		/// Sets setting value.
		/// </summary>
		/// <param name="key">Key.</param>
		/// <param name="value">Value.</param>
		public void SetSettingValue (string key, string value)
		{
			using (var fileStream = isoStore.OpenFile (FILE_PREFIX + key, FileMode.Create, FileAccess.Write)) {
				using (var writer = new StreamWriter (fileStream)) {
					writer.Write (value);
				}
			}
		}

		/// <summary>
		/// Gets setting value.
		/// </summary>
		/// <param name="key">Key.</param>
		/// <returns>Setting value.</returns>
		public string GetSettingValue (string key)
		{
			if (isoStore.FileExists (FILE_PREFIX + key)) {
				using (var fileStream = isoStore.OpenFile (FILE_PREFIX + key, FileMode.Open, FileAccess.Read)) {
					using (var reader = new StreamReader (fileStream)) {
						return reader.ReadToEnd ();
					}
				}
			}
			return null;
		}

		/// <summary>
		/// Removes setting value.
		/// </summary>
		/// <param name="key">Key.</param>
		public void RemoveSettingValue (string key)
		{
			if (isoStore.FileExists (FILE_PREFIX + key)) {
				isoStore.DeleteFile (FILE_PREFIX + key);
			}
		}


		#region File access

		// ToDo: Remove warning suppression
#pragma warning disable 1998
		/// <summary>
		/// Deletes file.
		/// </summary>
		/// <param name="fileName">File name.</param>
		/// <param name="folderName">Folder name.</param>
		/// <returns>True if file deleted, otherwise false.</returns>
		public async Task<bool> DeleteFileAsync (string fileName, string folderName = null)
		{
			if (isoStore.FileExists ((folderName ?? "") + Path.DirectorySeparatorChar + fileName)) {
				isoStore.DeleteFile ((folderName ?? "") + Path.DirectorySeparatorChar + fileName);
				return true;
			}
			return false;
		}

		/// <summary>
		/// Determines if file exists.
		/// </summary>
		/// <param name="fileName">File name.</param>
		/// <param name="folderName">Folder name.</param>
		/// <returns>True if file exists, otherwise false.</returns>
		public async Task<bool> FileExistsAsync (string fileName, string folderName = null)
		{
			return isoStore.FileExists ((folderName ?? "") + Path.DirectorySeparatorChar + fileName);
		}

		/// <summary>
		/// Gets stream.
		/// </summary>
		/// <param name="fileName">File name.</param>
		/// <param name="folderName">Folder name.</param>
		/// <returns>Stream object.</returns>
		public async Task<Stream> GetStreamAsync (string fileName, string folderName = null)
		{
			return isoStore.OpenFile ((folderName ?? "") + Path.DirectorySeparatorChar + fileName, FileMode.Open, FileAccess.Read);
		}

		/// <summary>
		/// Gets file names.
		/// </summary>
		/// <param name="folderName">Folder name.</param>
		/// <param name="fileNamePattern">File name pattern.</param>
		/// <returns>File name.</returns>
		public async Task<IEnumerable<string>> GetFileNamesAsync (string folderName = null, string fileNamePattern = null)
		{
			try {
				string pattern = (folderName ?? "") + Path.DirectorySeparatorChar + fileNamePattern ?? "*";
				if (!isoStore.FileExists (folderName ?? "")) {
					isoStore.CreateDirectory ((folderName ?? ""));
				}
				return isoStore.GetFileNames (pattern);
			} catch (DirectoryNotFoundException) {
				return new string [0];
			}
		}
#pragma warning restore 1998

		/// <summary>
		/// Writes stream to file.
		/// </summary>
		/// <param name="dataStream">Data stream.</param>
		/// <param name="fileName">File name.</param>
		/// <param name="folderName">Folder name.</param>
		/// <returns>Task object.</returns>
		public async Task WriteStreamToFileAsync (Stream dataStream, string fileName, string folderName = null)
		{
			// Ensure crashes folder exists
			if (!isoStore.DirectoryExists (folderName)) {
				isoStore.CreateDirectory (folderName);
			}

			using (var fileStream = isoStore.OpenFile ((folderName ?? "") + Path.DirectorySeparatorChar + fileName, FileMode.Create, FileAccess.Write)) {
				await dataStream.CopyToAsync (fileStream);
			}
		}

		/// <summary>
		/// Gets a value indicating whether a platform supports sync writes.
		/// </summary>
		public bool PlatformSupportsSyncWrite {
			get { return true; }
		}

		/// <summary>
		/// Writes stream to file.
		/// </summary>
		/// <param name="dataStream">Data stream.</param>
		/// <param name="fileName">File name.</param>
		/// <param name="folderName">Folder name.</param>
		public void WriteStreamToFileSync (Stream dataStream, string fileName, string folderName = null)
		{
			// Ensure crashes folder exists
			if (!isoStore.DirectoryExists (folderName)) {
				isoStore.CreateDirectory (folderName);
			}

			using (var fileStream = isoStore.OpenFile ((folderName ?? "") + Path.DirectorySeparatorChar + fileName, FileMode.Create, FileAccess.Write)) {
				dataStream.CopyTo (fileStream);
			}
		}


		#endregion


		string _appPackageName = null;

		/// <summary>
		/// Gets or sets application package name.
		/// </summary>
		public string AppPackageName {
			get {
				if (_appPackageName == null) {
					_appPackageName = Assembly.GetEntryAssembly ().EntryPoint?.DeclaringType?.Namespace ?? "Unknown";
				}
				return _appPackageName;
			}
			set {
				_appPackageName = value;
			}
		}

		string _appVersion = null;

		/// <summary>
		/// Gets or sets application version.
		/// </summary>
		public string AppVersion {
			get {

				if (_appVersion == null) {
					//ClickOnce
					try {
						var type = Type.GetType ("System.Deployment.Application.ApplicationDeployment");
						object deployment = type.GetMethod ("CurrentDeployment").Invoke (null, null);
						Version version = type.GetMethod ("CurrentVersion").Invoke (deployment, null) as Version;
						_appVersion = version.ToString ();
					} catch (Exception) { }
					//Excecuting Assembly
					_appVersion = Assembly.GetCallingAssembly ().GetName ().Version.ToString ();
				}
				return _appVersion ?? "0.0.0-unknown";
			}
			set {
				_appVersion = value;
			}
		}

		/// <summary>
		/// Gets OS version.
		/// </summary>
		public string OSVersion {
			get { return Environment.OSVersion.Version.ToString () + " " + Environment.OSVersion.ServicePack; }
		}

		/// <summary>
		/// Gets OS Platform.
		/// </summary>
		public string OSPlatform {
			get { return (Type.GetType ("Mono.Runtime") == null) ? "Windows" : "Mono"; }
		}

		/// <summary>
		/// Gets SDK version.
		/// </summary>
		public string SDKVersion {
			get { return HockeyConstants.SDKVERSION; }
		}

		/// <summary>
		/// Gets SDK name.
		/// </summary>
		public string SDKName {
			get { return HockeyConstants.SDKNAME; }
		}

		/// <summary>
		/// Gets User agent.
		/// </summary>
		public string UserAgentString {
			get { return HockeyConstants.USER_AGENT_STRING; }
		}

		private string _productID = null;

		/// <summary>
		/// Gets product id.
		/// </summary>
		public string ProductID {
			get { return _productID; }
			set { _productID = value; }
		}

		/// <summary>
		/// Gets manufacturer.
		/// </summary>
		public string Manufacturer {
			get {
				//TODO System.Management referenzieren !?
				/*
                Type.GetType
                ManagementClass mc = new ManagementClass("Win32_ComputerSystem");
            //collection to store all management objects
            ManagementObjectCollection moc = mc.GetInstances();
            if (moc.Count != 0)
            {
                foreach (ManagementObject mo in mc.GetInstances())
                {
                 mo["Manufacturer"].ToString()
                */
				return null;
			}
		}

		/// <summary>
		/// Gets model.
		/// </summary>
		public string Model {
			get {
				//TODO siehe Manufacturer mit "Model"
				return null;
			}
		}

	}
}
