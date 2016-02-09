HashCompute v2.0
================

This Windows console application will return the hashed output of the first parameter (currently only C# System.Cryptography classes supported). This can be interpreted as a file (or list of files, newline or comma or semicolon separated) to calculate the hash of their contents with the `-f` switch. The hash algorithm is SHA512 by default and uses a Managed implementation when possible (unless `-u` or `--unmanaged` is specified). Other hash algorithms selectable are SHA1, SHA256, SHA384, MD5 and RIPEMD, passed by name as the second parameter (optional) or anywhere in the command line with `-a=` or `--algorithm=`.

Pre-built binaries are in the /Builds folder. HashCompute.exe will be the last (potentially Development) build that was committed with the most features, HashCompute_vX.Y.exe will be the last release build for that Major and Minor version.

![Usage in cmd](https://raw.githubusercontent.com/Ehryk/HashCompute/master/Documentation/Images/cmdUsage2.png)

Usage:
---
 - ``HashCompute test``
 - ``HashCompute.exe (input) [Algorithm] [Encoding] [Options]``
 - ``echo | set /P=test | HashCompute.exe [Algorithm] [Encoding] [Options]``
 - ``HashCompute File1,File2 -f``
 - ``HashCompute [-h | --help | /? | -? | --? | ?]``
 - ``HashCompute --version``
 - ``Options:``
   - ``-a, --algorithm: Desired hash algorithm``
   - ``-e, --encoding: Desired encoding (UTF7,8,16,32), Defaults to system encoding``
   - ``-v, --verbose: Adds additional output``
   - ``-n, --nonewline: Removes trailing newline similar to echo -n``
   - ``-f, --file: Interpret the input as file(s), delimited by newlines, commas, or semicolons``
   - ``-t, --text: Same as -f but read the file content as text``
   - ``-u, --unmanaged: Uses unmanaged hash implementation, if available``
   - ``-l, --lowercase: Displays hash hex in lowercase (0dc3... instead of 0DC3...)``
   - ``-b, --big-endian: Interprets Input or Text contents with the big-endian of the selected encoding (multibyte encodings only)``
   - ``-c, --color: Disables Colored Output``
   - ``-x, --omit0x: Disables 0x hex specifier (NNN instead of 0xNNN...)``
   - ``-s, --hash-only: Disables filename output with hashes``
   - ``-8, --utf8: Print UTF8 of hash to console (in addition with -v, otherwise in place of)``
 - ``Supported Algorithms: MD5, SHA1, SHA256, SHA384, SHA512, RIPEMD``

Latest Changes:
---
 - Added text mode for hashing the contents of files as strings, various encodings
 - Added file mode for hashing the contents of files
 - Added support for redirection/piping (Stdin)
 - Added various options and using NuGet packages
 - Added all C# System.Cryptography hash algorithms

Release History:
---
 - v2.0 2016.02.09 Refactored with a 'Core' Class Library, added precompiled byte arrays
 - v1.5 2016.01.30 Added CRC32 support, decimal mode
 - v1.4 2015.04.06 Added support for various encodings, text mode
 - v1.3 2015.04.05 Added support for hashing file contents
 - v1.2 2015.04.04 Added support for redirection/piping (Stdin)
 - v1.1 2015.04.02 Added command line options, NuGet packages
 - v1.0 2015.04.01 Initial Release, handling multiple hash algorithms

Author:
 - Eric Menze ([@Ehryk42](https://twitter.com/Ehryk42))

Build Requirements:
---
 - Visual Studio (Built with Visual Studio 2013)
 - NuGet (Packages should restore)
   - [CommandLineParser](https://www.nuget.org/packages/CommandLineParser/)
   - [Costura.Fody](https://www.nuget.org/packages/Costura.Fody/)

Contact:
---
Eric Menze
 - [Email Me](mailto:rhaistlin+gh@gmail.com)
 - [Portfolio](http://ericmenze.com)
 - [Github](https://github.com/Ehryk)
 - [Twitter](https://twitter.com/Ehryk42)
 - [Source Code](https://github.com/Ehryk/HashCompute)
