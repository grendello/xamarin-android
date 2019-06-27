using Xamarin.Android.Shared;

namespace Xamarin.Android.Tests.Driver
{
	class Context
	{
		bool canOutputColor;
		bool canConsoleUseUnicode;
		bool? useColor;
		bool? dullMode;

		/// <summary>
		///   Access the only instance of the Context class
		/// </summary>
		public static Context Instance { get; }

		/// <summary>
		///   Whether the current run of the test driver can interact with the user or not
		/// </summary>
		public bool InteractiveSession { get; }

		/// <summary>
		///   Whether or not log messages should use color
		/// </summary>
		public bool UseColor {
			get => canOutputColor && (!useColor.HasValue || useColor.Value);
			set => useColor = value;
		}

		/// <summary>
		///   Current session execution mode. See <see cref="t:ExecutionMode" />
		/// </summary>
		public ExecutionMode ExecutionMode { get; set; } = ExecutionMode.Standard;

		/// <summary>
		///   If <c>true</c> make messages logged to console not use colors, do not use "fancy" progress indicators etc
		/// </summary>
		public bool DullMode {
			get => dullMode.HasValue ? dullMode.Value : ExecutionMode == ExecutionMode.CI;
			set => dullMode = value;
		}

		static Context ()
		{
			Instance = new Context ();
		}

		Context ()
		{
			Helpers.ConfigureConsole ();
			canOutputColor = Helpers.ConsoleSupportsColor ();
			canConsoleUseUnicode = Helpers.ConsoleSupportsUnicode ();
			InteractiveSession = Helpers.IsInteractiveSession ();
		}
	}
}
