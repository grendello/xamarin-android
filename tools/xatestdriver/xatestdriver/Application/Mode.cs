using System;
using System.Collections.Generic;
using System.Linq;

namespace Xamarin.Android.Tests.Driver
{
	/// <summary>
	///   Represents driver's execution mode, defined as a collection of dispatchers and Policies that govern the
	///   way tests are executed. Each mode has a set of dispatchers (which cannot be changed on runtime) and a
	///   default policy (which can be changed at runtime). The idea is that dispatchers are tailored to a specific
	///   host (e.g. a CI server or a local development machine) and the expectations/requirements of that host
	///   which will most likely not change and that's why they are "static" (or compile time) selections. Policies,
	///   however, can be changed because there might be hosts of the same kind but with different resources and so
	///   one CI server might be able to dispatch 10 simultaneous test sessions while another only 2.
	///
	///   <seealso cref="Dispatcher"/>
	///   <seealso cref="Policy"/>
	/// </summary>
	class Mode : AppObject
	{
		/// <summary>
		///   Non-empty list of non-null host device test dispatchers
		/// </summary>
		public IList<Dispatcher> DeviceTestDispatchers { get; }

		/// <summary>
		///   Non-empty list of non-null host unit test dispatchers
		/// </summary>
		public IList<Dispatcher> UnitTestDispatchers   { get; }

		/// <summary>
		///   A non-null policy to be used by this mode.
		/// </summary>
		public Policy Policy                          { get; set; }

		public Mode (string name, string description, Policy defaultPolicy, List<Dispatcher> deviceTestDispatchers, List<Dispatcher> unitTestDispatchers, IList<string> aliases = null)
			: base (name, description, aliases)
		{
			Policy = defaultPolicy ?? throw new ArgumentNullException (nameof (defaultPolicy));
			DeviceTestDispatchers = EnsureValidDispatchers (deviceTestDispatchers, nameof (deviceTestDispatchers));
			UnitTestDispatchers = EnsureValidDispatchers (unitTestDispatchers, nameof (unitTestDispatchers));
		}

		IList<Dispatcher> EnsureValidDispatchers (List<Dispatcher> dispatchers, string argName)
		{
			if (dispatchers == null)
				throw new ArgumentNullException (argName);

			var validDispatchers = dispatchers.Where (d => d != null).ToList ();
			if (validDispatchers.Count == 0)
				throw new ArgumentException ("must contain at least one valid dispatcher", argName);

			return validDispatchers.AsReadOnly ();
		}
	}
}
