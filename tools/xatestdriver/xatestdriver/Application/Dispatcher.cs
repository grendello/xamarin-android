using System;
using System.Collections.Generic;
using System.Linq;

namespace Xamarin.Android.Tests.Driver
{
	/// <summary>
	///   Base class for all test dispatcher classes.
	/// </summary>
	abstract class Dispatcher
	{
		/// <summary>
		///   User visible name of the dispatcher.
		/// </summary>
		public abstract string Name { get; }

		/// <summary>
		///   <c>true</c> if this particular <see cref="Dispatcher"/> implementation runs unit tests on the host
		///   system, as opposed to test on an Android device/emulator.
		/// </summary>
		public abstract bool RunsHostUnitTests { get; }

		/// <summary>
		///   Type of tests this instance of <see cref="Dispatcher"/> runs. <seealso cref="TestType"/>
		/// </summary>
		public abstract TestType Type { get; }

		/// <summary>
		///   A non-empty list of test items this dispatcher will run
		/// </summary>
		protected List<TestItem> TestItems { get; }

		/// <summary>
		///   Create instance of <see cref="Dispatcher"/> passing it a list of <see cref="TestItem"/> objects to
		///   be dispatched by this instance to appropriate executors. The list must not be <c>null</c> and it
		///   must contain at least a single valid <see cref="TestItem"/> object.
		/// </summary>
		protected Dispatcher (List<TestItem> testItems)
		{
			if (testItems == null)
				throw new ArgumentNullException (nameof (testItems));
			if (testItems.Count (ti => ti != null) == 0)
				throw new ArgumentException ("must contain at least a single valid TestItem", nameof (testItems));
		}
	}
}
