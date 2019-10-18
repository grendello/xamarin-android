using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Xamarin.Android.Tools;

namespace Xamarin.Android.Tasks
{
	class TypeMappingNativeAssemblyGenerator2 : NativeAssemblyGenerator2
	{
		readonly string baseFileName;
		readonly NativeTypeMappingData mappingData;
		readonly uint fieldAlignBytes;
		readonly bool sharedBitsWritten;

		public TypeMappingNativeAssemblyGenerator2 (NativeAssemblerTargetProvider targetProvider, NativeTypeMappingData mappingData, string baseFileName, bool sharedBitsWritten)
			: base (targetProvider, baseFileName)
		{
			this.mappingData = mappingData ?? throw new ArgumentNullException (nameof (mappingData));

			if (String.IsNullOrEmpty (baseFileName))
				throw new ArgumentException("must not be null or empty", nameof (TypeMappingNativeAssemblyGenerator2.baseFileName));

			this.baseFileName = baseFileName;
			fieldAlignBytes = targetProvider.Is64Bit ? 8u : 4u;
			this.sharedBitsWritten = sharedBitsWritten;
		}

		protected override void WriteFileFooter (StreamWriter output)
		{
			// The hash is written to make sure the assembly file which includes the data one is
			// actually different whenever the data changes. Relying on mapping header values for this
			// purpose would not be enough since the only change to the mapping might be a single-character
			// change in one of the type names and we must be sure the assembly is rebuilt in all cases,
			// thus the hash.
			WriteEndLine (output, $"Data Hash: TODO", indent: false);
			base.WriteFileFooter (output);
		}

		protected override void WriteSymbols (StreamWriter output)
		{
			output.WriteLine ();
			WriteHeaderField (output, "map_module_count", mappingData.MapModuleCount);

			output.WriteLine ();
			WriteHeaderField (output, "java_type_count", mappingData.JavaTypeCount);

			output.WriteLine ();
			WriteHeaderField (output, "java_name_width", mappingData.JavaNameWidth);

			output.WriteLine ();
			output.WriteLine ($"{Indent}.include{Indent}\"{Path.GetFileName (TypemapsIncludeFile)}\"");
			output.WriteLine ();

			if (!sharedBitsWritten) {
				using (var fs = File.Open (TypemapsIncludeFile, FileMode.Create)) {
					using (var mapOutput = new StreamWriter (fs, output.Encoding)) {
						WriteAssemblyNames (mapOutput);
						WriteMapModules (output, mapOutput, "map_modules");
						mapOutput.Flush ();
					}
				}
			} else {
				WriteMapModules (output, null, "map_modules");
			}

			WriteJavaMap (output, "map_java");
		}

		void WriteAssemblyNames (StreamWriter output)
		{
			foreach (var kvp in mappingData.AssemblyNames) {
				string label = kvp.Key;
				string name = kvp.Value;

				WriteData (output, name, label, isGlobal: false);
				output.WriteLine ();
			}
		}

		void WriteManagedMaps (StreamWriter output, string moduleSymbolName, TypeMapGenerator.ModuleData data)
		{
			var javaTypes = mappingData.JavaTypes.Keys.ToList ();
			var tokens = new SortedDictionary<int, int> ();
			foreach (TypeMapGenerator.TypeMapEntry entry in data.Types) {
				tokens[entry.Token] = javaTypes.IndexOf (entry.JavaName);
			}

			WriteSection (output, $".rodata.{moduleSymbolName}", hasStrings: false, writable: false);
			WriteStructureSymbol (output, moduleSymbolName, alignBits: 0, isGlobal: false);

			uint size = 0;
			foreach (var kvp in tokens) {
				int token = kvp.Key;
				int javaMapIndex = kvp.Value;

				size += WriteStructure (output, fieldAlignBytes, () => WriteManagedMapEntry (output, token, javaMapIndex));
			}

			WriteStructureSize (output, moduleSymbolName, size);
			output.WriteLine ();
		}

		uint WriteManagedMapEntry (StreamWriter output, int token, int javaMapIndex)
		{
			uint size = WriteData (output, token);
			size += WriteData (output, javaMapIndex);

			return size;
		}

		void WriteMapModules (StreamWriter output, StreamWriter mapOutput, string symbolName)
		{
			WriteCommentLine (output, "Managed to Java map: START", indent: false);
			WriteSection (output, $".data.rel.{symbolName}", hasStrings: false, writable: true);
			WriteStructureSymbol (output, symbolName, alignBits: TargetProvider.MapModulesAlignBits, isGlobal: true);

			uint size = 0;
			int moduleCounter = 0;
			foreach (var kvp in mappingData.Modules) {
				byte[] mvid = kvp.Key;
				TypeMapGenerator.ModuleData data = kvp.Value;

				string mapName = $"module{moduleCounter++}_managed_to_java";
				size += WriteStructure (output, fieldAlignBytes, () => WriteMapModule (output, mapName, mvid, data));
				if (mapOutput != null)
					WriteManagedMaps (mapOutput, mapName, data);
			}

			WriteStructureSize (output, symbolName, size);
			WriteCommentLine (output, "Managed to Java map: END", indent: false);
			output.WriteLine ();
		}

		uint WriteMapModule (StreamWriter output, string mapName, byte[] mvid, TypeMapGenerator.ModuleData data)
		{
			uint size = 0;
			WriteCommentLine (output, $"module_uuid: {data.Mvid}");
			size += WriteData (output, mvid);

			WriteCommentLine (output, "entry_count");
			size += WriteData (output, data.Types.Count);

			WriteCommentLine (output, "map");
			size += WritePointer (output, mapName);

			WriteCommentLine (output, $"assembly_name: {data.AssemblyName}");
			size += WritePointer (output, MakeLocalLabel (data.AssemblyNameLabel));

			WriteCommentLine (output, "image");
			size += WritePointer (output);

			output.WriteLine ();

			return size;
		}

		void WriteJavaMap (StreamWriter output, string symbolName)
		{
			WriteCommentLine (output, "Java to managed map: START", indent: false);
			WriteSection (output, $".rodata.{symbolName}", hasStrings: false, writable: false);
			WriteStructureSymbol (output, symbolName, alignBits: TargetProvider.MapJavaAlignBits, isGlobal: true);

			uint size = 0;
			int entryCount = 0;
			foreach (var kvp in mappingData.JavaTypes) {
				TypeMapGenerator.TypeMapEntry entry = kvp.Value;
				size += WriteJavaMapEntry (output, entry, entryCount++);
			}

			WriteStructureSize (output, symbolName, size);
			WriteCommentLine (output, "Java to managed map: END", indent: false);
			output.WriteLine ();
		}

		uint WriteJavaMapEntry (StreamWriter output, TypeMapGenerator.TypeMapEntry entry, int entryIndex)
		{
			uint size = 0;

			WriteCommentLine (output, $"#{entryIndex}");
			WriteCommentLine (output, "module_index");
			size += WriteData (output, entry.ModuleIndex);

			WriteCommentLine (output, "type_token_id");
			size += WriteData (output, entry.Token);

			WriteCommentLine (output, "java_name");
			size += WriteAsciiData (output, entry.JavaName, mappingData.JavaNameWidth);

			output.WriteLine ();

			return size;
		}

		void WriteHeaderField (StreamWriter output, string name, uint value)
		{
			WriteCommentLine (output, $"{name}: START", indent: false);
			WriteSection (output, $".rodata.{name}", hasStrings: false, writable: false);
			WriteSymbol (output, name, size: 4, alignBits: 2, isGlobal: true, isObject: true, alwaysWriteSize: true);
			WriteData (output, value);
			WriteCommentLine (output, $"{name}: END", indent: false);
		}
	}

	class TypeMappingNativeAssemblyGenerator : NativeAssemblyGenerator
	{
		NativeAssemblyDataStream dataStream;
		string dataFileName;
		uint dataSize;
		string mappingFieldName;

		public TypeMappingNativeAssemblyGenerator (NativeAssemblerTargetProvider targetProvider, NativeAssemblyDataStream dataStream, string dataFileName, uint dataSize, string mappingFieldName)
			: base (targetProvider)
		{
			if (dataStream == null)
				throw new ArgumentNullException (nameof (dataStream));
			if (String.IsNullOrEmpty (dataFileName))
				throw new ArgumentException ("must not be null or empty", nameof (dataFileName));
			if (String.IsNullOrEmpty (mappingFieldName))
				throw new ArgumentException ("must not be null or empty", nameof (mappingFieldName));

			this.dataStream = dataStream;
			this.dataFileName = dataFileName;
			this.dataSize = dataSize;
			this.mappingFieldName = mappingFieldName;
		}

		public bool EmbedAssemblies { get; set; }

		protected override void WriteFileHeader (StreamWriter output, string outputFileName)
		{
			// The hash is written to make sure the assembly file which includes the data one is
			// actually different whenever the data changes. Relying on mapping header values for this
			// purpose would not be enough since the only change to the mapping might be a single-character
			// change in one of the type names and we must be sure the assembly is rebuilt in all cases,
			// thus the hash.
			WriteEndLine (output, $"Data Hash: {Files.ToHexString (dataStream.GetStreamHash ())}", false);
			base.WriteFileHeader (output, outputFileName);
		}

		protected override void WriteSymbols (StreamWriter output)
		{
			WriteMappingHeader (output, dataStream, mappingFieldName);
			WriteCommentLine (output, "Mapping data");
			WriteSymbol (output, mappingFieldName, dataSize, isGlobal: true, isObject: true, alwaysWriteSize: true);
			if (EmbedAssemblies) {
				output.WriteLine ($"{Indent}.include{Indent}\"{dataFileName}\"");
			}
		}

		void WriteMappingHeader (StreamWriter output, NativeAssemblyDataStream dataStream, string mappingFieldName)
		{
			output.WriteLine ();
			WriteCommentLine (output, "Mapping header");
			WriteSection (output, $".data.{mappingFieldName}", hasStrings: false, writable: true);
			WriteSymbol (output, $"{mappingFieldName}_header", alignBits: 2, fieldAlignBytes: 4, isGlobal: true, alwaysWriteSize: true, structureWriter: () => {
				WriteCommentLine (output, "version");
				WriteData (output, dataStream.MapVersion);

				WriteCommentLine (output, "entry-count");
				WriteData (output, dataStream.MapEntryCount);

				WriteCommentLine (output, "entry-length");
				WriteData (output, dataStream.MapEntryLength);

				WriteCommentLine (output, "value-offset");
				WriteData (output, dataStream.MapValueOffset);
				return 16;
			});
			output.WriteLine ();
		}
	}
}
