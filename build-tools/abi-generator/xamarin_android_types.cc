// SPDX-License-Identifier: MIT
#include "xamarin_android_types.hh"

using namespace xamarin::abi::generator;

BinaryTypeMapHeaderDescriptor::BinaryTypeMapHeaderDescriptor ()
{
	add_field <uint32_t, expected_field_offset_magic, offsetof (BinaryTypeMapHeader, magic), field_name_magic> (&BinaryTypeMapHeader::magic);
	add_field <uint32_t, expected_field_offset_version, offsetof (BinaryTypeMapHeader, version), field_name_version> (&BinaryTypeMapHeader::version);
	add_field <uint32_t, expected_field_offset_entry_count, offsetof (BinaryTypeMapHeader, entry_count), field_name_entry_count> (&BinaryTypeMapHeader::entry_count);
	add_field <uint32_t, expected_field_offset_java_name_width, offsetof (BinaryTypeMapHeader, java_name_width), field_name_java_name_width> (&BinaryTypeMapHeader::java_name_width);
	add_field <uint32_t, expected_field_offset_managed_name_width, offsetof (BinaryTypeMapHeader, managed_name_width), field_name_managed_name_width> (&BinaryTypeMapHeader::managed_name_width);
	add_field <uint32_t, expected_field_offset_assembly_name_length, offsetof (BinaryTypeMapHeader, assembly_name_length), field_name_assembly_name_length> (&BinaryTypeMapHeader::assembly_name_length);

	ensure_all_fields_added ();
}

void BinaryTypeMapHeaderDescriptor::write_managed (ManagedCodeWriter& output)
{
}
