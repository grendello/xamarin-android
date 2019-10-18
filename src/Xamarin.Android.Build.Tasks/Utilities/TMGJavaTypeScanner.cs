using System;
using System.Collections.Generic;
using System.Reflection;

namespace Xamarin.Android.Tasks
{
	class TMGJavaTypeScanner
	{
		sealed class ProbeTypes
		{
			public Type AndroidRuntimeIJavaObject;
			public Type JavaLangObject;
			public Type JavaLangThrowable;
			public Type SystemException;
		}

		Action<string> logger;

		public bool ErrorOnCustomJavaObject { get; set; }
		public bool HasErrors { get; private set; }

		public TMGJavaTypeScanner (Action<string> logger)
		{
			if (logger == null)
				throw new ArgumentNullException (nameof (logger));
			this.logger = logger;
		}

		public List<TMGJavaType> GetJavaTypes (IEnumerable<string> assemblies, MetadataAssemblyResolver resolver)
		{
			var mlc = new MetadataLoadContext (resolver, "mscorlib");
			var javaTypes = new List<TMGJavaType> ();

			Assembly monoAndroid = mlc.LoadFromAssemblyName ("Mono.Android");
			Assembly corlib = mlc.CoreAssembly;

			var pt = new ProbeTypes {
				AndroidRuntimeIJavaObject = monoAndroid.GetType ("Android.Runtime.IJavaObject"),
				JavaLangObject = monoAndroid.GetType ("Java.Lang.Object"),
				JavaLangThrowable = monoAndroid.GetType ("Java.Lang.Throwable"),
				SystemException = corlib.GetType ("System.Exception"),
			};

			foreach (string assemblyPath in assemblies) {
				Assembly asm = mlc.LoadFromAssemblyPath (assemblyPath);
				if (asm == null)
					throw new InvalidOperationException ($"Assembly '{assemblyPath}' cannot be found");

				foreach (Module module in asm.Modules) {
					foreach (Type type in module.GetTypes ()) {
						AddJavaTypes (pt, javaTypes, type, module);
					}
				}
			}

			return javaTypes;
		}

		void AddJavaTypes (ProbeTypes pt, List<TMGJavaType> javaTypes, Type type, Module module)
		{
			if (pt.JavaLangObject.IsAssignableFrom (type) || pt.JavaLangThrowable.IsAssignableFrom (type)) {
				javaTypes.Add (new TMGJavaType { Type = type, Module = module });
			} else if (type.IsClass && !pt.SystemException.IsAssignableFrom (type) && pt.AndroidRuntimeIJavaObject.IsAssignableFrom (type)) {
				string prefix  = ErrorOnCustomJavaObject ? "error" : "warning";
				logger ($"{prefix} XA4212: Type `{type.FullName}` implements `Android.Runtime.IJavaObject` but does not inherit `Java.Lang.Object` or `Java.Lang.Throwable`. This is not supported.");
				HasErrors = ErrorOnCustomJavaObject;
				return;
			}
		}
	}
}
