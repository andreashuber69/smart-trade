////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2016-2017 Andreas Huber Dönni.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace SmartTrade
{
    using Android.App;

    internal abstract class ActivityBase : Activity
    {
        protected sealed override void OnDestroy()
        {
            // https://stackoverflow.com/questions/28863058/xamarin-android-finalizer-not-getting-called-when-leaving-the-activity-to-go-to
            base.OnDestroy();
            this.Dispose();
        }
    }
}