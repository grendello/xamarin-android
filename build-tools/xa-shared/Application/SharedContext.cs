using System;

namespace Xamarin.Android.Shared
{
	class SharedContext
	{
		public const ConsoleColor BannerColor  = ConsoleColor.DarkGreen;
		public const ConsoleColor SuccessColor = ConsoleColor.Green;
		public const ConsoleColor FailureColor = ConsoleColor.Red;
		public const ConsoleColor WarningColor = ConsoleColor.Yellow;

		static readonly string buildTimeStamp;

		public static string BuildTimeStamp => buildTimeStamp;

		static SharedContext ()
		{
			var now = DateTime.Now;
			buildTimeStamp = $"{now.Year}{now.Month:00}{now.Day:00}T{now.Hour:00}{now.Minute:00}{now.Second:00}";
		}

		/// <summary>
		///   Print a "banner" to the output stream - will not show anything only if logging verbosity is set to <see
		///   cref="LoggingVerbosity.Quiet"/>
		/// </summary>
		public static void Banner (LoggingVerbosity loggingVerbosity, string text)
		{
			if (loggingVerbosity <= LoggingVerbosity.Quiet)
				return;

			Log.Instance.StatusLine ();
			Log.Instance.StatusLine ("=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=", BannerColor);
			Log.Instance.StatusLine (text, BannerColor);
			Log.Instance.StatusLine ("=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=", BannerColor);
			Log.Instance.StatusLine ();
		}
	}
}
