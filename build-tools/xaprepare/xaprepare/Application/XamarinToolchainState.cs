using System;
using System.IO;

namespace Xamarin.Android.Prepare
{
	// Two different steps use the same toolchain archive. This class serves the purpose of sharing the archive's
	// download and unpack state, so that the preparation process takes less time.
	class XamarinAndroidToolchainState
	{
		public bool ArchiveDownloaded { get; set; }
		public string UnpackedDir { get; set; } = String.Empty;
		public string LocalArchivePath { get; }
		public Uri ArchiveUrl { get; }

		public XamarinAndroidToolchainState ()
		{
			string packageName = Path.GetFileName (Configurables.Urls.BinutilsArchive.LocalPath);
			LocalArchivePath = Path.Combine (Configurables.Paths.BinutilsCacheDir, packageName);
			ArchiveUrl = Configurables.Urls.BinutilsArchive;
		}
	}
}
