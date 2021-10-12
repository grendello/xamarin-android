// SPDX-License-Identifier: MIT
#if !defined (__XAMARIN_ANDROID_TYPES_HH)
#define __XAMARIN_ANDROID_TYPES_HH

#include <xamarin-app.hh>

#include "type_descriptor.hh"

namespace xamarin::abi::generator
{
	class BinaryTypeMapHeaderDescriptor final : public TypeDescriptor<BinaryTypeMapHeader, 24>
	{
		static constexpr char field_name_magic[] = "magic";
		static constexpr size_t expected_field_offset_magic = 0;

		static constexpr char field_name_version[] = "version";
		static constexpr size_t expected_field_offset_version = 4;

		static constexpr char field_name_entry_count[] = "entry_count";
		static constexpr size_t expected_field_offset_entry_count = 8;

		static constexpr char field_name_java_name_width[] = "java_name_width";
		static constexpr size_t expected_field_offset_java_name_width = 12;

		static constexpr char field_name_managed_name_width[] = "managed_name_width";
		static constexpr size_t expected_field_offset_managed_name_width = 16;

		static constexpr char field_name_assembly_name_length[] = "assembly_name_length";
		static constexpr size_t expected_field_offset_assembly_name_length = 20;

	public:
		explicit BinaryTypeMapHeaderDescriptor ();
		virtual void write_managed (ManagedCodeWriter& output) override final;
	};
}
#endif // __XAMARIN_ANDROID_TYPES_HH
