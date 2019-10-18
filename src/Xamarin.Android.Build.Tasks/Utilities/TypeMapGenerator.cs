using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Xamarin.Android.Tasks
{
	partial class TypeMapGenerator
	{
		const string TypeMapMagicString = "XATM"; // Xamarin Android Type Map
		const uint TypeMapFormatVersion = 1;

		sealed class UUIDByteArrayComparer : IComparer<byte[]>
		{
			public int Compare (byte[] left, byte[] right)
			{
				int ret;

				for (int i = 0; i < 16; i++) {
					ret = left[i].CompareTo (right[i]);
					if (ret != 0)
						return ret;
				}

				return 0;
			}
		}

		internal sealed class TypeMapEntry
		{
			public string JavaName;
			public string ManagedTypeName;
			public int Token;
			public int AssemblyNameIndex = -1;
			public int ModuleIndex = -1;
		}

		internal sealed class ModuleData
		{
			public Guid Mvid;
			public Assembly Assembly;
			public List<TypeMapEntry> Types;
			public string AssemblyName;
			public string AssemblyNameLabel;
		}

		Action<string> logger;
		Encoding binaryEncoding;
		byte[] magicString;
		string[] supportedAbis;
		bool embedAssemblies;

		public TypeMapGenerator (Action<string> logger, string[] supportedAbis, bool embedAssemblies)
		{
			this.logger = logger ?? throw new ArgumentNullException (nameof (logger));
			if (supportedAbis == null)
				throw new ArgumentNullException (nameof (supportedAbis));
			if (supportedAbis.Length == 0)
				throw new ArgumentException ("must not be empty", nameof (supportedAbis));
			this.supportedAbis = supportedAbis;
			this.embedAssemblies = embedAssemblies;

			binaryEncoding = new UTF8Encoding (false);
			magicString = binaryEncoding.GetBytes (TypeMapMagicString);
		}

		public bool Generate (MetadataAssemblyResolver resolver, IEnumerable<string> assemblies, string outputDirectory, bool generateNativeAssembly)
		{
			if (assemblies == null)
				throw new ArgumentNullException (nameof (assemblies));
			if (String.IsNullOrEmpty (outputDirectory))
				throw new ArgumentException ("must not be null or empty", nameof (outputDirectory));

			if (!Directory.Exists (outputDirectory))
				Directory.CreateDirectory (outputDirectory);

			var scanner = new TMGJavaTypeScanner (logger) {
				ErrorOnCustomJavaObject = false
			};

			List<TMGJavaType> javaTypes = scanner.GetJavaTypes (assemblies, resolver);
			if (scanner.HasErrors)
				return false;

			int assemblyId = 0;
			int maxJavaNameLength = 0;
			var knownAssemblies = new Dictionary<string, int> (StringComparer.Ordinal);
			var modules = new SortedDictionary<byte[], ModuleData> (new UUIDByteArrayComparer ());

			foreach (TMGJavaType td in javaTypes) {
				string assemblyName = td.Module.Assembly.FullName;

				if (!knownAssemblies.ContainsKey (assemblyName)) {
					assemblyId++;
					knownAssemblies.Add (assemblyName, assemblyId);
				}

				byte[] moduleUUID = td.Module.ModuleVersionId.ToByteArray ();
				ModuleData moduleData;
				if (!modules.TryGetValue (moduleUUID, out moduleData)) {
					moduleData = new ModuleData {
						Mvid = td.Module.ModuleVersionId,
						Assembly = td.Type.Assembly,
						Types = new List<TypeMapEntry> ()
					};
					modules.Add (moduleUUID, moduleData);
				}

				// TODO: see why we get
				//
				//    android/views/View
				//
				// instead of the correct
				//
				//    android/view/View
				//
				// attrributes not read correctly?
				string javaName = Java.Interop.Tools.TypeNameMappings.JavaNativeTypeManager.ToJniName (td.Type);
				if (generateNativeAssembly) {
					if (javaName.Length > maxJavaNameLength)
						maxJavaNameLength = javaName.Length;
				}

				moduleData.Types.Add (
					new TypeMapEntry {
						JavaName = javaName,
						ManagedTypeName = td.Type.FullName,
						Token = td.Type.MetadataToken,
						AssemblyNameIndex = knownAssemblies [assemblyName]
					}
				);
			}

			if (!generateNativeAssembly) {
				var moduleCounter = new Dictionary <Assembly, int> ();
				foreach (var kvp in modules) {
					OutputModule (outputDirectory, kvp.Key, kvp.Value, moduleCounter);
				}

				return true;
			}

			NativeAssemblerTargetProvider asmTargetProvider;
			bool sharedBitsWritten = false;
			var data = new NativeTypeMappingData (modules, javaTypes.Count, maxJavaNameLength + 1);
			foreach (string abi in supportedAbis) {
				switch (abi.Trim ()) {
					case "armeabi-v7a":
						asmTargetProvider = new ARMNativeAssemblerTargetProvider (is64Bit: false);
						break;

					case "arm64-v8a":
						asmTargetProvider = new ARMNativeAssemblerTargetProvider (is64Bit: true);
						break;

					case "x86":
						asmTargetProvider = new X86NativeAssemblerTargetProvider (is64Bit: false);
						break;

					case "x86_64":
						asmTargetProvider = new X86NativeAssemblerTargetProvider (is64Bit: true);
						break;

					default:
						throw new InvalidOperationException ($"Unknown ABI {abi}");
				}

				var generator = new TypeMappingNativeAssemblyGenerator2 (asmTargetProvider, data, Path.Combine (outputDirectory, "typemaps"), sharedBitsWritten);

				using (var fs = File.OpenWrite (generator.MainSourceFile)) {
					using (var sw = new StreamWriter (fs, new UTF8Encoding (false), bufferSize: 8192, leaveOpen: false)) {
						generator.Write (sw);
						sw.Flush ();
						sharedBitsWritten = true;
					}
				}
			}
			return true;
		}

		void OutputModule (string outputDirectory, byte[] moduleUUID, ModuleData moduleData, Dictionary <Assembly, int> moduleCounter)
		{
			if (moduleData.Types.Count == 0)
				return;

			int moduleNum;
			if (!moduleCounter.TryGetValue (moduleData.Assembly, out moduleNum)) {
				moduleNum = 0;
				moduleCounter [moduleData.Assembly] = 0;
			} else {
				moduleNum++;
				moduleCounter [moduleData.Assembly] = moduleNum;
			}

			string outputFile = Path.Combine (outputDirectory, $"{moduleData.Assembly.GetName ().Name}.{moduleNum}.typemap");

			using (var fs = File.Open (outputFile, FileMode.Create)) {
				using (var bw = new BinaryWriter (fs)) {
					OutputModule (bw, moduleUUID, moduleData);
					bw.Flush ();
				}
			}
		}

		// Binary file format, all data is little-endian:
		//
		//  [Magic string]           # XATM
		//  [Format version]         # 32-bit integer, 4 bytes
		//  [Module UUID]            # 16 bytes
		//  [Entry count]            # 32-bit integer, 4 bytes
		//  [Java type name width]   # 32-bit integer, 4 bytes
		//  [Assembly name size]     # 32-bit integer, 4 bytes
		//  [Assembly name]          # Non-null terminated assembly name
		//  [Java-to-managed map]    # Format described below, [Entry count] entries
		//  [Managed-to-java map]    # Format described below, [Entry count] entries
		//
		// Java-to-managed map format:
		//
		//    [Java type name]<NUL>[Managed type token ID]
		//
		//  Each name is padded with <NUL> to the width specified in the [Java type name width] field above.
		//  Names are written without the size prefix, instead they are always terminated with a nul character
		//  to make it easier and faster to handle by the native runtime.
		//
		//  Each token ID is a 32-bit integer, 4 bytes
		//
		//
		// Managed-to-java map format:
		//
		//    [Managed type token ID][Java type name table index]
		//
		//  Both fields are 32-bit integers, to a total of 8 bytes per entry. Index points into the
		//  [Java-to-managed map] table above.
		//
		void OutputModule (BinaryWriter bw, byte[] moduleUUID, ModuleData moduleData)
		{
			bw.Write (magicString);
			bw.Write (TypeMapFormatVersion);
			bw.Write (moduleUUID);

			var javaNames = new SortedDictionary<string, int> (StringComparer.Ordinal);
			var managedTypes = new SortedDictionary<int, int> ();
			var managedTypeNames = new Dictionary<int, string> ();

			int maxJavaNameLength = 0;
			foreach (TypeMapEntry entry in moduleData.Types) {
				javaNames.Add (entry.JavaName, entry.Token);
				if (entry.JavaName.Length > maxJavaNameLength)
					maxJavaNameLength = entry.JavaName.Length;

				managedTypes.Add (entry.Token, -1);
				managedTypeNames.Add (entry.Token, entry.ManagedTypeName);
			}

			var javaNameList = javaNames.Keys.ToList ();
			foreach (TypeMapEntry entry in moduleData.Types) {
				managedTypes [entry.Token] = javaNameList.IndexOf (entry.JavaName);
			}

			bw.Write (javaNames.Count);
			bw.Write (maxJavaNameLength);

			string assemblyName = moduleData.Assembly.GetName ().Name;
			bw.Write (assemblyName.Length);
			bw.Write (binaryEncoding.GetBytes (assemblyName));

			foreach (var kvp in javaNames) {
				bw.Write (binaryEncoding.GetBytes (kvp.Key));
				for (int i = 0; i < maxJavaNameLength - kvp.Key.Length; i++) {
					bw.Write ((byte)0);
				}

				bw.Write ((byte)0);
				bw.Write (kvp.Value);
			}

			foreach (var kvp in managedTypes) {
				bw.Write (kvp.Key);
				bw.Write (kvp.Value);
			}
		}
	}
}
