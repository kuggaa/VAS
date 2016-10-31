using System;
using Microsoft.HockeyApp.Services;
using Microsoft.HockeyApp.Channel;
using Microsoft.HockeyApp.DataContracts;
using VAS.KPI;

namespace Microsoft.HockeyApp
{
	internal class UnhandledExceptionTelemetryModule : IUnhandledExceptionTelemetryModule
	{
		private bool initialized;

		internal static Func<UnhandledExceptionEventArgs, bool> CustomUnhandledExceptionFunc {
			get; set;
		}

		public void Initialize ()
		{
			if (!initialized) {
				GLib.ExceptionManager.UnhandledException += (e) => {
					HockeyClient.Current.HandleException ((Exception)e.ExceptionObject);
					HockeyClient.Current.SendCrashesAsync ().ConfigureAwait (false);
				};

				initialized = true;
			}
		}

		public ITelemetry CreateCrashTelemetry (Exception exception, ExceptionHandledAt handledAt)
		{
			return new ExceptionTelemetry (exception) { HandledAt = handledAt };
		}
	}
}
