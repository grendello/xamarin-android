using System;
using System.Collections.Generic;

namespace Xamarin.Android.Tests.Driver
{
	abstract class DeviceTestItem : TestItem
	{
		/// <summary>
		///   Path to the Android package this test suite uses
		/// </summary>
		public string PackagePath        { get; }

		/// <summary>
		///   If <c>true</c> then the emulator will be restarted before running these tests. This may be
		///   important for performance tests to ensure that no unusual apps (e.g. previous tests that
		///   crashed/failed etc) are running in the emulator.
		/// </summary>
		public bool ForceRestartEmulator { get; set; }

		protected DeviceTestItem (string packagePath, string name, string description, IList<string> aliases = null)
			: base (name, description, aliases)
		{
			packagePath = packagePath?.Trim ();
			if (String.IsNullOrEmpty (packagePath))
				throw new ArgumentException ("must not be null or empty", nameof (packagePath));
			PackagePath = packagePath;
		}
	}
}
