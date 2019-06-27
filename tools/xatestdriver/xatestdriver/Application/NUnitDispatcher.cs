using System.Collections.Generic;

namespace Xamarin.Android.Tests.Driver
{
	/// <summary>
	///   <see cref="Dispatcher"/> implementation which runs NUnit tests on the host machine.
	/// </summary>
	class NUnitDispatcher : HostUnitDispatcher
	{
		public override string Name            => "NUnit";
		public override TestType Type          => TestType.NUnit;

		public NUnitDispatcher (List<TestItem> testItems)
			: base (testItems)
		{}
	}
}
