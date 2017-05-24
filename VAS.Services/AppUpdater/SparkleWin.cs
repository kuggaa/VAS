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
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using VAS.Core.Common;
using VAS.Core.Interfaces;

namespace VAS.Services.AppUpdater
{
	public class SparkleWin : IAppUpdater
	{
		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		public delegate bool CanShutdownCallback ();
		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		public delegate void ShutdownRequestCallback ();

		[DllImport ("libsparkle.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern void win_sparkle_init ();
		[DllImport ("libsparkle.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern void win_sparkle_cleanup ();
		[DllImport ("libsparkle.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		static extern void win_sparkle_set_appcast_url (string url);
		[DllImport ("libsparkle.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
		static extern void win_sparkle_set_app_details (string company_name, string app_name, string app_version);
		[DllImport ("libsparkle.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern void win_sparkle_check_update_with_ui ();
		[DllImport ("libsparkle.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern void win_sparkle_set_automatic_check_for_updates (int state);
		[DllImport ("libsparkle.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		static extern void win_sparkle_set_can_shutdown_callback (
			[param: MarshalAs (UnmanagedType.FunctionPtr)] CanShutdownCallback callback
			);
		[DllImport ("libsparkle.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		static extern void win_sparkle_set_shutdown_request_callback (
			[param: MarshalAs (UnmanagedType.FunctionPtr)] ShutdownRequestCallback callback
			);

		CanShutdownCallback can_shutdown_callback;
		ShutdownRequestCallback shutdown_request_callback;

		public void Start (string companyName, string appName, string version, string feedURL, string libDir)
		{
			win_sparkle_set_automatic_check_for_updates (1);
			win_sparkle_set_app_details (companyName, appName, version);
			win_sparkle_set_appcast_url (feedURL);
			Log.Debug ("Registering win_sparkle_set_can_shutdown_callback");
			can_shutdown_callback = HandleCanShutdownCallback;
			win_sparkle_set_can_shutdown_callback (can_shutdown_callback);
			Log.Debug ("Registering win_sparkle_set_shutdown_request_callback");
			shutdown_request_callback = HandleShutdownRequestCallback;
			win_sparkle_set_shutdown_request_callback (shutdown_request_callback);
			win_sparkle_init ();
		}

		public void Stop ()
		{
			win_sparkle_cleanup ();
		}

		public void CheckForUpdates ()
		{
			win_sparkle_check_update_with_ui ();
		}

		bool HandleCanShutdownCallback ()
		{
			Log.Debug ("WinSparkle CanShutdownCallback called");
			Task<bool> taskResult = App.Current.GUIToolkit.Invoke (() => {
				return App.Current.Dialogs.QuestionMessage ("Do you want to close RiftAnalyst to install the update?", "Install Update Confirmation");
			});
			taskResult.ConfigureAwait (false);
			// Because this is a delegate and we don't have an option to await the result of the task, Wait() it with a
			// continuation in a different thread to avoid blocking.
			return taskResult.Result;
		}

		void HandleShutdownRequestCallback ()
		{
			Log.Debug ("WinSparkle ShutdownRequestCallback called");
			Task<bool> task = App.Current.GUIToolkit.Invoke (() => {
				return App.Current.GUIToolkit.Quit ();
			});
			task.ConfigureAwait (false);
			// Because this is a delegate and we don't have an option to await the result of the task, Wait() it with a
			// continuation in a different thread to avoid blocking.
			task.Wait ();
		}
	}
}
