using System;
using Xamarin.Android.Prepare;

namespace Xamarin.Android.Shared
{
	partial class Log
	{
		static Context ctx;

		static LoggingVerbosity ConfigureLogVerbosity ()
		{
			return ctx != null ? ctx.LoggingVerbosity : Configurables.Defaults.LoggingVerbosity;
		}

		static bool ConfigureUseColor ()
		{
			return ctx?.UseColor ?? false;
		}

		public static void SetContext (Context context)
		{
			if (context == null)
				throw new ArgumentNullException (nameof (context));
			ctx = context;
		}
	}
}
