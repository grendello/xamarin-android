using System;
using System.Collections.Generic;

namespace Xamarin.Android.Tests.Driver
{
	static class Tests
	{
		public static readonly List<TestItem> nunitTests = new List<TestItem> {
			new UnitTestItem ()
		};

		public static readonly List<TestItem> xunitTests = new List<TestItem> {
			new UnitTestItem ()
		};

		public static readonly List<TestItem> apkTests   = new List<TestItem> {
			new DeviceTestItem ()
		};

		public static readonly List<TestItem> aabTests   = new List<TestItem> {
			new DeviceTestItem ()
		};

		static readonly Policy serialPolicy = new Policy (
			name: "serial",
			description: "Run all selected test suites serially",
			aliases: new List<string> { "slow", "single", "s" }
		) {
			MaxParallelDispatchers = 1,
			MaxParallelExecutors = 1
		};

		static readonly Policy parallelPolicy = new Policy (
			name: "parallel",
			description: "Run all selected test suites in parallel without stressing the host machine too much",
			aliases: new List<string> { "moderate", "m", "conservative" }
		) {
			MaxParallelDispatchers = 2,
			MaxParallelExecutors = 2,
		};

		static readonly Policy massiveParallelPolicy = new Policy (
			name: "massive-parallel",
			description: "Run all selected test suites in as many parallel processes as possible (based on CPU count)",
			aliases: new List<string> { "massive", "heavy", "aggressive", "h" }
		) {
			MaxParallelDispatchers = (uint)Math.Max (4, Environment.ProcessorCount * 0.3),
			MaxParallelExecutors = (uint)Math.Max (8, Environment.ProcessorCount * 0.7),
		};

		public static readonly List<Policy> Policies = new List<Policy> {
			serialPolicy,
			parallelPolicy,
			massiveParallelPolicy,
		};

		static readonly NUnitDispatcher nunitDispatcher = new NUnitDispatcher (nunitTests);
		static readonly XUnitDispatcher xunitDispatcher = new XUnitDispatcher (xunitTests);
		static readonly ApkDispatcher apkDispatcher     = new ApkDispatcher (apkTests);
		static readonly AabDispatcher aabDispatcher     = new AabDispatcher (aabTests);

		static readonly Mode standardMode = new Mode (
			"standard",
			"Standard execution mode with lightly parallel default policy, suitable for everyday testing",
			parallelPolicy,
			new List <Dispatcher> { apkDispatcher, aabDispatcher},
			new List <Dispatcher> { nunitDispatcher, xunitDispatcher},
			new List <string> { "everyday", "light", "l" }
		);

		static readonly Mode heavyMode = new Mode (
			"heavy",
			"Heavy execution mode with massively parallel default policy, suitable for running on CI machines",
			parallelPolicy,
			new List <Dispatcher> { apkDispatcher, aabDispatcher},
			new List <Dispatcher> { nunitDispatcher, xunitDispatcher},
			new List <string> { "ci", "h" }
		);

		// Make sure DefaultMode is set to one of the instances placed in Modes
		public static readonly List<Mode> Modes = new List<Mode> {
			standardMode,
			heavyMode
		};

		public static readonly Mode DefaultMode = standardMode;
	}
}
