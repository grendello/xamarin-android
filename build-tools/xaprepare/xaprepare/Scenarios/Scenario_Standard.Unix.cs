using System;
using System.Collections.Generic;

namespace Xamarin.Android.Prepare
{
	partial class Scenario_Standard
	{
		partial void AddRequiredOSSpecificSteps (bool beforeBundle, XamarinAndroidToolchainState xatState)
		{
			if (beforeBundle) {
				Steps.Add (new Step_InstallLlvmMingw (xatState));
			}

			AddRequiredMacOSSteps (beforeBundle);

			if (Context.Instance.WindowsJitAbisEnabled) {
				if (!beforeBundle) {
					// We must run this step after binutils are installed, since they contain the required mingw toolchain
					Log.DebugLine ("Windows JIT ABIs ENABLED, ADDING MinGW dependencies build step (BEFORE bundle)");
					Steps.Add (new Step_BuildMingwDependencies ());
				}
			} else {
				Log.DebugLine ("Windows JIT ABis DISABLED, SKIPPING MinGW dependencies build step");
			}
		}

		partial void AddRequiredMacOSSteps (bool beforeBundle);
	}
}
