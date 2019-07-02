using System;
using System.IO;
using System.Collections.Generic;

namespace Xamarin.Android.Tests.Driver
{
	class Tests
	{
		readonly Policy serialPolicy;
		readonly Policy parallelPolicy;
		readonly Policy massivelyParallelPolicy;

		readonly Mode standardMode;
		readonly Mode heavyMode;

		readonly NUnitDispatcher nunitDispatcher;
		readonly XUnitDispatcher xunitDispatcher;
		readonly ApkDispatcher apkDispatcher;
		readonly AabDispatcher aabDispatcher;

		public readonly List<TestItem> NUnitTests;
		public readonly List<TestItem> XunitTests;
		public readonly List<TestItem> ApkTests;
		public readonly List<TestItem> AabTests;

		// Make sure DefaultMode is set to one of the instances placed in Modes
		public readonly List<Mode> Modes;
		public readonly List<Policy> Policies;
		public readonly Mode DefaultMode;

		public Tests (Context context)
		{
			string testsDir = Path.Combine (BuildPaths.XamarinAndroidSourceRoot, "bin", $"Test{context.Configuration}");

			// Tests
			NUnitTests = new List<TestItem> {
				new NUnitTestItem (
					testAssemblyPath: Path.Combine (testsDir, "Xamarin.Android.Build.Tests.dll"),
					name: "Xamarin.Android.Build.Tests",
					description: "Xamarin.Android MSBuild unit tests",
					aliases: new List<string> { "XABT", "build" }
				),

				new NUnitTestItem (
					testAssemblyPath: Path.Combine (testsDir, "CodeBehind", "CodeBehindUnitTests.dll"),
					name: "CodeBehindUnitTests",
					description: "Xamarin.Android code-behind unit tests",
					aliases: new List<string> { "cb", "code-behind", "codebehind" }
				),

				new NUnitTestItem (
					testAssemblyPath: Path.Combine (testsDir, "EmbeddedDSOUnitTests.dll"),
					name: "EmbeddedDSOUnitTests",
					description: "Xamarin.Android embedded DSO unit tests",
					aliases: new List<string> { "embedded-dso", "edso" }
				),

				new NUnitTestItem (
					testAssemblyPath: Path.Combine (testsDir, "Xamarin.Android.Build.Tests.dll"),
					name: "Xamarin.Android.Build.Tests",
					description: "Xamarin.Android MSBuild unit tests",
					aliases: new List<string> { "XABT", "build" }
				),

				new NUnitTestItem (
					testAssemblyPath: Path.Combine (testsDir, "Xamarin.Android.MakeBundle-UnitTests.dll"),
					name: "Xamarin.Android.MakeBundle-UnitTests",
					description: "Xamarin.Android MakeBundle unit tests",
					aliases: new List<string> { "mkbundle", "make-bundle" }
				),
			};

			XunitTests = new List<TestItem> {
				// None for now
			};

			ApkTests = new List<TestItem> {
			};

			AabTests = new List<TestItem> {
			};

			// Dispatchers
			if (NUnitTests.Count > 0)
				nunitDispatcher = new NUnitDispatcher (NUnitTests);

			if (XunitTests.Count > 0)
				xunitDispatcher = new XUnitDispatcher (XunitTests);

			if (ApkTests.Count > 0)
				apkDispatcher = new ApkDispatcher (ApkTests);

			if (AabTests.Count > 0)
				aabDispatcher = new AabDispatcher (AabTests);

			// Policies
			serialPolicy = new Policy (
				name: "serial",
				description: "Run all selected test suites serially",
				aliases: new List<string> { "slow", "single", "s" }
			) {
				MaxParallelDispatchers = 1,
				MaxParallelExecutors = 1
			};

			parallelPolicy = new Policy (
				name: "parallel",
				description: "Run all selected test suites in parallel without stressing the host machine too much",
				aliases: new List<string> { "moderate", "m", "conservative" }
			) {
				MaxParallelDispatchers = 2,
				MaxParallelExecutors = 2,
			};

			massivelyParallelPolicy = new Policy (
				name: "massive-parallel",
				description: "Run all selected test suites in as many parallel processes as possible (based on CPU count)",
				aliases: new List<string> { "massive", "heavy", "aggressive", "h" }
			) {
				MaxParallelDispatchers = (uint)Math.Max (2, Environment.ProcessorCount * 0.3),
				MaxParallelExecutors = (uint)Math.Max (4, (Environment.ProcessorCount * 0.7) - 1),
			};

			Policies = new List<Policy> {
				serialPolicy,
				parallelPolicy,
				massivelyParallelPolicy,
			};

			// Modes
			standardMode = new Mode (
				"standard",
				"Standard execution mode with lightly parallel default policy, suitable for everyday testing",
				parallelPolicy,
				new List <Dispatcher> { apkDispatcher, aabDispatcher},
				new List <Dispatcher> { nunitDispatcher, xunitDispatcher},
				new List <string> { "everyday", "light", "l" }
			);

			heavyMode = new Mode (
				"heavy",
				"Heavy execution mode with massively parallel default policy, suitable for running on CI machines",
				parallelPolicy,
				new List <Dispatcher> { apkDispatcher, aabDispatcher},
				new List <Dispatcher> { nunitDispatcher, xunitDispatcher},
				new List <string> { "ci", "h" }
			);

			Modes = new List<Mode> {
				standardMode,
				heavyMode
			};

			DefaultMode = standardMode;
		}
	}
}
