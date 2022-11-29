// SPDX-License-Identifier: MIT
#if !defined (__MANAGED_CODE_WRITER_HH)
#define __MANAGED_CODE_WRITER_HH

#include <iostream>
#include <fstream>
#include <stdexcept>
#include <string>
#include <vector>

#include "enum_bitwise_ops.hh"

namespace xamarin::abi::generator
{
	enum class ManagedClassModifier
	{
		None              = 0 << 0,
		Abstract          = 1 << 0,
		Internal          = 1 << 1,
		New               = 1 << 3,
		Partial           = 1 << 4,
		Private           = 1 << 5,
		Protected         = 1 << 6,
		ProtectedInternal = 1 << 7,
		Public            = 1 << 8,
		Sealed            = 1 << 9,
		Static            = 1 << 10,
	};

	enum class ManagedMethodModifier
	{
		None              = 0 << 0,
		Abstract          = 1 << 0,
		Async             = 1 << 1,
		Extern            = 1 << 2,
		Internal          = 1 << 3,
		New               = 1 << 4,
		Override          = 1 << 5,
		Partial           = 1 << 6,
		Private           = 1 << 7,
		PrivateProtected  = 1 << 8,
		Protected         = 1 << 9,
		ProtectedInternal = 1 << 10,
		Public            = 1 << 11,
		Sealed            = 1 << 12,
		Static            = 1 << 13,
		Virtual           = 1 << 14,
	};

	class ManagedCodeWriter
	{
#if defined (_WIN32)
		static constexpr char newline[] = "\r\n";
#else
		static constexpr char newline[] = "\n";
#endif

	public:
		using ManagedArgument = std::pair<std::string, std::string>;

	public:
		explicit ManagedCodeWriter (std::string const& output_file_path)
			: out_stream (output_file_path)
		{}

		~ManagedCodeWriter ()
		{
			out_stream.flush ();
			out_stream.close ();
		}

		std::ostream& output_stream ()
		{
			return out_stream;
		}

		void add_using (std::string const& ns)
		{
			write_indent ();
			out_stream << "using " << ns << ";" << newline;
		}

		void add_newline (size_t count = 1)
		{
			if (count == 0) {
				return;
			}

			out_stream.write (newline, count);
		}

		void add_not_null_argument_check (std::string const& arg_name, std::string const& message = "")
		{}

		void start_namespace (std::string const& ns)
		{
			write_indent ();
			out_stream << "namespace " << ns << newline
			           << "{" << newline;
			current_indent++;
		}

		void end_namespace ()
		{
			end_block ();
		}

		void start_constructor (std::string const& name, ManagedMethodModifier modifiers, std::vector<ManagedArgument> *args = nullptr)
		{
			start_method_common (/* return_type */ "", name, modifiers, args);
		}

		void start_constructor (std::string const& name, std::vector<ManagedArgument> *args)
		{
			start_constructor (name, ManagedMethodModifier::Public, args);
		}

		void end_constructor ()
		{
			end_block ();
		}

		void start_method (std::string const& return_type, std::string const& name, ManagedMethodModifier modifiers = ManagedMethodModifier::Public, std::vector<std::pair<std::string, std::string>> *args = nullptr)
		{
			if (return_type.length () == 0) {
				throw std::invalid_argument ("'return_type' must not be empty");
			}

			start_method_common (return_type, name, modifiers, args);
		}

		void end_method ()
		{
			end_block ();
		}

		void end_block ()
		{
			outdent ();
			write_indent ();
			out_stream << "}" << newline;
		}

		void start_class (std::string const& klass, ManagedClassModifier modifiers, std::vector<std::string> *base_types = nullptr)
		{
			write_indent ();

			constexpr ManagedClassModifier AccessibilityMask =
				          ManagedClassModifier::Internal |
				          ManagedClassModifier::Private |
				          ManagedClassModifier::Protected |
				          ManagedClassModifier::ProtectedInternal |
				          ManagedClassModifier::Public;

			ManagedClassModifier flags = modifiers & AccessibilityMask;
			if (flags != ManagedClassModifier::None &&
				flags != ManagedClassModifier::Internal &&
			    flags != ManagedClassModifier::Private &&
			    flags != ManagedClassModifier::Protected &&
			    flags != ManagedClassModifier::ProtectedInternal &&
			    flags != ManagedClassModifier::Internal) {
				throw std::invalid_argument ("'modifiers' may specify only one class access modifier");
			}

			auto invalid_if_not_nested = [this](std::string const& modifier) {
				if (in_class) {
					return;
				}

				std::string message ("Modifier '");
				message += modifier;
				message += "' is invalid if class is not nested";
				throw std::invalid_argument (message);
			};

			if ((modifiers & ManagedClassModifier::Public) == ManagedClassModifier::Public) {
				out_stream << "public ";
			} else if ((modifiers & ManagedClassModifier::ProtectedInternal) == ManagedClassModifier::ProtectedInternal) {
				invalid_if_not_nested ("protected internal");
				out_stream << "protected internal ";
			} else if ((modifiers & ManagedClassModifier::Protected) == ManagedClassModifier::Protected) {
				invalid_if_not_nested ("protected");
				out_stream << "protected ";
			} else if ((modifiers & ManagedClassModifier::Internal) == ManagedClassModifier::Internal) {
				if (in_class) {
					// `internal` is default for classes in the namespace scope
					out_stream << "internal ";
				}
			} else if ((modifiers & ManagedClassModifier::Private) == ManagedClassModifier::Private) {
				invalid_if_not_nested ("private");
				out_stream << "private ";
			}

			constexpr ManagedClassModifier KlassModifierMask =
				          ManagedClassModifier::Abstract |
				          ManagedClassModifier::Sealed |
				          ManagedClassModifier::Static;

			flags = modifiers & KlassModifierMask;
			if (flags != ManagedClassModifier::None &&
				flags != ManagedClassModifier::Abstract &&
			    flags != ManagedClassModifier::Sealed &&
			    flags != ManagedClassModifier::Static) {
				throw std::invalid_argument ("'modifiers' can specify only one of: 'Abstract', 'Sealed' or 'Static' class modifiers");
			}

			if ((modifiers & ManagedClassModifier::Abstract) == ManagedClassModifier::Abstract) {
				out_stream << "abstract ";
			}else  if ((modifiers & ManagedClassModifier::Sealed) == ManagedClassModifier::Sealed) {
				out_stream << "sealed ";
			} else if ((modifiers & ManagedClassModifier::Static) == ManagedClassModifier::Static) {
				out_stream << "static ";
			}

			if ((modifiers & ManagedClassModifier::New) == ManagedClassModifier::New) {
				invalid_if_not_nested ("new");
				out_stream << "new ";
			}

			if ((modifiers & ManagedClassModifier::Partial) == ManagedClassModifier::Partial) {
				out_stream << "partial ";
			}

			out_stream << "class " << klass;

			if (base_types != nullptr && base_types->size () > 0) {
				out_stream << " : ";
				bool first = true;
				for (std::string const& type : *base_types) {
					if (!first) {
						out_stream << ", ";
					} else {
						first = false;
					}

					out_stream << type;
				}
			}

			out_stream << newline;
			write_indent ();
			out_stream << "{" << newline;
			current_indent++;

			in_class = true;
		}

		void end_class ()
		{
			outdent ();
			write_indent ();
			out_stream << "}" << newline;

			in_class = false;
		}

	protected:
		void write_indent ()
		{
			if (current_indent == 0) {
				return;
			}
			for (size_t i = 0; i < current_indent; i++) {
				out_stream << "\t";
			}
		}

		void outdent ()
		{
			if (current_indent > 0) {
				current_indent--;
			}
		}

	private:
		void start_method_common (std::string const& return_type, std::string const& name, ManagedMethodModifier modifiers, std::vector<std::pair<std::string, std::string>> *args)
		{
			if (name.length () == 0) {
				throw std::invalid_argument ("'name' must not be empty");
			}

			write_indent ();

			constexpr ManagedMethodModifier AccessibilityMask =
				          ManagedMethodModifier::Internal |
				          ManagedMethodModifier::Private |
				          ManagedMethodModifier::PrivateProtected |
				          ManagedMethodModifier::Protected |
				          ManagedMethodModifier::ProtectedInternal |
				          ManagedMethodModifier::Public;

			ManagedMethodModifier flags = modifiers & AccessibilityMask;
			if (flags == ManagedMethodModifier::None) {
				// Private is the default, make sure it's present
				modifiers |= ManagedMethodModifier::Private;
			}

			if (flags != ManagedMethodModifier::Internal &&
			    flags != ManagedMethodModifier::Private &&
			    flags != ManagedMethodModifier::PrivateProtected &&
			    flags != ManagedMethodModifier::Protected &&
			    flags != ManagedMethodModifier::ProtectedInternal &&
			    flags != ManagedMethodModifier::Public) {
				throw std::invalid_argument ("'modifiers' may specify only one method access modifier");
			}

			constexpr ManagedMethodModifier VTableMask =
				          ManagedMethodModifier::Override |
				          ManagedMethodModifier::Static |
				          ManagedMethodModifier::Virtual;

			flags = modifiers & VTableMask;
			if (flags != ManagedMethodModifier::None &&
			    flags != ManagedMethodModifier::Override &&
			    flags != ManagedMethodModifier::Static &&
			    flags != ManagedMethodModifier::Virtual) {
				throw std::invalid_argument ("`modifiers` may specify only one method vtable modifier");
			}

			constexpr ManagedMethodModifier NewOverrideMask =
				          ManagedMethodModifier::New |
				          ManagedMethodModifier::Override;

			flags = modifiers & NewOverrideMask;
			if (flags != ManagedMethodModifier::None &&
			    flags != ManagedMethodModifier::New &&
			    flags != ManagedMethodModifier::Override) {
				throw std::invalid_argument ("`modifiers` may specify only one of the `New` and `Override` modifiers");
			}

			constexpr ManagedMethodModifier AbstractForbiddenMask =
				          ManagedMethodModifier::Extern |
				          ManagedMethodModifier::Sealed |
				          ManagedMethodModifier::Static |
				          ManagedMethodModifier::Virtual;

			if ((modifiers & ManagedMethodModifier::Abstract) == ManagedMethodModifier::Abstract) {
				if ((modifiers & AbstractForbiddenMask) != ManagedMethodModifier::None) {
					throw std::invalid_argument ("`modifiers` must not specify any vtable modifiers when `Abstract` is set");
				}
			}

			constexpr ManagedMethodModifier PrivateForbiddenMask =
				          ManagedMethodModifier::Abstract |
				          ManagedMethodModifier::Override |
				          ManagedMethodModifier::Virtual;

			if ((modifiers & ManagedMethodModifier::Private) == ManagedMethodModifier::Private) {
				if ((modifiers & PrivateForbiddenMask) != ManagedMethodModifier::None) {
					throw std::invalid_argument ("`modifiers` must not specify any of `Abstract`, `Override` or `Virtual` modifiers when `Private` is set");
				}
			}

			constexpr ManagedMethodModifier PartialForbiddenMask =
				          ManagedMethodModifier::Abstract |
				          ManagedMethodModifier::Extern |
				          ManagedMethodModifier::Internal |
				          ManagedMethodModifier::New |
				          ManagedMethodModifier::Override |
				          ManagedMethodModifier::Private |
				          ManagedMethodModifier::Protected |
				          ManagedMethodModifier::Public |
				          ManagedMethodModifier::Virtual |
				          ManagedMethodModifier::Sealed;

			if ((modifiers & ManagedMethodModifier::Partial) == ManagedMethodModifier::Partial) {
				if ((modifiers & PartialForbiddenMask) != ManagedMethodModifier::None) {
					throw std::invalid_argument ("`modifiers` must not specify any of `Abstract`, `Extern`, `Internal`, `New`, `Override`, `Private`, `Protected`, `Public`, `Sealed` or `Virtual` modifiers when `Partial` is set");
				}

				if (return_type != "void") {
					throw std::invalid_argument ("`return_type` must be `void` for partial methods");
				}
			}

			if ((modifiers & ManagedMethodModifier::Sealed) == ManagedMethodModifier::Sealed) {
				modifiers |= ManagedMethodModifier::Override;
			}

			if ((modifiers & ManagedMethodModifier::Public) == ManagedMethodModifier::Public) {
				out_stream << "public ";
			} else if ((modifiers & ManagedMethodModifier::ProtectedInternal) == ManagedMethodModifier::ProtectedInternal) {
				out_stream << "protected internal ";
			} else if ((modifiers & ManagedMethodModifier::Protected) == ManagedMethodModifier::Protected) {
				out_stream << "protected ";
			} else if ((modifiers & ManagedMethodModifier::Internal) == ManagedMethodModifier::Internal) {
				out_stream << "internal ";
			} else if ((modifiers & ManagedMethodModifier::PrivateProtected) == ManagedMethodModifier::PrivateProtected) {
				out_stream << "private protected ";
			}

			bool has_body = true;
			if ((modifiers & ManagedMethodModifier::Extern) == ManagedMethodModifier::Extern) {
				out_stream << "extern ";
				has_body = false;
			}

			if ((modifiers & ManagedMethodModifier::Sealed) == ManagedMethodModifier::Sealed) {
				out_stream << "sealed ";
			}

			if ((modifiers & ManagedMethodModifier::Abstract) == ManagedMethodModifier::Abstract) {
				out_stream << "abstract ";
				has_body = false;
			} else if ((modifiers & ManagedMethodModifier::Static) == ManagedMethodModifier::Static) {
				out_stream << "static ";
			} else if ((modifiers & ManagedMethodModifier::Virtual) == ManagedMethodModifier::Virtual) {
				out_stream << "virtual ";
			} else if ((modifiers & ManagedMethodModifier::Override) == ManagedMethodModifier::Override) {
				out_stream << "override ";
			}

			if ((modifiers & ManagedMethodModifier::New) == ManagedMethodModifier::New) {
				out_stream << "new ";
			}

			if ((modifiers & ManagedMethodModifier::Partial) == ManagedMethodModifier::Partial) {
				out_stream << "partial ";
			}

			out_stream << name << " (";

			if (args != nullptr) {
				bool first = true;
				for (std::pair<std::string, std::string> const& arg : *args) {
					if (first) {
						first = false;
					} else {
						out_stream << ", ";
					}

					out_stream << arg.first << " " << arg.second;
				}
			}

			out_stream << ")";
			if (!has_body) {
				out_stream << ";" << newline;
				return;
			}

			out_stream << newline;
			write_indent ();
			out_stream << "{" << newline;
			current_indent++;
		}

	private:
		std::ofstream out_stream;
		size_t current_indent = 0;
		bool in_class = false;
	};
}
#endif // __MANAGED_CODE_WRITER_HH
