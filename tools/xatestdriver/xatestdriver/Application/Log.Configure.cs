using System;

namespace Xamarin.Android.Shared
{
	partial class Log
	{
		static LoggingVerbosity ConfigureLogVerbosity ()
		{
			return LoggingVerbosity.Verbose;
		}

		static bool ConfigureUseColor ()
		{
			return true;
		}
	}
}
