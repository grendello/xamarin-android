using System;
using System.Collections.Generic;

namespace Xamarin.Android.Tests.Driver
{
	/// <summary>
	///   Sets the execution policy for all the dispatchers and executors.
	///   <seealso cref="Dispatcher"/>
	///   <seealso cref="Executor"/>
	/// </summary>
	class Policy : AppObject
	{
		/// <summary>
		///   Number of <see cref="Dispatcher"/> instances that are allowed to run at the same time.
		///   <c>0</c> is treated as equal to <c>1</c>
		/// </summary>
		public uint MaxParallelDispatchers { get; set; } = 1;

		/// <summary>
		///   Number of <see cref="Executor"/> instances that are allowed to run within a single <see
		///   cref="Dispatcher"/> at the same time. <c>0</c> is treated as equal to <c>1</c>
		/// </summary>
		public uint MaxParallelExecutors   { get; set; } = 1;

		public Policy (string name, string description, IList<string> aliases = null)
			: base (name, description, aliases)
		{}
	}
}
