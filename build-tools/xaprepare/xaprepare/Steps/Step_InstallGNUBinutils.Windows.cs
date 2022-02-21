namespace Xamarin.Android.Prepare
{
	partial class Step_InstallGNUBinutils
	{
		const string HostName = "windows";
		static readonly string[]? ExecutableExtensions = WindowsExtensions;

		bool HaveLlvmMingw (string mingwDestinationDirectory)
		{
			return true;
		}

		void InstallLlvmMingw (string sourceDir, string mingwDestinationDirectory)
		{
			// No-op
		}
	}
}
