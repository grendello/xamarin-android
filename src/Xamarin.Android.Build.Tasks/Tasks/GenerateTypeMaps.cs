using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using Microsoft.Build.Framework;

using Java.Interop.Tools.Cecil;
using Java.Interop.Tools.Diagnostics;
using Xamarin.Android.Tools;

namespace Xamarin.Android.Tasks
{
    public class GenerateTypeMaps : AndroidTask
	{
		public override string TaskPrefix => "GTM";

		[Required]
		public ITaskItem[] ResolvedAssemblies { get; set; }

		[Required]
		public ITaskItem[] ResolvedUserAssemblies { get; set; }

		[Required]
		public ITaskItem [] FrameworkDirectories { get; set; }

		[Required]
		public string[] SupportedAbis { get; set; }

		[Required]
		public string OutputDirectory { get; set; }

		[Required]
		public bool GenerateNativeAssembly { get; set; }

		[Required]
		public bool EmbedAssemblies { get; set; }

		public bool ErrorOnCustomJavaObject { get; set; }

		public override bool RunTask ()
		{
			try {
//				GenerateNativeAssembly = false; // TEMPORARY, FOR TESTING
				Run ();
			} catch (XamarinAndroidException e) {
				Log.LogCodedError (string.Format ("XA{0:0000}", e.Code), e.MessageWithoutCode);
				if (MonoAndroidHelper.LogInternalExceptions)
					Log.LogMessage (e.ToString ());
			}

			if (Log.HasLoggedErrors) {
				// Ensure that on a rebuild, we don't *skip* the `_GenerateJavaStubs` target,
				// by ensuring that the target outputs have been deleted.
				// TODO: remove all .jm and .mj files
				Files.DeleteFile (Path.Combine (OutputDirectory, "typemap.jm"), Log);
				Files.DeleteFile (Path.Combine (OutputDirectory, "typemap.mj"), Log);
			}

			return !Log.HasLoggedErrors;
		}

		void Run ()
		{
			var assemblyPaths = new List<string> ();
			var interestingAssemblies = new HashSet<string> (StringComparer.OrdinalIgnoreCase);

			foreach (ITaskItem assembly in ResolvedAssemblies) {
				assemblyPaths.Add (assembly.ItemSpec);
				if (String.Compare ("MonoAndroid", assembly.GetMetadata ("TargetFrameworkIdentifier"), StringComparison.Ordinal) != 0)
					continue;
				if (interestingAssemblies.Contains (assembly.ItemSpec))
					continue;
				interestingAssemblies.Add (assembly.ItemSpec);
			}

			var res = new PathAssemblyResolver (assemblyPaths);
			var tmg = new TypeMapGenerator ((string message) => Log.LogDebugMessage (message), SupportedAbis, EmbedAssemblies);
			if (!tmg.Generate (res, interestingAssemblies, OutputDirectory, GenerateNativeAssembly))
				throw new XamarinAndroidException (99999, "Failed to generate type maps");
		}
	}
}
