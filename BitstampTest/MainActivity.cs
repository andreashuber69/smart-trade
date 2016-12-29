////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2016-2017 Andreas Huber Dönni.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace BitstampTest
{
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;

    using Android.App;
    using Android.OS;
    using Xamarin.Android.NUnitLite;

    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Instantiated through reflection.")]
    [Activity(Label = "BitstampTest", MainLauncher = true, Icon = "@drawable/icon")]
    internal sealed class MainActivity : TestSuiteActivity
    {
        protected sealed override void OnCreate(Bundle bundle)
        {
            this.AddTest(Assembly.GetExecutingAssembly());
            base.OnCreate(bundle);
        }
    }
}
