using System.Collections.Generic;

namespace Xamarin.Android.Tests.Driver
{
	class Tests
	{
		static readonly List<TestItem> nunitTests = new List<TestItem> {
		};

		static readonly List<TestItem> xunitTests = new List<TestItem> {
		};

		static readonly List<TestItem> apkTests   = new List<TestItem> {
		};

		static readonly List<TestItem> aabTests   = new List<TestItem> {
		};

		static readonly NUnitDispatcher nunitDispatcher = new NUnitDispatcher (nunitTests);
		static readonly XUnitDispatcher xunitDispatcher = new XUnitDispatcher (xunitTests);
		static readonly ApkDispatcher apkDispatcher     = new ApkDispatcher (apkTests);
		static readonly AabDispatcher aabDispatcher     = new AabDispatcher (aabTests);

		readonly List<Dispatcher> allUnitTestDispatchers = new List<Dispatcher> {
			nunitDispatcher,
			xunitDispatcher,
		};

		readonly List<Dispatcher> allDeviceTestDispatchers = new List<Dispatcher> {
			apkDispatcher,
			aabDispatcher,
		};
	}
}
