using System;
using System.Collections.Generic;

namespace Xamarin.Android.Tests.Driver
{
	/// <summary>
	///   Base class for all the test items supported by the driver.
	/// </summary>
	abstract class TestItem : AppObject
	{
		protected TestItem (string name, string description, IList<string> aliases = null)
			: base (name, description, aliases)
		{}
	}
}
