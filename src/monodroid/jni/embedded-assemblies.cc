#include <host-config.h>

#include <assert.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <sys/stat.h>
#include <sys/mman.h>
#include <fcntl.h>
#include <ctype.h>
#include <libgen.h>
#include <errno.h>

#include <mono/metadata/assembly.h>
#include <mono/metadata/image.h>
#include <mono/metadata/mono-config.h>

#include "java-interop-util.h"

#include "monodroid.h"
#include "util.hh"
#include "embedded-assemblies.hh"
#include "globals.hh"
#include "monodroid-glue.hh"
#include "xamarin-app.hh"
#include "cpp-util.hh"

namespace xamarin::android::internal {
#if defined (DEBUG) || !defined (ANDROID)
	struct TypeMappingInfo {
		char                     *source_apk;
		char                     *source_entry;
		int                       num_entries;
		int                       entry_length;
		int                       value_offset;
		const   char             *mapping;
		TypeMappingInfo          *next;
	};
#endif // DEBUG || !ANDROID
}

using namespace xamarin::android;
using namespace xamarin::android::internal;

void EmbeddedAssemblies::set_assemblies_prefix (const char *prefix)
{
	if (assemblies_prefix_override != nullptr)
		delete[] assemblies_prefix_override;
	assemblies_prefix_override = prefix != nullptr ? utils.strdup_new (prefix) : nullptr;
}

MonoAssembly*
EmbeddedAssemblies::open_from_bundles (MonoAssemblyName* aname, bool ref_only)
{
	const char *culture = mono_assembly_name_get_culture (aname);
	const char *asmname = mono_assembly_name_get_name (aname);

	size_t name_len = culture == nullptr ? 0 : strlen (culture) + 1;
	name_len += sizeof (".dll");
	name_len += strlen (asmname);

	size_t alloc_size = ADD_WITH_OVERFLOW_CHECK (size_t, name_len, 1);
	char *name = new char [alloc_size];
	name [0] = '\0';

	if (culture != nullptr && *culture != '\0') {
		strcat (name, culture);
		strcat (name, "/");
	}
	strcat (name, asmname);
	char *ename = name + strlen (name);

	MonoAssembly *a = nullptr;
	MonoBundledAssembly **p;

	*ename = '\0';
	if (!utils.ends_with (name, ".dll")) {
		strcat (name, ".dll");
	}

	log_info (LOG_ASSEMBLY, "open_from_bundles: looking for bundled name: '%s'", name);

	for (p = bundled_assemblies; p != nullptr && *p; ++p) {
		MonoImage *image = nullptr;
		MonoImageOpenStatus status;
		const MonoBundledAssembly *e = *p;

		if (strcmp (e->name, name) == 0 &&
				(image  = mono_image_open_from_data_with_name ((char*) e->data, e->size, 0, nullptr, ref_only, name)) != nullptr &&
				(a      = mono_assembly_load_from_full (image, name, &status, ref_only)) != nullptr) {
			mono_config_for_assembly (image);
			break;
		}
	}
	delete[] name;

	if (a && utils.should_log (LOG_ASSEMBLY)) {
		log_info_nocheck (LOG_ASSEMBLY, "open_from_bundles: loaded assembly: %p\n", a);
	}
	return a;
}

MonoAssembly*
EmbeddedAssemblies::open_from_bundles_full (MonoAssemblyName *aname, UNUSED_ARG char **assemblies_path, UNUSED_ARG void *user_data)
{
	return embeddedAssemblies.open_from_bundles (aname, false);
}

MonoAssembly*
EmbeddedAssemblies::open_from_bundles_refonly (MonoAssemblyName *aname, UNUSED_ARG char **assemblies_path, UNUSED_ARG void *user_data)
{
	return embeddedAssemblies.open_from_bundles (aname, true);
}

void
EmbeddedAssemblies::install_preload_hooks ()
{
	mono_install_assembly_preload_hook (open_from_bundles_full, nullptr);
	mono_install_assembly_refonly_preload_hook (open_from_bundles_refonly, nullptr);
}

int
EmbeddedAssemblies::TypeMappingInfo_compare_key (const void *a, const void *b)
{
	return strcmp (reinterpret_cast <const char*> (a), reinterpret_cast <const char*> (b));
}

template<typename Key, typename Entry, int (*compare)(const Key*, const Entry*)>
const Entry*
EmbeddedAssemblies::binary_search (const Key *key, const Entry *base, size_t nmemb)
{
	static_assert (compare != nullptr, "compare is a required template parameter");

	while (nmemb > 0) {
		const Entry *ret = base + (nmemb / 2);
		int result = compare (key, ret);
		if (result < 0) {
			nmemb /= 2;
		} else if (result > 0) {
			base = ret + 1;
			nmemb -= nmemb / 2 + 1;
		} else {
			return ret;
		}
	}

	return nullptr;
}

template<typename Key, typename Entry, int (*compare)(const Key*, const Entry*)>
const Entry*
EmbeddedAssemblies::binary_search (const Key *key, const Entry *base, size_t nmemb, size_t extra_size)
{
	static_assert (compare != nullptr, "compare is a required template parameter");

	constexpr size_t size = sizeof(Entry);
	while (nmemb > 0) {
		const Entry *ret = reinterpret_cast<const Entry*>(reinterpret_cast<const uint8_t*>(base) + (size + extra_size) * (nmemb / 2));
		int result = compare (key, ret);
		if (result < 0) {
			nmemb /= 2;
		} else if (result > 0) {
			base = reinterpret_cast<const Entry*>(reinterpret_cast<const uint8_t*>(ret) + size + extra_size);
			nmemb -= nmemb / 2 + 1;
		} else {
			return ret;
		}
	}

	return nullptr;
}

inline const char*
EmbeddedAssemblies::find_entry_in_type_map (const char *name, uint8_t map[], TypeMapHeader& header)
{
	// const char *e = reinterpret_cast<const char*> (bsearch (name, map, header.entry_count, header.entry_length, TypeMappingInfo_compare_key ));
	// if (e == nullptr)
	// 	return nullptr;
	// return e + header.value_offset;
	return nullptr;
}

MonoReflectionType*
EmbeddedAssemblies::typemap_java_to_managed (MonoString *java_type)
{
// #if defined (DEBUG) || !defined (ANDROID)
// 	for (TypeMappingInfo *info = java_to_managed_maps; info != nullptr; info = info->next) {
// 		/* log_warn (LOG_DEFAULT, "# jonp: checking file: %s!%s for type '%s'", info->source_apk, info->source_entry, java); */
// 		const char *e = reinterpret_cast<const char*> (bsearch (java, info->mapping, static_cast<size_t>(info->num_entries), static_cast<size_t>(info->entry_length), TypeMappingInfo_compare_key));
// 		if (e == nullptr)
// 			continue;
// 		return e + info->value_offset;
// 	}
// #endif
// 	return find_entry_in_type_map (java, jm_typemap, jm_typemap_header);
	// Requirement:
	//   We need to implement the stuff below as an icall, so that we can return a Type instance
	//   directly. To do that use `mono_add_internal_call` to register a call and then declare it on
	//   the managed side as:
	//
	//   [MethodImplAttribute(MethodImplOptions.InternalCall)]
	//   static extern Type typemap_java_to_managed (string java_type_name);
	//
	// Steps:
	//   * Find structure corresponding to the module UUID
	//   * Ensure the entry has a valid MonoImage* (mono_image_loaded() + friends)
	//   * Find `java` entry
	//   * Find MonoType* using `mono_class_get (image, token_id)`
	//   * return `mono_type_get_object (image, type)`
	timing_period total_time;
	if (XA_UNLIKELY (utils.should_log (LOG_TIMING))) {
		timing = new Timing ();
		total_time.mark_start ();
	}

	simple_pointer_guard<char[], false> java_type_name (mono_string_to_utf8 (java_type));
	if (!java_type_name || *java_type_name == '\0') {
		return nullptr;
	}

	const TypeMapJava *java_entry = binary_search<const char, TypeMapJava, compare_java_name> (java_type_name.get (), map_java, java_type_count, java_name_width);
	if (java_entry == nullptr) {
		log_warn (LOG_ASSEMBLY, "Unable to find mapping to a managed type from Java type '%s'", java_type_name.get ());
		return nullptr;
	}

	if (java_entry->module_index >= map_module_count) {
		log_warn (LOG_ASSEMBLY, "Mapping from Java type '%s' to managed type has invalid module index", java_type_name.get ());
		return nullptr;
	}

	TypeMapModule &module = const_cast<TypeMapModule&>(map_modules[java_entry->module_index]);
	const TypeMapModuleEntry *entry = binary_search <int32_t, TypeMapModuleEntry, compare_type_token> (&java_entry->type_token_id, module.map, module.entry_count);
	if (entry == nullptr) {
		log_warn (LOG_ASSEMBLY, "Unable to find mapping from Java type '%s' to managed type with token ID %u in module [%s]", java_type_name.get (), java_entry->type_token_id, mono_guid_to_string (module.module_uuid));
		return nullptr;
	}

	if (module.image == nullptr) {
		module.image = mono_image_loaded (module.assembly_name);
		if (module.image == nullptr) {
			// TODO: load
			log_error (LOG_ASSEMBLY, "Assembly '%s' not loaded yet!", module.assembly_name);
		}

		if (module.image == nullptr) {
			log_error (LOG_ASSEMBLY, "Unable to load assembly '%s' when looking up managed type corresponding to Java type '%s'", module.assembly_name, java_type_name.get ());
			return nullptr;
		}
	}

	MonoClass *klass = mono_class_get (module.image, static_cast<uint32_t>(java_entry->type_token_id));
	if (klass == nullptr) {
		log_error (LOG_ASSEMBLY, "Unable to find managed type with token ID %u in assembly '%s', corresponding to Java type '%s'", java_entry->type_token_id, module.assembly_name, java_type_name.get ());
		return nullptr;
	}

	MonoReflectionType *ret = mono_type_get_object (mono_domain_get (), mono_class_get_type (klass));
	if (XA_UNLIKELY (utils.should_log (LOG_TIMING))) {
		total_time.mark_end ();

		Timing::info (total_time, "Typemap.java_to_managed: end, total time");
	}

	return ret;
}

int
EmbeddedAssemblies::compare_java_name (const char *java_name, const TypeMapJava *entry)
{
	return strcmp (java_name, reinterpret_cast<const char*>(entry->java_name));
}

const char*
EmbeddedAssemblies::typemap_managed_to_java (const uint8_t *mvid, const int32_t token)
{
// #if defined (DEBUG) || !defined (ANDROID)
// 	for (TypeMappingInfo *info = managed_to_java_maps; info != nullptr; info = info->next) {
// 		/* log_warn (LOG_DEFAULT, "# jonp: checking file: %s!%s for type '%s'", info->source_apk, info->source_entry, managed); */
// 		const char *e = reinterpret_cast <const char*> (bsearch (managed, info->mapping, static_cast<size_t>(info->num_entries), static_cast<size_t>(info->entry_length), TypeMappingInfo_compare_key));
// 		if (e == nullptr)
// 			continue;
// 		return e + info->value_offset;
// 	}
// #endif
// 	return find_entry_in_type_map (managed, mj_typemap, mj_typemap_header);
	timing_period total_time;
	if (XA_UNLIKELY (utils.should_log (LOG_TIMING))) {
		timing = new Timing ();
		total_time.mark_start ();
	}

	if (mvid == nullptr) {
		return nullptr;
	}

	const TypeMapModule *match = binary_search<uint8_t, TypeMapModule, compare_mvid> (mvid, map_modules, map_module_count);
	if (match == nullptr) {
		log_warn (LOG_ASSEMBLY, "Module matching MVID [%s] not found.", mono_guid_to_string (mvid));
		return nullptr;
	}

	if (match->map == nullptr) {
		log_warn (LOG_ASSEMBLY, "Module with MVID [%s] has no associated type map.", mono_guid_to_string (mvid));
		return nullptr;
	}

	// Each map entry is a pair of 32-bit integers: [TypeTokenID][JavaMapArrayIndex]
	const TypeMapModuleEntry *entry = binary_search <int32_t, TypeMapModuleEntry, compare_type_token> (&token, match->map, match->entry_count);
	if (entry == nullptr) {
		log_warn (LOG_ASSEMBLY, "Type with token %d in module [%s] not found.", token, mono_guid_to_string (mvid));
		return nullptr;
	}

	if (entry->java_map_index >= java_type_count) {
		log_warn (LOG_ASSEMBLY, "Type with token %d in module [%s] has invalid Java type index %u", token, mono_guid_to_string (mvid), entry->java_map_index);
		return nullptr;
	}

	const TypeMapJava *java_entry = reinterpret_cast<const TypeMapJava*> (reinterpret_cast<const uint8_t*>(map_java) + ((sizeof(TypeMapJava) + java_name_width) * entry->java_map_index));
	const char *ret = reinterpret_cast<const char*>(java_entry->java_name);

	if (XA_UNLIKELY (utils.should_log (LOG_TIMING))) {
		total_time.mark_end ();

		Timing::info (total_time, "Typemap.managed_to_java: end, total time");
	}

	log_debug (
		LOG_ASSEMBLY,
		"Type with token %d in module [%s] corresponds to Java type '%s'",
		token,
		mono_guid_to_string (mvid),
		ret
	);

	return ret;
}

int
EmbeddedAssemblies::compare_type_token (const int32_t *token, const TypeMapModuleEntry *entry)
{
	return *token - entry->type_token_id;
}

int
EmbeddedAssemblies::compare_mvid (const uint8_t *mvid, const TypeMapModule *module)
{
	return memcmp (mvid, module->module_uuid, 16);
}

#if defined (DEBUG) || !defined (ANDROID)
void
EmbeddedAssemblies::extract_int (const char **header, const char *source_apk, const char *source_entry, const char *key_name, int *value)
{
	int    read              = 0;
	int    consumed          = 0;
	size_t key_name_len      = 0;
	char   scanf_format [20] = { 0, };

	if (header == nullptr || *header == nullptr)
		return;

	key_name_len    = strlen (key_name);
	if (key_name_len >= (sizeof (scanf_format) - sizeof ("=%d%n"))) {
		*header = nullptr;
		return;
	}

	snprintf (scanf_format, sizeof (scanf_format), "%s=%%d%%n", key_name);

	read = sscanf (*header, scanf_format, value, &consumed);
	if (read != 1) {
		log_warn (LOG_DEFAULT, "Could not read header '%s' value from '%s!%s': read %i elements, expected 1 element. Contents: '%s'",
				key_name, source_apk, source_entry, read, *header);
		*header = nullptr;
		return;
	}
	*header = *header + consumed + 1;
}

bool
EmbeddedAssemblies::add_type_mapping (TypeMappingInfo **info, const char *source_apk, const char *source_entry, const char *addr)
{
	TypeMappingInfo *p        = new TypeMappingInfo (); // calloc (1, sizeof (struct TypeMappingInfo));
	int              version  = 0;
	const char      *data     = addr;

	extract_int (&data, source_apk, source_entry, "version",   &version);
	if (version != 1) {
		delete p;
		log_warn (LOG_DEFAULT, "Unsupported version '%i' within type mapping file '%s!%s'. Ignoring...", version, source_apk, source_entry);
		return false;
	}

	extract_int (&data, source_apk, source_entry, "entry-count",  &p->num_entries);
	extract_int (&data, source_apk, source_entry, "entry-len",    &p->entry_length);
	extract_int (&data, source_apk, source_entry, "value-offset", &p->value_offset);
	p->mapping      = data;

	if ((p->mapping == 0) ||
			(p->num_entries <= 0) ||
			(p->entry_length <= 0) ||
			(p->value_offset >= p->entry_length) ||
			(p->mapping == nullptr)) {
		log_warn (LOG_DEFAULT, "Could not read type mapping file '%s!%s'. Ignoring...", source_apk, source_entry);
		delete p;
		return false;
	}

	p->source_apk   = strdup (source_apk);
	p->source_entry = strdup (source_entry);
	if (*info) {
		(*info)->next = p;
	} else {
		*info = p;
	}
	return true;
}
#endif // DEBUG || !ANDROID

EmbeddedAssemblies::md_mmap_info
EmbeddedAssemblies::md_mmap_apk_file (int fd, uint32_t offset, uint32_t size, const char* filename, const char* apk)
{
	md_mmap_info file_info;
	md_mmap_info mmap_info;

	size_t pageSize       = static_cast<size_t>(monodroid_getpagesize());
	uint32_t offsetFromPage  = static_cast<uint32_t>(offset % pageSize);
	uint32_t offsetPage      = offset - offsetFromPage;
	uint32_t offsetSize      = size + offsetFromPage;

	mmap_info.area        = mmap (nullptr, offsetSize, PROT_READ, MAP_PRIVATE, fd, static_cast<off_t>(offsetPage));

	if (mmap_info.area == MAP_FAILED) {
		log_fatal (LOG_DEFAULT, "Could not `mmap` apk `%s` entry `%s`: %s", apk, filename, strerror (errno));
		exit (FATAL_EXIT_CANNOT_FIND_APK);
	}

	mmap_info.size  = offsetSize;
	file_info.area  = (void*)((const char*)mmap_info.area + offsetFromPage);
	file_info.size  = size;

	log_info (LOG_ASSEMBLY, "                       mmap_start: %08p  mmap_end: %08p  mmap_len: % 12u  file_start: %08p  file_end: %08p  file_len: % 12u      apk: %s  file: %s",
	          mmap_info.area, reinterpret_cast<int*> (mmap_info.area) + mmap_info.size, (unsigned int) mmap_info.size,
	          file_info.area, reinterpret_cast<int*> (file_info.area) + file_info.size, (unsigned int) file_info.size, apk, filename);

	return file_info;
}

bool
EmbeddedAssemblies::register_debug_symbols_for_assembly (const char *entry_name, MonoBundledAssembly *assembly, const mono_byte *debug_contents, int debug_size)
{
	const char *entry_basename  = strrchr (entry_name, '/') + 1;
	// System.dll, System.dll.mdb case
	if (strncmp (assembly->name, entry_basename, strlen (assembly->name)) != 0) {
		// That failed; try for System.dll, System.pdb case
		const char *eb_ext  = strrchr (entry_basename, '.');
		if (eb_ext == nullptr)
			return false;
		off_t basename_len    = static_cast<off_t>(eb_ext - entry_basename);
		assert (basename_len > 0 && "basename must have a length!");
		if (strncmp (assembly->name, entry_basename, static_cast<size_t>(basename_len)) != 0)
			return false;
	}

	mono_register_symfile_for_assembly (assembly->name, debug_contents, debug_size);

	return true;
}

void
EmbeddedAssemblies::gather_bundled_assemblies_from_apk (const char* apk, monodroid_should_register should_register)
{
	int fd;

	if ((fd = open (apk, O_RDONLY)) < 0) {
		log_error (LOG_DEFAULT, "ERROR: Unable to load application package %s.", apk);
		exit (FATAL_EXIT_NO_ASSEMBLIES);
	}

	zip_load_entries (fd, utils.strdup_new (apk), should_register);
	close(fd);
}

#if defined (DEBUG) || !defined (ANDROID)
void
EmbeddedAssemblies::try_load_typemaps_from_directory (const char *path)
{
	// read the entire typemap file into a string
	// process the string using the add_type_mapping
	char *dir_path = utils.path_combine (path, "typemaps");
	if (dir_path == nullptr || !utils.directory_exists (dir_path)) {
		log_warn (LOG_DEFAULT, "directory does not exist: `%s`", dir_path);
		free (dir_path);
		return;
	}

	monodroid_dir_t *dir;
	if ((dir = utils.monodroid_opendir (dir_path)) == nullptr) {
		log_warn (LOG_DEFAULT, "could not open directory: `%s`", dir_path);
		free (dir_path);
		return;
	}

	monodroid_dirent_t *e;
	while ((e = androidSystem.readdir (dir)) != nullptr) {
#if WINDOWS
		char *file_name = utils.utf16_to_utf8 (e->d_name);
#else   /* def WINDOWS */
		char *file_name = e->d_name;
#endif  /* ndef WINDOWS */
		char *file_path = utils.path_combine (dir_path, file_name);
		if (utils.monodroid_dirent_hasextension (e, ".mj") || utils.monodroid_dirent_hasextension (e, ".jm")) {
			char *val = nullptr;
			size_t len = androidSystem.monodroid_read_file_into_memory (file_path, val);
			if (len > 0 && val != nullptr) {
				if (utils.monodroid_dirent_hasextension (e, ".mj")) {
					if (!add_type_mapping (&managed_to_java_maps, file_path, override_typemap_entry_name, ((const char*)val)))
						delete[] val;
				} else if (utils.monodroid_dirent_hasextension (e, ".jm")) {
					if (!add_type_mapping (&java_to_managed_maps, file_path, override_typemap_entry_name, ((const char*)val)))
						delete[] val;
				}
			}
		}
	}
	utils.monodroid_closedir (dir);
	free (dir_path);
	return;
}
#endif

size_t
EmbeddedAssemblies::register_from (const char *apk_file, monodroid_should_register should_register)
{
	size_t prev  = bundled_assemblies_count;

	gather_bundled_assemblies_from_apk (apk_file, should_register);

	log_info (LOG_ASSEMBLY, "Package '%s' contains %i assemblies", apk_file, bundled_assemblies_count - prev);

	if (bundled_assemblies) {
		size_t alloc_size = MULTIPLY_WITH_OVERFLOW_CHECK (size_t, sizeof(void*), bundled_assemblies_count + 1);
		bundled_assemblies  = reinterpret_cast <MonoBundledAssembly**> (utils.xrealloc (bundled_assemblies, alloc_size));
		bundled_assemblies [bundled_assemblies_count] = nullptr;
	}

	return bundled_assemblies_count;
}
