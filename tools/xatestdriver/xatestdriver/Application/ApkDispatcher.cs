using System.Collections.Generic;

namespace Xamarin.Android.Tests.Driver
{
	/// <summary>
	///   <see cref="Dispatcher"/> implementation which runs tests packaged as APK archives on Android
	///   device/emulator.
	/// </summary>
	class ApkDispatcher : AndroidDeviceDispatcher
	{
		public override string Name            => "APK";
		public override TestType Type          => TestType.APK;

		public ApkDispatcher (List<TestItem> testItems)
			: base (testItems)
		{}
	}
}
