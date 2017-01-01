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

        internal NotificationPopup(Context context, string contentText)
        {
            this.manager = NotificationManager.FromContext(context);
            this.id = (int)(Java.Lang.JavaSystem.CurrentTimeMillis() & int.MaxValue);
            this.Update(context, contentText);
        }

        internal void Update(Context context, string contentText)
        {
            using (var builder = new Notification.Builder(context))
            using (var intent = new Intent(context, typeof(MainActivity)))
            {
                builder
                    .SetSmallIcon(Resource.Drawable.ic_stat_name)
                    .SetContentTitle(context.Resources.GetString(Resource.String.app_name))
                    .SetContentText(contentText)
                    .SetContentIntent(PendingIntent.GetActivity(context, 0, intent, 0))
                    .SetAutoCancel(true);
                this.manager.Notify(this.id, builder.Build());
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private readonly NotificationManager manager;
        private readonly int id;
    }
}