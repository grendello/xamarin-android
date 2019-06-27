using System.Collections.Generic;

namespace Xamarin.Android.Tests.Driver
{
	/// <summary>
	///   Base class for all the dispatchers which support tests running on Android device/emulator.
	///
	///   <seealso cref="Dispatcher"/>
	/// </summary>
	abstract class AndroidDeviceDispatcher : Dispatcher
	{
		public override bool RunsHostUnitTests => false;

		protected AndroidDeviceDispatcher (List<TestItem> testItems)
			: base (testItems)
		{}
	}
}
