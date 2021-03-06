<configuration>
	<dllmap dll="java-interop" target="__Internal" />
	<dllmap wordsize="32" dll="i:cygwin1.dll" target="/system/lib/libc.so" />
	<dllmap wordsize="64" dll="i:cygwin1.dll" target="/system/lib64/libc.so" />
	<dllmap wordsize="32" dll="libc" target="/system/lib/libc.so" />
	<dllmap wordsize="64" dll="libc" target="/system/lib64/libc.so" />
	<dllmap wordsize="32" dll="intl" target="/system/lib/libc.so" />
	<dllmap wordsize="64" dll="intl" target="/system/lib64/libc.so" />
	<dllmap wordsize="32" dll="libintl" target="/system/lib/libc.so" />
	<dllmap wordsize="64" dll="libintl" target="/system/lib64/libc.so" />
	<dllmap dll="MonoPosixHelper" target="libMonoPosixHelper.so" />
	<dllmap wordsize="32" dll="i:msvcrt" target="/system/lib/libc.so" />
	<dllmap wordsize="64" dll="i:msvcrt" target="/system/lib64/libc.so" />
	<dllmap wordsize="32" dll="i:msvcrt.dll" target="/system/lib/libc.so" />
	<dllmap wordsize="64" dll="i:msvcrt.dll" target="/system/lib64/libc.so" />
	<dllmap wordsize="32" dll="sqlite" target="/system/lib/libsqlite.so" />
	<dllmap wordsize="64" dll="sqlite" target="/system/lib64/libsqlite.so" />
	<dllmap wordsize="32" dll="sqlite3" target="/system/lib/libsqlite.so" />
	<dllmap wordsize="64" dll="sqlite3" target="/system/lib64/libsqlite.so" />
	<dllmap wordsize="32" dll="liblog" target="/system/lib/liblog.so" />
	<dllmap wordsize="64" dll="liblog" target="/system/lib64/liblog.so" />
	<dllmap dll="i:kernel32.dll">
		<dllentry dll="__Internal" name="CopyMemory" target="mono_win32_compat_CopyMemory"/>
		<dllentry dll="__Internal" name="FillMemory" target="mono_win32_compat_FillMemory"/>
		<dllentry dll="__Internal" name="MoveMemory" target="mono_win32_compat_MoveMemory"/>
		<dllentry dll="__Internal" name="ZeroMemory" target="mono_win32_compat_ZeroMemory"/>
	</dllmap>

	<dllmap os="osx" dll="i:cygwin1.dll" target="/usr/lib/libc.dylib" />
	<dllmap os="osx" dll="libc" target="/usr/lib/libc.dylib" />
	<dllmap os="osx" dll="intl" target="/usr/lib/libc.dylib" />
	<dllmap os="osx" dll="libintl" target="/usr/lib/libc.dylib" />
	<dllmap os="osx" dll="i:msvcrt" target="/usr/lib/libc.dylib" />
	<dllmap os="osx" dll="i:msvcrt.dll" target="/usr/lib/libc.dylib" />
	<dllmap os="osx" dll="sqlite" target="/usr/lib/libsqlite3.dylib" />
	<dllmap os="osx" dll="sqlite3" target="/usr/lib/libsqlite3.dylib" />
</configuration>
 