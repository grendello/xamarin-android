using System;
using System.Collections.Generic;

namespace Xamarin.Android.Tests.Driver
{
	abstract class DeviceTestItem : TestItem
	{
		public string PackagePath { get; }

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
