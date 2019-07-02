using System;
using System.Collections.Generic;

using Mono.Options;
using Xamarin.Android.Shared;

namespace Xamarin.Android.Tests.Driver
{
	class MainClass
	{
		sealed class ParsedOptions
		{
			public bool? EnableApkTests   { get; set; }
			public bool? EnableAabTests   { get; set; }
			public bool? EnableNUnitTests { get; set; }
			public bool? EnableXUnitTests { get; set; }
			public bool ListAll           { get; set; }
			public bool ListModes         { get; set; }
			public bool ListPolicies      { get; set; }
			public bool ListTestSuites    { get; set; }
			public bool RunTests          { get; set; } = true;
			public string ModeName        { get; set; }
			public string PolicyName      { get; set; }

			public bool ShowHelp          { get; set; }

			public bool AnyTestsSelected  => EnableApkTests.HasValue || EnableAabTests.HasValue || EnableNUnitTests.HasValue || EnableXUnitTests.HasValue;
			public bool ListSomething     => ListAll || ListModes || ListPolicies || ListTestSuites;

			public void EnableAllTests ()
			{
				EnableApkTests   = true;
				EnableAabTests   = true;
				EnableNUnitTests = true;
				EnableXUnitTests = true;
			}

			public void EnableDeviceTests ()
			{
				EnableApkTests = true;
				EnableAabTests = true;
			}

			public void EnableHostTests ()
			{
				EnableNUnitTests = true;
				EnableXUnitTests = true;
			}
		}

		public static int Main (string [] args)
		{
			try {
				return Run (args);
			} catch (AggregateException aex) {
				foreach (Exception ex in aex.InnerExceptions) {
					Helpers.PrintException (ex);
				}
			} catch (Exception ex) {
				Helpers.PrintException (ex);
			} finally {
				Log.Instance.Dispose ();
				Helpers.ResetConsole ();
			}

			return 1;
		}

		static int Run (string[] args)
		{
			var parsedOptions = new ParsedOptions ();

			var opts = new OptionSet {
				"Usage: xatestdriver [OPTIONS] <TestSelection> [<Operation>]...",
				$"Xamarin.Android v{BuildInfo.XAVersion} test driver",
				"",
				"At least one of the test selection options must be specified.",
				"",
				"Test selection:",
				{"A|all", "Execute all tests", v => parsedOptions.EnableAllTests ()},
				"",
				"On device tests:",
				{"a|apk", "Execute APK tests", v => parsedOptions.EnableApkTests = true},
				{"b|aab", "Execute AAB tests", v => parsedOptions.EnableAabTests = true},
				{"d|device", "Execute device (both APK and AAB) tests", v => parsedOptions.EnableDeviceTests ()},
				"",
				"Host unit tests:",
				{"n|nunit", "Execute NUnit tests", v => parsedOptions.EnableNUnitTests = true},
				{"x|xunit", "Execute xUnit tests", v => parsedOptions.EnableXUnitTests = true},
				{"u|unit", "Execute unit (both NUnit and xUnit) tests", v => parsedOptions.EnableHostTests ()},
				"",
				"Operations:",
				{"l|list:", "List: everything without any value passed or pass any string of letters shown here in brackets: [a]ll, [m]odes, [p]olicies, [t]ests", v => ParseListOptions (v, parsedOptions) },
				{"r|run", "Run tests (the default operation)", v => parsedOptions.RunTests = true },
				"",
				"Other:",
				{"m|mode=", "Use the named {MODE}", v => parsedOptions.ModeName = v },
				{"p|policy=", "Use the named {POLICY}", v => parsedOptions.PolicyName = v },
				"",
				{"h|help", "Show help", v => parsedOptions.ShowHelp = true},
			};

			opts.Parse (args);
			if (parsedOptions.ShowHelp) {
				opts.WriteOptionDescriptions (Console.Out);
				return 0;
			}

			if (!Context.Instance.Init ()) {
				Log.Instance.ErrorLine ("Failed to initialize application context");
				return 1;
			}

			if (parsedOptions.ListSomething) {
				ListSomething (parsedOptions);
			}

			if (parsedOptions.RunTests && !parsedOptions.AnyTestsSelected) {
				Log.Instance.ErrorLine ("At least one test selection option must be passed to the program. Please see the --help output");
				return 1;
			}

			return 0;
		}

		static void ListSomething (ParsedOptions parsedOptions)
		{
			Context.Instance.Banner ("List");

			bool first = true;
			List (parsedOptions, ListModes, ref first);
			List (parsedOptions, ListPolicies, ref first);
			List (parsedOptions, ListTestSuites, ref first);
		}

		static void List (ParsedOptions parsedOptions, Func<ParsedOptions, bool> func, ref bool first)
		{
			if (!first)
				Log.Instance.StatusLine ();
			if (func (parsedOptions))
				first = false;
		}

		static bool ListModes (ParsedOptions parsedOptions)
		{
			if (!parsedOptions.ListModes && !parsedOptions.ListAll)
				return false;

			Log.Instance.StatusLine ("Modes");
			foreach (Mode mode in Tests.Modes) {
				string defaultMode = mode == Tests.DefaultMode ? " [default]" : null;
				Log.Instance.Status ($"  {Context.Instance.Characters.Bullet} {mode.Name}", ConsoleColor.White);
				if (mode.Aliases != null) {
					Log.Instance.Status ($" / {MakeAliases (mode.Aliases)}", ConsoleColor.White);
				}
				Log.Instance.StatusLine (defaultMode, ConsoleColor.Green);
				Log.Instance.StatusLine ($"    {mode.Description}");
			}

			return true;
		}

		static bool ListPolicies (ParsedOptions parsedOptions)
		{
			if (!parsedOptions.ListPolicies && !parsedOptions.ListAll)
				return false;

			Log.Instance.StatusLine ("Policies");
			foreach (Policy policy in Tests.Policies) {
				Log.Instance.Status ($"  {Context.Instance.Characters.Bullet} {policy.Name}", ConsoleColor.White);
				if (policy.Aliases != null) {
					Log.Instance.Status ($" / {MakeAliases (policy.Aliases)}", ConsoleColor.White);
				}
				Log.Instance.StatusLine ();
				Log.Instance.StatusLine ($"    {policy.Description}");
			}

			return true;
		}

		static bool ListTestSuites (ParsedOptions parsedOptions)
		{
			if (!parsedOptions.ListTestSuites && !parsedOptions.ListAll)
				return false;

			return true;
		}

		static string MakeAliases (IList<string> list)
		{
			if (list == null || list.Count == 0)
				return String.Empty;
			return String.Join (" / ", list);
		}

		static void ParseListOptions (string value, ParsedOptions parsedOptions)
		{
			parsedOptions.RunTests = false;
			value = value?.Trim ();
			if (String.IsNullOrEmpty (value)) {
				parsedOptions.ListAll = true;
				return;
			}

			bool setSome = false;
			for (int i = 0; i < value.Length; i++) {
				switch (Char.ToLowerInvariant(value [i])) {
					case 'a':
						setSome = true;
						parsedOptions.ListAll = true;
						return;

					case 'm':
						setSome = true;
						parsedOptions.ListModes = true;
						break;

					case 'p':
						setSome = true;
						parsedOptions.ListPolicies = true;
						break;

					case 't':
						setSome = true;
						parsedOptions.ListTestSuites = true;
						break;
				}
			}

			if (!setSome)
				parsedOptions.ListAll = true;
		}
	}
}
