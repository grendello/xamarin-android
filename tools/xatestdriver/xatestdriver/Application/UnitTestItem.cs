using System;
using System.Collections.Generic;

namespace Xamarin.Android.Tests.Driver
{
	abstract class UnitTestItem : TestItem
	{
		public string TestAssemblyPath { get; }

		protected UnitTestItem (string testAssemblyPath, string name, string description, IList<string> aliases = null)
			: base (name, description, aliases)
		{
			testAssemblyPath = testAssemblyPath?.Trim ();
			if (String.IsNullOrEmpty (testAssemblyPath))
				throw new ArgumentException ("must not be null or empty", nameof (testAssemblyPath));
			TestAssemblyPath = testAssemblyPath;
		}
	}
}
