using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Xamarin.Android.Prepare
{
	abstract class StepXamarinAndroidToolchain : StepWithDownloadProgress
	{
		protected abstract string ProductName { get; }
		protected XamarinAndroidToolchainState State { get; }

		protected StepXamarinAndroidToolchain (string description, XamarinAndroidToolchainState state)
			: base (description)
		{
			State = state;
		}

		protected async Task<bool> UnpackToolchain ()
		{
			if (!String.IsNullOrEmpty (State.UnpackedDir) && Directory.Exists (State.UnpackedDir)) {
				return true;
			}
			State.UnpackedDir = String.Empty;

			string tempDir = Path.Combine (Path.GetTempPath (), "xaprepare-xat");
			Utilities.DeleteDirectorySilent (tempDir);
			Utilities.CreateDirectory (tempDir);

			Log.DebugLine ($"Unpacking {ProductName} archive {State.LocalArchivePath} into {tempDir}");
			if (!await Utilities.Unpack (State.LocalArchivePath, tempDir, cleanDestinatioBeforeUnpacking: true)) {
				return false;
			}

			State.UnpackedDir = tempDir;
			return true;
		}

		protected async Task<bool> DownloadToolchain (Context context)
		{
			if (State.ArchiveDownloaded) {
				return true;
			}

			if (await DownloadToolchain (context, State.LocalArchivePath, State.ArchiveUrl)) {
				State.ArchiveDownloaded = true;
				return true;
			}

			return false;
		}

		async Task<bool> DownloadToolchain (Context context, string localPackagePath, Uri url)
		{
			if (Utilities.FileExists (localPackagePath)) {
				Log.StatusLine ($"{ProductName} archive already downloaded");
				return true;
			}

			Log.StatusLine ($"Downloading {ProductName} from ", url.ToString (), tailColor: ConsoleColor.White);
			(bool success, ulong size, HttpStatusCode status) = await Utilities.GetDownloadSizeWithStatus (url);
			if (!success) {
				if (status == HttpStatusCode.NotFound) {
					Log.ErrorLine ($"{ProductName} archive URL not found");
					return false;
				}
				Log.WarningLine ($"Failed to obtain {ProductName} size. HTTP status code: {status} ({(int)status})");
			}

			DownloadStatus downloadStatus = Utilities.SetupDownloadStatus (context, size, context.InteractiveSession);
			Log.StatusLine ($"  {context.Characters.Link} {url}", ConsoleColor.White);
			await Download (context, url, localPackagePath, ProductName, Path.GetFileName (localPackagePath), downloadStatus);

			if (!File.Exists (localPackagePath)) {
				Log.ErrorLine ($"Download of {ProductName} from {url} failed.");
				return false;
			}

			return true;
		}
	}
}
