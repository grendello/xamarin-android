using System;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Xamarin.Android.Tools.BootstrapTasks
{
	public class GenerateTypeMapGeneratorBCLAssemblyNames : Task
	{
		[Required]
		public ITaskItem[] Files { get; set; }

		[Required]
		public ITaskItem OutputFile { get; set; }

		public override bool Execute ()
		{
			var sb = new StringBuilder ();
			sb.AppendLine ("using System;");
			sb.AppendLine ("using System.Collections.Generic;");
			sb.AppendLine ();
			sb.AppendLine ("namespace Xamarin.Android.Tasks {");
			sb.AppendLine ("\tpartial class TypeMapGenerator {");
			sb.AppendLine ("\t\tstatic readonly HashSet<string> BCLAssemblyNames = new HashSet<string> (StringComparer.OrdinalIgnoreCase) {");
			foreach (var file in Files.Select(x => Path.GetFileNameWithoutExtension (x.ItemSpec)).Distinct().OrderBy(x => x)) {
				sb.AppendFormat ("\t\t\t\"{0}\"," + Environment.NewLine, file);
			}
			sb.AppendLine ("\t\t};");
			sb.AppendLine ("\t}");
			sb.AppendLine ("}");

			File.WriteAllText (OutputFile.ItemSpec, sb.ToString ());

			return !Log.HasLoggedErrors;
		}
	}
}
