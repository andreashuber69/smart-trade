////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2016-2017 Andreas Huber Dönni.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;

// General
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("SmartTrade")]
[assembly: AssemblyCopyright("Copyright 2016-2017 Andreas Huber Dönni.\r\nDistributed under the Boost Software License, Version 1.0.\r\n(See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)")]
[assembly: AssemblyTrademark("")]

// COM
[assembly: ComVisible(false)]

// CA enforced
[assembly: CLSCompliant(false)]

#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif

// i18n
[assembly: AssemblyCulture("")]
[assembly: NeutralResourcesLanguage("en-US")]

// Versioning
[assembly: AssemblyVersion("0.0.0.1")]
[assembly: AssemblyFileVersion("0.0.0.1")]
[assembly: AssemblyInformationalVersion("0.0.0.1")]
