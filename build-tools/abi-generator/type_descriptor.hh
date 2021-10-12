// SPDX-License-Identifier: MIT
#if !defined (__TYPE_DESCRIPTOR_HH)
#define __TYPE_DESCRIPTOR_HH

#include <stdexcept>
#include <string>
#include <typeinfo>
#include <type_traits>

#if defined(__unix__) || defined(__APPLE__)
#define HAVE_CXA_DEMANGLE 1
#endif

#if defined(HAVE_CXA_DEMANGLE)
#include <cxxabi.h>
#else
#error System not supported yet
#endif

#include "managed_code_writer.hh"

namespace xamarin::abi::generator
{
	class TypeDescriptorBase
	{
	public:
		explicit TypeDescriptorBase (std::string managed_type_name)
			: _type_name (managed_type_name)
		{}

		const std::string& type_name () const noexcept
		{
			return _type_name;
		}

		std::string make_managed_name (std::string const& native_name) const
		{
			// TODO: upper-case all the characters preceded by _ and remove the underscores
			return native_name;
		}

		virtual void write_managed (ManagedCodeWriter& output)
		{
			output.start_class (type_name ());
			output.end_class ();
		};

	private:
		std::string const _type_name;
	};

	template<typename Type, size_t TypeSize>
	class TypeDescriptor : public TypeDescriptorBase
	{
	public:
		static constexpr size_t type_size = TypeSize;

	public:
		explicit TypeDescriptor (std::string managed_type_name = demangle_name (typeid(Type)))
			: TypeDescriptorBase (managed_type_name)
		{
			static_assert (sizeof(Type) == TypeSize, "Specified type size does not match the actual one.");
		}

	protected:
		template<typename ExpectedFieldType, size_t ExpectedFieldOffset, size_t ActualOffset, const char NativeFieldName[], typename ActualFieldType>
		void add_field (ActualFieldType Type::*field, std::string managed_field_name = "")
		{
			static_assert (std::is_same_v<ExpectedFieldType, ActualFieldType>, "Unexpected field type");
			static_assert (ActualOffset == ExpectedFieldOffset, "Unexpected field offset");

			std::string field_name = managed_field_name.length () == 0 ? make_managed_name (NativeFieldName) : managed_field_name;
			Type obj{};
			ExpectedFieldType value = obj.*field;

			added_fields_size += sizeof(ExpectedFieldType);
		}

		void ensure_all_fields_added () const
		{
			if (added_fields_size == TypeSize) {
				return;
			}

			std::string message ("Invalid number of fields added to type '");
			message.append (type_name ());
			message.append (": expected type size ");
			message.append (std::to_string (TypeSize));
			message.append (", got ");
			message.append (std::to_string (added_fields_size));
			message.append (" instead.");

			throw std::runtime_error (message);
		}

	private:
		static std::string demangle_name (std::type_info const& ti)
		{
#if defined(HAVE_CXA_DEMANGLE)
			int status;
			return ::abi::__cxa_demangle (ti.name (), 0, 0, &status);
#else
			// TODO: use https://docs.microsoft.com/en-us/windows/win32/api/dbghelp/nf-dbghelp-undecoratesymbolname
#endif
		}

	private:
		size_t added_fields_size = 0;
	};
}
#endif // __TYPE_DESCRIPTOR_HH
