using System;
using System.IO;

using Xamarin.Android.Shared;

namespace Xamarin.Android.Tests.Driver
{
	class Context
	{
		const string LogFilePrefix = "tests";

		bool canOutputColor;
		bool canConsoleUseUnicode;
		bool? useColor;
		bool? dullMode;
		string mainLogFilePath;
		string configuration;
		string logDirectory;
		string overridenLogDirectory;
		Characters characters;

		/// <summary>
		///   Access the only instance of the Context class
		/// </summary>
		public static Context Instance { get; }

		/// <summary>
		///   A set of various special characters used in progress messages. See <see cref="t:Characters" />
		/// </summary>
		public Characters Characters => characters;

		/// <summary>
		///   Do not use emoji characters
		/// </summary>
		public bool NoEmoji { get; set; }

		/// <summary>
		///   If <c>true</c>, the current console is capable of displayig UTF-8 characters
		/// </summary>
		public bool CanConsoleUseUnicode => canConsoleUseUnicode;

		/// <summary>
		///   Current session build configuration
		/// </summary>
		public string Configuration {
			get => configuration ?? Properties.GetRequiredValue (KnownProperties.Configuration);
			set {
				if (String.IsNullOrEmpty (value))
					throw new ArgumentException ("must not be null or empty", nameof (value));
				if (!String.IsNullOrEmpty (configuration))
					throw new InvalidOperationException ("Configuration can be set only once");

				logDirectory = null;
				configuration = value;
			}
		}

		/// <summary>
		///   Set of properties available in this instance of the bootstrapper. See <see cref="KnownProperties" /> and <see
		///   cref="Properties" />
		/// </summary>
		public Properties Properties { get; } = new Properties ();

		/// <summary>
		///   Time stamp of the current run
		/// </summary>
		public string BuildTimeStamp => SharedContext.BuildTimeStamp;

		/// <summary>
		///   Whether the current run of the test driver can interact with the user or not
		/// </summary>
		public bool InteractiveSession { get; }

		/// <summary>
		///   Whether or not log messages should use color
		/// </summary>
		public bool UseColor {
			get => canOutputColor && (!useColor.HasValue || useColor.Value);
			set => useColor = value;
		}

		/// <summary>
		///   Current session execution mode. See <see cref="t:ExecutionMode" />
		/// </summary>
		public ExecutionMode ExecutionMode { get; set; } = ExecutionMode.Standard;

		/// <summary>
		///   If <c>true</c> make messages logged to console not use colors, do not use "fancy" progress indicators etc
		/// </summary>
		public bool DullMode {
			get => dullMode.HasValue ? dullMode.Value : ExecutionMode == ExecutionMode.CI;
			set => dullMode = value;
		}

		/// <summary>
		///   Logging verbosity/level of the current session.
		/// </summary>
		public LoggingVerbosity LoggingVerbosity { get; set; } = LoggingVerbosity.Normal;

		/// <summary>
		///   Directory containing all the session logs
		/// </summary>
		public string LogDirectory {
			get => GetLogDirectory ();
			set {
				if (String.IsNullOrEmpty (value))
					throw new ArgumentException ("must not be null or empty", nameof (value));

				overridenLogDirectory = value;
			}
		}

		static Context ()
		{
			Instance = new Context ();
		}

		Context ()
		{
			Helpers.ConfigureConsole ();
			canOutputColor = Helpers.ConsoleSupportsColor ();
			canConsoleUseUnicode = Helpers.ConsoleSupportsUnicode ();
			InteractiveSession = Helpers.IsInteractiveSession ();

			mainLogFilePath = GetLogFilePath (tags: null, mainLogFile: true);
			Log.Instance.SetLogFile (mainLogFilePath);
		}

		public bool Init ()
		{
			characters = Characters.Create (NoEmoji, DullMode, CanConsoleUseUnicode);

			return true;
		}

		/// <summary>
		///   Print a "banner" to the output stream - will not show anything only if logging verbosity is set to <see
		///   cref="LoggingVerbosity.Quiet"/>
		/// </summary>
		public void Banner (string text)
		{
			SharedContext.Banner (LoggingVerbosity, text);
		}

		/// <summary>
                ///   Construct and return path to a log file other than the main log file. The <paramref name="tags"/> parameter
                ///   is a string appended to the log name - it MUST consist only of characters valid for file/path names.
                /// </summary>
                public string GetLogFilePath (string tags)
                {
                        return GetLogFilePath (tags, false);
                }

                string GetLogFilePath (string tags, bool mainLogFile)
                {
                        string logFileName;
                        if (String.IsNullOrEmpty (tags)) {
                                if (!mainLogFile)
                                        throw new ArgumentException ("must not be null or empty", nameof (tags));
                                logFileName = $"{LogFilePrefix}-{BuildTimeStamp}.log";
                        } else {
                                logFileName = $"{LogFilePrefix}-{BuildTimeStamp}.{tags}.log";
                        }

                        return Path.Combine (LogDirectory, logFileName);
                }

		string GetLogDirectory ()
		{
			if (!String.IsNullOrEmpty (overridenLogDirectory))
				return overridenLogDirectory;

			if (!String.IsNullOrEmpty (logDirectory))
				return logDirectory;

			logDirectory = Path.Combine (BuildPaths.XamarinAndroidSourceRoot, "bin", $"Test{Configuration}");
			if (!Directory.Exists (logDirectory))
				Directory.CreateDirectory (logDirectory);

			return logDirectory;
		}
	}
}
