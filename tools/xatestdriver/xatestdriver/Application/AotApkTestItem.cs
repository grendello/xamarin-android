using System.Collections.Generic;

namespace Xamarin.Android.Tests.Driver
{
	class AotApkTestItem : DeviceTestItem
	{
		protected AotApkTestItem (string packagePath, string name, string description, IList<string> aliases = null)
			: base (packagePath, name, description, aliases)
		{}
	}
}
