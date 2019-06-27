using System.Collections.Generic;

namespace Xamarin.Android.Tests.Driver
{
	/// <summary>
	///   <see cref="Dispatcher"/> implementation which runs tests packaged as AAB bundles on Android
	///   device/emulator.
	/// </summary>
	class AabDispatcher : AndroidDeviceDispatcher
	{
		public override string Name            => "AAB";
		public override TestType Type          => TestType.AAB;

		public AabDispatcher (List<TestItem> testItems)
			: base (testItems)
		{}
	}
}
