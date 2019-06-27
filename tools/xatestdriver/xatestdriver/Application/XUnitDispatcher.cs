using System.Collections.Generic;

namespace Xamarin.Android.Tests.Driver
{
	/// <summary>
	///   <see cref="Dispatcher"/> implementation which runs xUnit tests on the host machine.
	/// </summary>
	class XUnitDispatcher : HostUnitDispatcher
	{
		public override string Name            => "xUnit";
		public override TestType Type          => TestType.XUnit;

		public XUnitDispatcher (List<TestItem> testItems)
			: base (testItems)
		{}
	}
}
