HashCompute v1.2
================

This Windows console application will return the hashed output of the first parameter (currently only SHA512).

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
