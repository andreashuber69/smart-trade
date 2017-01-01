////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2016-2017 Andreas Huber Dönni.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace SmartTrade
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;

    using Android.App;
    using Android.Content;

    internal sealed class NotificationPopup : IDisposable
    {
        public void Dispose() => this.manager.Cancel(this.id);

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The disposable is passed to an API method, TODO.")]
        internal NotificationPopup(Context context, Notification.Builder builder)
        {
            this.manager = NotificationManager.FromContext(context);
            this.id = Interlocked.Increment(ref lastId);
            this.Update(context, builder);
        }

        internal void Update(Context context, Notification.Builder builder)
        {
            builder
                .SetSmallIcon(Resource.Drawable.ic_stat_name)
                .SetContentTitle(context.Resources.GetString(Resource.String.app_name))
                .SetContentIntent(PendingIntent.GetActivity(context, 0, new Intent(context, typeof(MainActivity)), 0))
                .SetAutoCancel(true);
            this.manager.Notify(this.id, builder.Build());
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static int lastId = 0;
        private readonly NotificationManager manager;
        private readonly int id;
    }
}