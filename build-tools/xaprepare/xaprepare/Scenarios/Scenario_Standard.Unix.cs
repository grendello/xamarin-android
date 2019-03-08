using System;
using System.Collections.Generic;

namespace Xamarin.Android.Prepare
{
	partial class Scenario_Standard
	{
		partial void AddRequiredPlatformSpecificSteps ()
		{
			if (Context.Instance.WindowsJitAbisEnabled) {
				Log.DebugLine ("Windows JIT ABIs ENABLED, ADDING MinGW dependencies build step");
				Steps.Add (new Step_BuildMingwDependencies ());
			} else {
				Log.DebugLine ("Windows JIT ABis DISABLED, SKIPPING MinGW dependencies build step");
			}
			Steps.Add (new Step_Get_Windows_GAS ());
		}
	}
}
