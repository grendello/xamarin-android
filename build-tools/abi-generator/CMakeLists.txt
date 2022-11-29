# SPDX-License-Identifier: MIT
cmake_minimum_required(VERSION 3.19.2)

set(CMAKE_EXPORT_COMPILE_COMMANDS ON CACHE BOOL "Generate compile_commands.json" FORCE)

if(NOT DEFINED XA_BUILD_CONFIGURATION)
  message(FATAL_ERROR "Please set the XA_BUILD_CONFIGURATION variable on command line (-DXA_BUILD_CONFIGURATION=name)")
endif()

project(
  abi-generator
  LANGUAGES CXX
  )

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_CXX_EXTENSIONS OFF)

option(ENABLE_NET6 "Use Mono headers from the Net6+ nugets" OFF)

set(XA_BIN_DIR "${CMAKE_CURRENT_SOURCE_DIR}/../../bin/${XA_BUILD_CONFIGURATION}")
set(XA_BUILD_DIR "${CMAKE_CURRENT_SOURCE_DIR}/../../bin/Build${XA_BUILD_CONFIGURATION}")

include("${XA_BUILD_DIR}/xa_build_configuration.cmake")

if(ENABLE_NET6)
  if(ANDROID_ABI MATCHES "^arm64-v8a")
    set(NET6_RUNTIME_DIR "${NETCORE_APP_RUNTIME_DIR_ARM64}")
  elseif(ANDROID_ABI MATCHES "^armeabi-v7a")
    set(NET6_RUNTIME_DIR "${NETCORE_APP_RUNTIME_DIR_ARM}")
  elseif(ANDROID_ABI MATCHES "^x86_64")
    set(NET6_RUNTIME_DIR "${NETCORE_APP_RUNTIME_DIR_X86_64}")
  elseif(ANDROID_ABI MATCHES "^x86")
    set(NET6_RUNTIME_DIR "${NETCORE_APP_RUNTIME_DIR_X86}")
  else()
    message(FATAL "${ANDROID_ABI} is not supported for .NET6 builds")
  endif()
endif()

set(COMPILER_FLAGS
  -O2
  -s
  )

#
# We need structures defined by xamarin-app.hh for both Debug and Release builds
#
add_compile_definitions(DEBUG RELEASE)
add_compile_options("$<$<COMPILE_LANGUAGE:CXX>:${COMPILER_FLAGS}>")
include_directories(
  ${CMAKE_CURRENT_SOURCE_DIR}/../../src/monodroid/jni
  )

if(ENABLE_NET6)
  include_directories("${NET6_RUNTIME_DIR}/native/include/mono-2.0")
else()
  include_directories("${XA_BIN_DIR}/include/mono-2.0")
endif()

add_executable(
  abi-generator
  main.cc
  type_descriptor.cc
  xamarin_android_types.cc
  )
