using System;
using System.IO;
using System.Threading.Tasks;

namespace Xamarin.Android.Prepare
{
	partial class Step_InstallLlvmMingw : StepXamarinAndroidToolchain
	{
		const string MingwVersionFileName = "version.txt";
		static readonly char[] BinutilsVersionSplitChars = { '-' };

		// Paths for a VERY cursory check, hopefully it will be enough in most cases
		static readonly string[] mingwFiles = {
			"bin/aarch64-w64-mingw32-clang",
			"bin/aarch64-w64-mingw32-clang++",
			"bin/aarch64-w64-mingw32-gcc",
			"bin/aarch64-w64-mingw32-g++",
			"bin/aarch64-w64-mingw32-ld",
			"bin/clang",
			"bin/clang++",
			"bin/x86_64-w64-mingw32-clang",
			"bin/x86_64-w64-mingw32-clang++",
			"bin/x86_64-w64-mingw32-gcc",
			"bin/x86_64-w64-mingw32-g++",
			"bin/x86_64-w64-mingw32-ld",
		};

		static readonly string[] mingwDirs = {
			"aarch64-w64-mingw32/bin",
			"generic-w64-mingw32/include",
			"x86_64-w64-mingw32/bin",
		};

		protected override string ProductName => $"LLVM MinGW";

		public Step_InstallLlvmMingw (XamarinAndroidToolchainState state)
			: base ("Install LLVM MinGW toolchain", state)
		{}

        protected override async Task<bool> Execute (Context context)
        {
			string mingwDestinationDirectory = Configurables.Paths.HostLlvmMingwInstallDir;
			if (HaveLlvmMingw (mingwDestinationDirectory)) {
				Log.StatusLine ("LLVM MinGW is already installed");
			}

			if (!await DownloadToolchain (context)) {
				return false;
			}

			if (!await UnpackToolchain ()) {
				return false;
			}

			InstallLlvmMingw (State.UnpackedDir, mingwDestinationDirectory);
			return true;
		}

		bool HaveLlvmMingw (string mingwDestinationDirectory)
		{
			Log.DebugLine ($"Checking if LLVM MinGW is installed in {mingwDestinationDirectory}");
			string versionFilePath = Path.Combine (mingwDestinationDirectory, MingwVersionFileName);
			if (!File.Exists (versionFilePath)) {
				return false;
			}

			string llvmVersion = String.Empty;
			string mingwVersion = String.Empty;
			string xatVersion = String.Empty;

			string[] lines = File.ReadAllLines (versionFilePath);
			foreach (string l in lines) {
				if (llvmVersion.Length > 0 && mingwVersion.Length > 0 && xatVersion.Length > 0) {
					break;
				}

				string line = l.Trim ();
				if (llvmVersion.Length == 0 && line.StartsWith ("L:", StringComparison.Ordinal)) {
					llvmVersion = line.Substring (2);
					continue;
				}

				if (mingwVersion.Length == 0 && line.StartsWith ("M:", StringComparison.Ordinal)) {
					mingwVersion = line.Substring (2);
					continue;
				}

				if (xatVersion.Length == 0 && line.StartsWith ("X:", StringComparison.Ordinal)) {
					xatVersion = line.Substring (2);
					continue;
				}
			}

			bool haveAllVersions = true;
			VerifyVersion (llvmVersion, "LLVM");
			VerifyVersion (mingwVersion, "MinGW");
			VerifyVersion (xatVersion, "Xamarin.Android Toolchain");

			if (!haveAllVersions) {
				return false;
			}

			string wantedLlvmVersion = String.Empty;
			string wantedMingwVersion = String.Empty;
			string wantedXatVersion = String.Empty;

			string[] versions = Configurables.BinutilsVersion.Split (BinutilsVersionSplitChars, 4);
			if (versions.Length != 4) {
				Log.DebugLine ($"Unexpected BinutilsVersion format. Expected 4 parts separated with '-', got {versions.Length} instead");
				return false;
			}

			foreach (string v in versions) {
				if (wantedLlvmVersion.Length > 0 && wantedMingwVersion.Length > 0) {
					break;
				}

				if (v.StartsWith ("L1_", StringComparison.Ordinal)) {
					// Ignore version of LLVM used to build binutils we ship with Xamarin.Android
					continue;
				}

				if (wantedLlvmVersion.Length == 0 && v.StartsWith ("L2_", StringComparison.Ordinal)) {
					wantedLlvmVersion = v.Substring (3);
					continue;
				}

				if (wantedMingwVersion.Length == 0 && v.StartsWith ("M_", StringComparison.Ordinal)) {
					wantedMingwVersion = v.Substring (2);
					continue;
				}
			}
			wantedXatVersion = versions[3];

			// Versions must be identical, we don't care about newer/older
			if (String.Compare (wantedLlvmVersion, llvmVersion, StringComparison.Ordinal) != 0 ||
				String.Compare (wantedMingwVersion, mingwVersion, StringComparison.Ordinal) != 0 ||
				String.Compare (wantedXatVersion, xatVersion, StringComparison.Ordinal) != 0) {
				Log.DebugLine ("llvm-mingw versions aren't what we want:");
				Log.DebugLine ($"Have LLVM {llvmVersion}, want {wantedLlvmVersion}");
				Log.DebugLine ($"Have MinGW {mingwVersion}, want {wantedMingwVersion}");
				Log.DebugLine ($"Have Xamarin.Android Toolchain {xatVersion}, want {wantedXatVersion}");
				return false;
			}

			bool haveAllFilesAndDirs = true;
			foreach (string file in mingwFiles) {
				string filePath = Path.Combine (mingwDestinationDirectory, file);

				// We don't care if they're symlinks or not. Symlinks are followed and we effectively check whether the
				// both the symlink and the destination exist.
				if (File.Exists (filePath)) {
					continue;
				}

				Log.DebugLine ($"LLVM MinGW file '{filePath}' is missing");
				haveAllFilesAndDirs = false;
			}

			foreach (string dir in mingwDirs) {
				string dirPath = Path.Combine (mingwDestinationDirectory, dir);

				if (Directory.Exists (dirPath)) {
					continue;
				}

				Log.DebugLine ($"LLVM MinGW directory '{dirPath}' is missing");
				haveAllFilesAndDirs = false;
			}

			return haveAllFilesAndDirs;

			void VerifyVersion (string ver, string name)
			{
				if (ver.Length > 0) {
					return;
				}

				Log.DebugLine ($"Unable to determine {name} version from file '{versionFilePath}'");
				haveAllVersions = false;
			}
		}

		void InstallLlvmMingw (string sourceDir, string mingwDestinationDirectory)
		{
			Log.StatusLine ();
			Log.StatusLine ($"Installing LLVM MinGW for {Context.Instance.OS.Name}");

			string topDir = Path.Combine (sourceDir, "llvm-mingw");
			string hostSourceDir = Path.Combine (topDir, HostName);
			if (!Directory.Exists (hostSourceDir)) {
				throw new InvalidOperationException ($"LLVM MinGW source directory '{hostSourceDir}' does not exist");
			}

			string versionFileSourcePath = Path.Combine (topDir, MingwVersionFileName);
			if (!File.Exists (versionFileSourcePath)) {
				throw new InvalidOperationException ($"LLVM MinGW version file '{versionFileSourcePath}' does not exist");
			}

			Utilities.DeleteDirectory (mingwDestinationDirectory);
			Utilities.CreateDirectory (mingwDestinationDirectory);
			CopyRecursively (hostSourceDir.Length + 1, hostSourceDir, mingwDestinationDirectory);
			Utilities.CopyFileToDir (versionFileSourcePath, mingwDestinationDirectory);
		}

		void CopyRecursively (int dirPrefixLength, string sourceDir, string destinationDir)
		{
			foreach (string sourcePath in Directory.EnumerateFileSystemEntries (sourceDir)) {
				bool specialFile = Utilities.IsSpecialFile (sourcePath);

				// Symlinks to directories should not be recursed into
				if (!specialFile && Directory.Exists (sourcePath)) {
					CopyRecursively (dirPrefixLength, sourcePath, destinationDir);
					continue;
				}

				string destPath = Path.Combine (destinationDir, sourcePath.Substring (dirPrefixLength));

				// Make sure symlinks are preserved (handleSpecialFiles == true)
				Utilities.CopyFile (sourcePath, destPath, handleSpecialFiles: true);
			}
		}
	}
}
