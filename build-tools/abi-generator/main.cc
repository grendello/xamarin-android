// SPDX-License-Identifier: MIT
#include <iostream>
#include <vector>

#include "xamarin_android_types.hh"

using namespace xamarin::abi::generator;

int main (int argc, char **argv)
{
	std::vector<TypeDescriptorBase> type_descriptors {
		BinaryTypeMapHeaderDescriptor{}
	};

	ManagedCodeWriter writer { "/tmp/test.cs" };
	writer.start_namespace ("Xamarin.Android.NativeRuntime");
	for (TypeDescriptorBase& desc : type_descriptors) {
		desc.write_managed (writer);
	}
	writer.end_namespace ();

	return 0;
}
