using System;
using System.Collections.Generic;
using System.Linq;

namespace Xamarin.Android.Tasks
{
	class NativeTypeMappingData
	{
		public SortedDictionary<byte[], TypeMapGenerator.ModuleData> Modules { get; }
		public SortedDictionary<string, string> AssemblyNames { get; }
		public SortedDictionary<string, TypeMapGenerator.TypeMapEntry> JavaTypes { get; }

		public uint MapModuleCount { get; }
		public uint JavaTypeCount  { get; }
		public uint JavaNameWidth  { get; }

		public NativeTypeMappingData (SortedDictionary<byte[], TypeMapGenerator.ModuleData> modules, int javaTypeCount, int javaNameWidth)
		{
			Modules = modules ?? throw new ArgumentNullException (nameof (modules));

			MapModuleCount = (uint)modules.Count;
			JavaTypeCount = (uint)javaTypeCount;
			JavaNameWidth = (uint)javaNameWidth;

			AssemblyNames = new SortedDictionary<string, string> (StringComparer.Ordinal);
			JavaTypes = new SortedDictionary<string, TypeMapGenerator.TypeMapEntry> (StringComparer.Ordinal);

			List<TypeMapGenerator.ModuleData> moduleList = modules.Values.ToList ();
			int managedStringCounter = 0;
			foreach (var kvp in modules) {
				TypeMapGenerator.ModuleData data = kvp.Value;
				data.AssemblyNameLabel = $"map_aname.{managedStringCounter++}";
				data.AssemblyName = data.Assembly.GetName ().Name;
				AssemblyNames.Add (data.AssemblyNameLabel, data.AssemblyName);

				int moduleIndex = moduleList.IndexOf (data);
				foreach (TypeMapGenerator.TypeMapEntry entry in data.Types) {
					entry.ModuleIndex = moduleIndex;
					JavaTypes[entry.JavaName] = entry;
				}
			}
		}
	}
}
