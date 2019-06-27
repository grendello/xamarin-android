using System;

using Mono.Options;

namespace Xamarin.Android.Tests.Driver
{
	class MainClass
	{
		sealed class ParsedOptions
		{
			public bool? RunApkTests   { get; set; }
			public bool? RunAabTests   { get; set; }
			public bool? RunNUnitTests { get; set; }
			public bool? RunXUnitTests { get; set; }

			public bool ShowHelp       { get; set; }

			public bool AnyTestsSelected => RunApkTests.HasValue || RunAabTests.HasValue || RunNUnitTests.HasValue || RunXUnitTests.HasValue;

			public void EnableAllTests ()
			{
				RunApkTests   = true;
				RunAabTests   = true;
				RunNUnitTests = true;
				RunXUnitTests = true;
			}

			public void EnableDeviceTests ()
			{
				RunApkTests = true;
				RunAabTests = true;
			}

			public void EnableHostTests ()
			{
				RunNUnitTests = true;
				RunXUnitTests = true;
			}
		}

		public static int Main (string [] args)
		{
			var parsedOptions = new ParsedOptions ();

			var opts = new OptionSet {
				"Usage: xatestdriver [OPTIONS] <TestType> ...",
				$"Xamarin.Android v{BuildInfo.XAVersion} test driver",
				"",
				"At least one of the test selection options must be specified.",
				"",
				{"A|all", "Execute all tests", v => parsedOptions.EnableAllTests ()},
				"",
				"On device tests:",
				{"a|apk", "Execute APK tests", v => parsedOptions.RunApkTests = true},
				{"b|aab", "Execute AAB tests", v => parsedOptions.RunAabTests = true},
				{"d|device", "Execute device (both APK and AAB) tests", v => parsedOptions.EnableDeviceTests ()},
				"",
				"Host unit tests:",
				{"n|nunit", "Execute NUnit tests", v => parsedOptions.RunNUnitTests = true},
				{"x|xunit", "Execute xUnit tests", v => parsedOptions.RunXUnitTests = true},
				{"u|unit", "Execute unit (both NUnit and xUnit) tests", v => parsedOptions.EnableHostTests ()},
				"",
				{"h|help", "Show help", v => parsedOptions.ShowHelp = true},
			};

			opts.Parse (args);
			if (parsedOptions.ShowHelp) {
				opts.WriteOptionDescriptions (Console.Out);
				return 0;
			}

			if (!parsedOptions.AnyTestsSelected) {
				Console.Error.WriteLine ("At least one test selection option must be passed to the program. Please see the --help output");
				return 1;
			}

			return 0;
		}
	}
}
