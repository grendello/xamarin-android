// SPDX-License-Identifier: MIT
#if !defined (__MANAGED_CODE_WRITER_HH)
#define __MANAGED_CODE_WRITER_HH

#include <iostream>
#include <fstream>
#include <string>

namespace xamarin::abi::generator
{
	class ManagedCodeWriter
	{
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

		void write_using (std::string const& ns)
		{
			write_indent ();
			out_stream << "using " << ns << ";" << std::endl;
		}

		void start_namespace (std::string const& ns)
		{
			write_indent ();
			out_stream << "namespace " << ns << std::endl
			           << "{" << std::endl;
			current_indent++;
		}

		void end_namespace ()
		{
			outdent ();
			write_indent ();
			out_stream << "}" << std::endl;
		}

		void start_class (std::string const& klass)
		{
			write_indent ();
			out_stream << "class " << klass << std::endl;
			write_indent ();
			out_stream << "{" << std::endl;
			current_indent++;
		}

		void end_class ()
		{
			outdent ();
			write_indent ();
			out_stream << "}" << std::endl;
		}

	protected:
		void write_indent ()
		{
			if (current_indent == 0) {
				return;
			}
			out_stream.write ("\t", current_indent);
		}

		void outdent ()
		{
			if (current_indent > 0) {
				current_indent--;
			}
		}

	private:
		std::ofstream out_stream;
		size_t current_indent = 0;
	};
}
#endif // __MANAGED_CODE_WRITER_HH
