#include <stdint.h>
#include <stdlib.h>

#include "xamarin-app.hh"

// This file MUST have "valid" values everywhere - the DSO it is compiled into is loaded by the
// designer on desktop.
const uint32_t map_module_count = 2;
const uint32_t java_type_count = 2;
const uint32_t java_name_width = 32;

static const TypeMapModuleEntry module1_managed_to_java[] = {
	{
		.type_token_id = 0,
		.java_map_index = 0,
	}
};

static const TypeMapModuleEntry module2_managed_to_java[] = {
	{
		.type_token_id = 1,
		.java_map_index = 0,
	}
};

const TypeMapModule map_modules[] = {
	{
		.module_uuid = { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
		.entry_count = 1,
		.map = module1_managed_to_java,
		.assembly_name = "assembly_one",
		.image = nullptr,
	},
	{
		.module_uuid = { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 },
		.entry_count = 1,
		.map = module2_managed_to_java,
		.assembly_name = "assembly_two",
		.image = nullptr,
	}
};

const TypeMapJava map_java[] = {
	{
		.module_index = 0,
		.type_token_id = 0,
	},
	{
		.module_index = 1,
		.type_token_id = 1,
	},
};

ApplicationConfig application_config = {
	.uses_mono_llvm = false,
	.uses_mono_aot = false,
	.uses_assembly_preload = false,
	.is_a_bundled_app = false,
	.broken_exception_transitions = false,
	.bound_exception_type = 0, // System
	.package_naming_policy = 0,
	.environment_variable_count = 0,
	.system_property_count = 0,
	.android_package_name = "com.xamarin.test",
};

const char* mono_aot_mode_name = "";
const char* app_environment_variables[] = {};
const char* app_system_properties[] = {};
