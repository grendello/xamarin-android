using System;
using System.Collections.Generic;
using System.Linq;

namespace Xamarin.Android.Tests.Driver
{
	abstract class AppObject
	{
		/// <summary>
		///   A short description of the object.
		/// </summary>
		public string Description                     { get; }

		/// <summary>
		///   Short mode name. Should not contain spaces or any characters that may require quoting when used
		///   in the operating system shell. Name may be used on command line to select something (e.g. a <see
		///   cref="Policy"/> or <see cref="Mode"/>).
		/// </summary>
		public string Name                            { get; }

		/// <summary>
		///   Optional set of <see cref="Name"/> aliases that can be used when selecting mode on the command
		///   line.
		/// </summary>
		public IList<string> Aliases                  { get; }

		protected AppObject (string name, string description, IList<string> aliases = null)
		{
			name = name?.Trim ();
			if (String.IsNullOrEmpty (name))
				throw new ArgumentException ("must not be null or empty", nameof (name));

			description = description?.Trim ();
			if (String.IsNullOrEmpty (description))
				throw new ArgumentException ("must not be null or empty", nameof (description));

			Name = name;
			Description = description;

			var nonEmptyAliases = aliases.Where (s => !String.IsNullOrEmpty (s)).Select (s => s.Trim ()).Where (s => !String.IsNullOrEmpty (s)).ToList ();
			if (nonEmptyAliases.Count == 0)
				return;

			Aliases = nonEmptyAliases.AsReadOnly ();
		}
	}
}
