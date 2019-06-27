using System;
using System.Collections.Concurrent;

namespace Xamarin.Android.Shared
{
	partial class Log
	{
		void InitOS ()
		{}

		void ShutdownOS ()
		{}

		void DoConsoleWrite (string message)
		{
			Console.Write (message);
		}

		void DoConsoleWriteLine (string message)
		{
			Console.WriteLine (message);
		}
	}
}
