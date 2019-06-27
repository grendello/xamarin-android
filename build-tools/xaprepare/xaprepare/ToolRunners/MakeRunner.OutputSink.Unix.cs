using System;

using Xamarin.Android.Shared;

namespace Xamarin.Android.Prepare
{
	partial class MakeRunner
	{
		class OutputSink : ToolRunner.ToolOutputSink
		{
			public OutputSink (Log log, string logFilePath)
				: base (log, logFilePath)
			{}

			public override void WriteLine (string value)
			{
				base.WriteLine (value);
			}
		}
	}
}
