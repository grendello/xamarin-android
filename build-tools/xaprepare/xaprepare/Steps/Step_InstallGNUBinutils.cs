using System;
using System.IO;
using System.Threading.Tasks;

namespace Xamarin.Android.Prepare
{
	partial class Step_InstallGNUBinutils : StepXamarinAndroidToolchain
	{
		static readonly string[]? WindowsExtensions = {".exe", ".cmd"};

		protected override string ProductName => $"Xamarin.Android Toolchain {Configurables.Defaults.BinutilsVersion}";

		public Step_InstallGNUBinutils (XamarinAndroidToolchainState state)
			: base ("Install Xamarin.Android Toolchain", state)
		{}

		protected override async Task<bool> Execute (Context context)
		{
			string hostDestinationDirectory = Configurables.Paths.HostBinutilsInstallDir;
			string windowsDestinationDirectory = Configurables.Paths.WindowsBinutilsInstallDir;

			bool hostHaveAll = HaveAllBinutils (hostDestinationDirectory);
			bool windowsHaveAll = HaveAllBinutils (windowsDestinationDirectory, WindowsExtensions);

			if (hostHaveAll && windowsHaveAll) {
				Log.StatusLine ("Xamarin.Android binutils toolchain is already installed");
				return true;
			}

			if (!await DownloadToolchain (context)) {
				return false;
			}

			if (!await UnpackToolchain ()) {
				return false;
			}

			if (!hostHaveAll) {
				CopyToDestination (context, "Host", State.UnpackedDir, hostDestinationDirectory, executableExtensions: ExecutableExtensions);
			}

			if (!windowsHaveAll) {
				CopyToDestination (context, "Windows", State.UnpackedDir, windowsDestinationDirectory, "windows", WindowsExtensions);
			}

			return true;
		}

		bool CopyToDestination (Context context, string label, string sourceDir, string destinationDir, string osName = HostName, string[]? executableExtensions = null)
		{
			Log.StatusLine ();
			Log.StatusLine ($"Installing for {label}:");

			string sourcePath = Path.Combine (sourceDir, osName);
			foreach (var kvp in Configurables.Defaults.AndroidToolchainPrefixes) {
				string prefix = kvp.Value;
				CopyTools (prefix);
			}
			CopyTools (String.Empty);

			return true;

			void CopyTools (string prefix)
			{
				bool copyPrefixed = !String.IsNullOrEmpty (prefix);
				foreach (NDKTool tool in Configurables.Defaults.NDKTools) {
					if (tool.Prefixed != copyPrefixed) {
						continue;
					}

					string toolSourcePath = GetToolPath (sourcePath, prefix, tool, executableExtensions, throwOnMissing: true);
					string toolName = Path.GetFileName (toolSourcePath);
					string toolDestinationPath = Path.Combine (destinationDir, toolName);
					string versionMarkerPath = GetVersionMarker (toolDestinationPath);

					Log.StatusLine ($"  {context.Characters.Bullet} Installing ", toolName, tailColor: ConsoleColor.White);
					Utilities.CopyFile (toolSourcePath, toolDestinationPath);
					File.WriteAllText (versionMarkerPath, DateTime.UtcNow.ToString ());
				}
			}
		}

		bool HaveAllBinutils (string dir, string[]? executableExtensions = null)
		{
			Log.DebugLine ($"Checking if all binutils are installed in {dir}");
			foreach (var kvp in Configurables.Defaults.AndroidToolchainPrefixes) {
				string prefix = kvp.Value;
				if (!CheckToolsExist (prefix)) {
					return false;
				}
			}

			return CheckToolsExist (String.Empty);

			bool CheckToolsExist (string prefix)
			{
				bool checkPrefixed = !String.IsNullOrEmpty (prefix);
				foreach (NDKTool tool in Configurables.Defaults.NDKTools) {
					if (tool.Prefixed != checkPrefixed) {
						continue;
					}
					string toolPath = GetToolPath (dir, prefix, tool, executableExtensions);
					string toolName = Path.GetFileName (toolPath);
					string versionMarkerPath = GetVersionMarker (toolPath);

					Log.DebugLine ($"Checking {toolName}");
					if (!Utilities.FileExists (toolPath)) {
						Log.DebugLine ($"Binutils tool {toolPath} does not exist");
						return false;
					}

					if (!Utilities.FileExists (versionMarkerPath)) {
						Log.DebugLine ($"Binutils tool {toolPath} exists, but its version is incorrect");
						return false;
					}
				}

				return true;
			}
		}

		string GetToolPath (string sourcePath, string prefix, NDKTool tool, string[]? executableExtensions = null, bool throwOnMissing = false)
		{
			string baseName = $"{(String.IsNullOrEmpty (tool.DestinationName) ? tool.Name : tool.DestinationName)}";

			if (!String.IsNullOrEmpty (prefix)) {
				baseName = $"{prefix}-{baseName}";
			}

			if (executableExtensions == null || executableExtensions.Length == 0) {
				return Path.Combine (sourcePath, baseName);
			}

			foreach (string executableExtension in executableExtensions) {
				string binary = Path.Combine (sourcePath, $"{baseName}{executableExtension}");
				if (!Utilities.FileExists (binary)) {
					continue;
				}

				return binary;
			}

			if (throwOnMissing) {
				string extensions = String.Join (",", executableExtensions);
				throw new InvalidOperationException ($"Failed to find binary file '{baseName}{{{extensions}}}'");
			}

			return baseName;
		}

		string GetVersionMarker (string toolPath)
		{
			return $"{toolPath}.{Configurables.Defaults.BinutilsVersion}";
		}
	}
}
