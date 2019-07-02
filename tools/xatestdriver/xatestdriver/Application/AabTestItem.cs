using System.Collections.Generic;

namespace Xamarin.Android.Tests.Driver
{
	class AabTestItem : DeviceTestItem
	{
		public AabTestItem (string packagePath, string name, string description, IList<string> aliases = null)
			: base (packagePath, name, description, aliases)
		{}
	}
}
