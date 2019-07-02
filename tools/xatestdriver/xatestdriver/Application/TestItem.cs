using System;
using System.Collections.Generic;

namespace Xamarin.Android.Tests.Driver
{
	/// <summary>
	///   Base class for all the test items supported by the driver.
	/// </summary>
	abstract class TestItem : AppObject
	{
		/// <summary>
		///   If <c>true</c>, the tests will always be ran without any other tests executing in parallel.
		///   Important for performance testing tests but also for tests that, as part of the test suite, run
		///   heavily parallel workloads (e.g. Xamarin.Android msbuild tests)
		/// </summary>
		public bool AlwaysRunAlone { get; set; }

		protected TestItem (string name, string description, IList<string> aliases = null)
			: base (name, description, aliases)
		{}
	}
}
