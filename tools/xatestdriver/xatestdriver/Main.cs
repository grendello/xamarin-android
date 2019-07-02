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
			static readonly char[] listSeparators = new char[] { ',' };

			List<string> testSuiteNames;
			List<string> testNames;

			public bool? EnableApkTests        { get; set; }
			public bool? EnableAabTests        { get; set; }
			public bool? EnableNUnitTests      { get; set; }
			public bool? EnableXUnitTests      { get; set; }
			public bool ListAll                { get; set; }
			public bool ListModes              { get; set; }
			public bool ListPolicies           { get; set; }
			public bool ListTestSuites         { get; set; }
			public bool RunTests               { get; set; } = true;
			public string ModeName             { get; set; }
			public string PolicyName           { get; set; }
			public string Configuration        { get; set; }
			public string ResponseFile         { get; set; }
			public bool? UseHeadlessEmulator   { get; set; } = Configurables.Defaults.UseHeadlessEmulator;

			public bool ShowHelp               { get; set; }

			public List<string> TestSuiteNames => testSuiteNames;
			public List<string> TestNames      => testNames;
			public bool AnyTestsSelected       => EnableApkTests.HasValue || EnableAabTests.HasValue || EnableNUnitTests.HasValue || EnableXUnitTests.HasValue;
			public bool ListSomething          => ListAll || ListModes || ListPolicies || ListTestSuites;

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

			public void AddTestSuiteNames (string commaSeparatedEntries)
			{
				AddToList (ref testSuiteNames, commaSeparatedEntries);
			}

			public void AddTestNames (string commaSeparatedEntries)
			{
				AddToList (ref testNames, commaSeparatedEntries);
			}

			void AddToList (ref List<string> list, string commaSeparatedEntries)
			{
				commaSeparatedEntries = commaSeparatedEntries?.Trim ();
				if (String.IsNullOrEmpty (commaSeparatedEntries))
					return;

				if (list == null)
					list = new List<string> ();

				list.AddRange (commaSeparatedEntries.Split (listSeparators, StringSplitOptions.RemoveEmptyEntries));
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
				"Usage: xatestdriver [OPTIONS] <TestSelection> [<Operation>]... [@ResponseFilePath]",
				$"Xamarin.Android v{BuildInfo.XAVersion} test driver",
				"",
				"At least one of the test selection options must be specified.",
				"",
				"Test selection:",
				{"A|all", "Execute all tests", v => parsedOptions.EnableAllTests ()},
				{"s|suite=", "Execute tests from the named {SUITE} only, a comma-separated list. Can be used multiple times.", v => parsedOptions.AddTestSuiteNames(v)},
				{"t|test=", "Run only the indicated tests. {LIST} is comma-separated. See the documentation for entry format. Can be used multiple times", v => parsedOptions.AddTestNames (v)},
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
				{"l|list:", "List: everything without any value passed or pass any string of letters shown here in brackets: [a]ll, [m]odes, [p]olicies, [t]ests", v => ParseListOptions (v, parsedOptions)},
				{"r|run", "Run tests (the default operation)", v => parsedOptions.RunTests = true},
				"",
				"Other:",
				{"m|mode=", "Use the named {MODE}", v => parsedOptions.ModeName = v},
				{"p|policy=", "Use the named {POLICY}", v => parsedOptions.PolicyName = v},
				{"c|configuration=", $"Run tests in the specified {{CONFIGURATION}} (Default: {Context.Instance.Configuration})", v => parsedOptions.Configuration = v},
				{"he|no-he|headless-emulator|no-headless-emulator",
				 $"Turn off (the no- prefix) or on using the Android headless emulator, if available. (Default: {Configurables.Defaults.UseHeadlessEmulator})",
				 n => parsedOptions.UseHeadlessEmulator = ParseOnOffOption (n, Configurables.Defaults.UseHeadlessEmulator)},
				"",
				{"h|help", "Show help", v => parsedOptions.ShowHelp = true},
			};

			opts.Parse (args);
			if (parsedOptions.ShowHelp) {
				opts.WriteOptionDescriptions (Console.Out);
				return 0;
			}

			if (parsedOptions.UseHeadlessEmulator.HasValue)
				Context.Instance.UseHeadlessEmulator = parsedOptions.UseHeadlessEmulator.Value;
			if (!String.IsNullOrEmpty (parsedOptions.Configuration))
				Context.Instance.Configuration = parsedOptions.Configuration;

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

		static bool ParseOnOffOption (string name, bool defaultValue)
		{
			name = name?.Trim ();
			if (String.IsNullOrEmpty (name))
				return defaultValue;

			return !name.StartsWith ("no-", StringComparison.OrdinalIgnoreCase);
		}

		static void ListSomething (ParsedOptions parsedOptions)
		{
			Context.Instance.Banner ("List");

			var tests = new Tests (Context.Instance);

			bool first = true;
			List (tests, parsedOptions, ListModes, ref first);
			List (tests, parsedOptions, ListPolicies, ref first);
			List (tests, parsedOptions, ListTestSuites, ref first);
		}

		static void List (Tests tests, ParsedOptions parsedOptions, Func<Tests, ParsedOptions, bool> func, ref bool first)
		{
			if (!first)
				Log.Instance.StatusLine ();
			if (func (tests, parsedOptions))
				first = false;
		}

		static bool ListModes (Tests tests, ParsedOptions parsedOptions)
		{
			if (!parsedOptions.ListModes && !parsedOptions.ListAll)
				return false;

			Log.Instance.StatusLine ("Modes");
			foreach (Mode mode in tests.Modes) {
				PrintInfo (mode, mode == tests.DefaultMode ? " [default]" : null);
			}

			return true;
		}

		static bool ListPolicies (Tests tests, ParsedOptions parsedOptions)
		{
			if (!parsedOptions.ListPolicies && !parsedOptions.ListAll)
				return false;

			Log.Instance.StatusLine ("Policies");
			foreach (Policy policy in tests.Policies) {
				PrintInfo (policy);
			}

			return true;
		}

		static bool ListTestSuites (Tests tests, ParsedOptions parsedOptions)
		{
			if (!parsedOptions.ListTestSuites && !parsedOptions.ListAll)
				return false;

			Log.Instance.StatusLine ("Tests");
			ListTests ("host", tests.NUnitTests);
			ListTests ("host", tests.XunitTests);
			ListTests ("device", tests.ApkTests);
			ListTests ("device", tests.AabTests);

			return true;

			void ListTests (string kind, List<TestItem> testList)
			{
				if (testList == null || testList.Count == 0)
					return;

				foreach (TestItem ti in testList) {
					PrintInfo (ti, $" [{kind}]");
				}
			}
		}

		static void PrintInfo (AppObject ao, string firstLineTail = null)
		{
			Log.Instance.Status ($"  {Context.Instance.Characters.Bullet} {ao.Name}", ConsoleColor.White);
			if (ao.Aliases != null) {
				Log.Instance.Status ($" / {MakeAliases (ao.Aliases)}", ConsoleColor.White);
			}
			Log.Instance.StatusLine (firstLineTail, ConsoleColor.Green);
			Log.Instance.StatusLine ($"    {ao.Description}");
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
