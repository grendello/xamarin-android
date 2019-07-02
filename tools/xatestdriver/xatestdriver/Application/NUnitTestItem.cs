using System;
using System.Collections.Generic;

namespace Xamarin.Android.Tests.Driver
{
	class NUnitTestItem : UnitTestItem
	{
		public NUnitTestItem (string testAssemblyPath, string name, string description, IList<string> aliases = null)
			: base (testAssemblyPath, name, description, aliases)
		{}
	}
}
