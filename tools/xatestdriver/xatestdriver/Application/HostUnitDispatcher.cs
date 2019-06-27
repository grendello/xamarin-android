using System.Collections.Generic;

namespace Xamarin.Android.Tests.Driver
{
	/// <summary>
	///   Base class for all the dispatchers which support unit tests running on the host machine.
	///
	///   <seealso cref="Dispatcher"/>
	/// </summary>
	abstract class HostUnitDispatcher : Dispatcher
	{
		public override bool RunsHostUnitTests => true;

		protected HostUnitDispatcher (List<TestItem> testItems)
			: base (testItems)
		{}
	}
}
