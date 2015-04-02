HashCompute v1.2
================

This Windows console application will return the hashed output of the first parameter (currently only C# System.Cryptography classes supported). This is SHA512 by default and uses a Managed implementation when possible (unless `-u` or `--unmanaged` is specified). Other hash algorithms selectable are SHA1, SHA256, SHA384, MD5 and RIPEMD, passed by name as the second parameter (optional) or anywhere in the command line with `-a=` or `--algorithm=`.

Pre-built binaries are in the /Builds folder. HashCompute.exe will be the last (potentially Development) build that was committed with the most features, HashCompute_vX.Y.exe will be the last release build for that Major and Minor version.

![Usage in cmd](https://raw.githubusercontent.com/Ehryk/HashCompute/master/Documentation/Images/cmdUsage.png)

Usage:
---
 - ``HashCompute test``
 - ``HashCompute.exe (input) [Algorithm] [Options]``
 - ``HashCompute [-h | --help | /? | -? | --? | ?]``
 - ``HashCompute --version``
 - ``Options:``
   - ``-v, --verbose: Adds additional output``
   - ``-n, --nonewline: Removes trailing newline similar to echo -n``
   - ``-u, --unmanaged: Uses unmanaged hash implementation, if possible``
   - ``-l, --lowercase: Displays hash hex in lowercase``
   - ``-c, --color: Disables Colored Output``
 - ``Supported Algorithms: MD5, SHA1, SHA256, SHA384, SHA512, RIPEMD``

Latest Changes:
---
 - Added various options and using NuGet packages

Release History:
---
 - v1.2 (In Development)
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
 - [www.ericmenze.com](http://ericmenze.com)
 - [Github](https://github.com/Ehryk)
 - [Twitter](https://twitter.com/Ehryk42)
 - [Source Code](https://github.com/Ehryk/HashCompute)
