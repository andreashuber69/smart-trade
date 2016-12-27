////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2016-2017 Andreas Huber Dönni.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Bitstamp
{
    using System.Diagnostics.CodeAnalysis;

    using Android.App;

    /// <summary>Fake main activity to work around https://bugzilla.xamarin.com/show_bug.cgi?id=43553.</summary>
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Instantiated through reflection.")]
    [Activity(MainLauncher = true)]
    internal sealed class FakeActivity : Activity
    {
    }
}