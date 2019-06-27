namespace Xamarin.Android.Tests.Driver
{
	class Context
	{
		/// <summary>
		///   Access the only instance of the Context class
		/// </summary>
		public static Context Instance { get; }

		static Context ()
		{
			Instance = new Context ();
		}

		Context ()
		{
		}
	}
}
