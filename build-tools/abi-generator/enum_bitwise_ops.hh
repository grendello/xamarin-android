// SPDX-License-Identifier: MIT
#if !defined (__ENUM_BITWISE_OPS_HH)
#define __ENUM_BITWISE_OPS_HH

#include <type_traits>

namespace xamarin::abi::generator
{
	template <typename TEnum, std::enable_if_t<std::is_enum_v<TEnum>, int> = 0>
	constexpr TEnum& operator |= (TEnum& left, TEnum right) noexcept
	{
		return left = left | right;
	}

	template <typename TEnum, std::enable_if_t<std::is_enum_v<TEnum>, int> = 0>
	constexpr TEnum& operator &= (TEnum& left, TEnum right) noexcept
	{
		return left = left & right;
	}

	template <typename TEnum, std::enable_if_t<std::is_enum_v<TEnum>, int> = 0>
	constexpr TEnum& operator ^= (TEnum& left, TEnum right) noexcept
	{
		return left = left ^ right;
	}

	template <typename TEnum, std::enable_if_t<std::is_enum_v<TEnum>, int> = 0>
	constexpr TEnum operator ~ (TEnum right) noexcept
	{
		return static_cast<TEnum>(~static_cast<std::underlying_type_t<TEnum>> (right));
	}

	template <typename TEnum, std::enable_if_t<std::is_enum_v<TEnum>, int> = 0>
	constexpr TEnum operator | (TEnum left, TEnum right) noexcept
	{
		return static_cast<TEnum>(static_cast<std::underlying_type_t<TEnum>> (left) | static_cast<std::underlying_type_t<TEnum>> (right));
	}

	template <typename TEnum, std::enable_if_t<std::is_enum_v<TEnum>, int> = 0>
	constexpr TEnum operator & (TEnum left, TEnum right) noexcept
	{
		return static_cast<TEnum>(static_cast<std::underlying_type_t<TEnum>> (left) & static_cast<std::underlying_type_t<TEnum>> (right));
	}

	template <typename TEnum, std::enable_if_t<std::is_enum_v<TEnum>, int> = 0>
	constexpr TEnum operator ^ (TEnum left, TEnum right) noexcept
	{
		return static_cast<TEnum>(static_cast<std::underlying_type_t<TEnum>> (left) ^ static_cast<std::underlying_type_t<TEnum>> (right));
	}
}
#endif // __ENUM_BITWISE_OPS_HH
