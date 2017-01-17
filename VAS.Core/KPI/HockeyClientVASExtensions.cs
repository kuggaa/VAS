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
//

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.HockeyApp;
using Microsoft.HockeyApp.Extensibility.Windows;
using Microsoft.HockeyApp.Services;
using VAS.KPI.Services;

namespace VAS.KPI
{
	public static class HockeyClientVASExtensions
	{
		/// <summary>
		/// Configures HockeyClient.
		/// </summary>
		/// <param name="client">HockeyClient object.</param>
		/// <param name="identifier">Identfier.</param>
		/// <param name="keepRunningAfterException">Keep running after exception.</param>
		/// <returns>Instance object.</returns>
		public async static Task<IHockeyClientConfigurable> Configure (this IHockeyClient client, string identifier)
		{
			client.AsInternal ().PlatformHelper = new HockeyPlatformHelperMono ();
			client.AsInternal ().AppIdentifier = identifier;

			ServiceLocator.AddService<IPlatformService> (new PlatformService ());

			ServiceLocator.AddService<BaseStorageService> (new StorageService ());
			ServiceLocator.AddService<IApplicationService> (new ApplicationService ());
			ServiceLocator.AddService<IDeviceService> (new DeviceService ());
			ServiceLocator.AddService<IHttpService> (new HttpClientTransmission ());
			ServiceLocator.AddService<IUnhandledExceptionTelemetryModule> (new UnhandledExceptionTelemetryModule ());

			await WindowsAppInitializer.InitializeAsync (identifier);
			return (IHockeyClientConfigurable)client;
		}

		/// <summary>
		/// Adds the handler for UnobservedTaskExceptions
		/// </summary>
		/// <param name="this"></param>
		/// <returns></returns>
		public static IHockeyClientConfigurable RegisterDefaultUnobservedTaskExceptionHandler (this IHockeyClientConfigurable @this)
		{
			TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
			return @this;
		}

		/// <summary>
		/// Removes the handler for UnobservedTaskExceptions
		/// </summary>
		/// <param name="this"></param>
		/// <returns></returns>
		public static IHockeyClientConfigurable UnregisterDefaultUnobservedTaskExceptionHandler (this IHockeyClientConfigurable @this)
		{
			TaskScheduler.UnobservedTaskException -= TaskScheduler_UnobservedTaskException;
			return @this;
		}

		static async void TaskScheduler_UnobservedTaskException (object sender, UnobservedTaskExceptionEventArgs e)
		{
			await HockeyClient.Current.AsInternal ().HandleExceptionAsync (e.Exception);
		}

		static async void Current_ThreadException (object sender, ThreadExceptionEventArgs e)
		{
			await HockeyClient.Current.AsInternal ().HandleExceptionAsync (e.Exception);
		}

		#region CrashHandling

		public static void HandleException (this IHockeyClient @this, Exception ex)
		{
			HockeyClient.Current.AsInternal ().HandleException (ex);
		}

		/// <summary>
		/// Send crashes to the HockeyApp server
		/// </summary>
		/// <param name="client"></param>
		/// <returns></returns>
		public static async Task<bool> SendCrashesAsync (this IHockeyClient client)
		{
			client.AsInternal ().CheckForInitialization ();
			bool result = await client.AsInternal ().SendCrashesAndDeleteAfterwardsAsync ().ConfigureAwait (false);
			return result;
		}

		#endregion

	}
}
