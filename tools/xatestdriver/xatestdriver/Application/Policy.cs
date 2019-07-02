using System;
using System.Collections.Generic;
using System.Linq;

namespace Xamarin.Android.Tests.Driver
{
	/// <summary>
	///   Sets the execution policy for all the dispatchers and executors.
	///   <seealso cref="Dispatcher"/>
	///   <seealso cref="Executor"/>
	/// </summary>
	class Policy
	{
		/// <summary>
		///   Number of <see cref="Dispatcher"/> instances that are allowed to run at the same time.
		///   <c>0</c> is treated as equal to <c>1</c>
		/// </summary>
		public uint MaxParallelDispatchers { get; set; }

		/// <summary>
		///   Number of <see cref="Executor"/> instances that are allowed to run within a single <see
		///   cref="Dispatcher"/> at the same time. <c>0</c> is treated as equal to <c>1</c>
		/// </summary>
		public uint MaxParallelExecutors   { get; set; }

		/// <summary>
		///   A short description of the policy.
		/// </summary>
		public string Description          { get; }

		/// <summary>
		///   Short policy name. Should not contain spaces or any characters that may require quoting when used
		///   in the operating system shell. Name will be used on command line to select the execution policy.
		/// </summary>
		public string Name                 { get; }

		/// <summary>
		///   Optional set of <see cref="Name"/> aliases that can be used when selecting policy on the command
		///   line.
		/// </summary>
		public IList<string> Aliases       { get; }

		public Policy (string name, string description, IList<string> aliases = null)
		{
			name = name?.Trim ();
			if (String.IsNullOrEmpty (name))
				throw new ArgumentException ("must not be null or empty", nameof (name));

			description = description?.Trim ();
			if (String.IsNullOrEmpty (description))
				throw new ArgumentException ("must not be null or empty", nameof (description));

			Name = name;
			Description = description;

			if (aliases == null)
				return;

			var nonEmptyAliases = aliases.Where (s => !String.IsNullOrEmpty (s)).Select (s => s.Trim ()).Where (s => !String.IsNullOrEmpty (s)).ToList ();
			if (nonEmptyAliases.Count == 0)
				return;

			Aliases = nonEmptyAliases.AsReadOnly ();
		}
	}
}
