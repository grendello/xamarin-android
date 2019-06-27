using System;
using System.IO;
using System.Text;

namespace Xamarin.Android.Shared
{
	static class Helpers
	{
		public static void ConfigureConsole ()
		{
			try {
				// This may throw on Windows
				Console.CursorVisible = false;
			} catch (IOException) {
				// Ignore
			}
		}

		public static bool ConsoleSupportsColor ()
		{
			// Standard Console class offers no way to detect if the terminal can use color, so we use this rather poor
			// way to detect it
			try {
				ConsoleColor color = Console.ForegroundColor;
			} catch (IOException) {
				return false;
			}

			return true;
		}

		public static bool ConsoleSupportsUnicode ()
		{
			return
				Console.OutputEncoding is UTF7Encoding ||
				Console.OutputEncoding is UTF8Encoding ||
				Console.OutputEncoding is UTF32Encoding ||
				Console.OutputEncoding is UnicodeEncoding;
		}

		public static bool IsInteractiveSession ()
		{
			Log.Instance.Todo ("better checks for interactive session (isatty?)");
			try {
				return !Console.IsOutputRedirected;
			} catch (IOException) {
				return true; // Windows may throw here, but it also means we're not redirected
			}
		}
	}
}
