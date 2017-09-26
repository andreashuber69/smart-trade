////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2016-2017 Andreas Huber Dönni.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace SmartTrade
{
    using System;
    using Android.App;
    using Android.Content;

    using static Logger;
    using static System.Globalization.CultureInfo;

    internal sealed class NotificationPopup : IDisposable
    {
        public void Dispose() => this.manager.Cancel(this.id);

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        internal NotificationPopup(Context context, int contentFormatId, params object[] args)
        {
            this.manager = NotificationManager.FromContext(context);
            this.id = (int)(Java.Lang.JavaSystem.CurrentTimeMillis() & int.MaxValue);
            this.Update(context, contentFormatId, args);
        }

        internal string ContentText { get; private set; }

        internal void Update(Context context, int contentFormatId, params object[] args) =>
            this.Update(context, context.Resources.GetString(contentFormatId), args);

        internal void Update(Context context, string contentFormat, params object[] args)
        {
            // Sometimes exception messages contain curly braces, which confuses string.Format
            this.ContentText = args.Length > 0 ? string.Format(CurrentCulture, contentFormat, args) : contentFormat;

            using (var builder = new Notification.Builder(context))
            using (var intent = new Intent(context, typeof(MainActivity)))
            using (var style = new Notification.BigTextStyle())
            {
                builder
                    .SetSmallIcon(Resource.Drawable.ic_stat_name)
                    .SetContentTitle(context.Resources.GetString(Resource.String.AppName))
                    .SetContentText(this.ContentText)
                    .SetContentIntent(PendingIntent.GetActivity(context, 0, intent, 0))
                    .SetStyle(style.BigText(this.ContentText))
                    .SetAutoCancel(true);
                this.manager.Notify(this.id, builder.Build());
            }

            Info("Popup: {0}", this.ContentText);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private readonly NotificationManager manager;
        private readonly int id;
    }
}
