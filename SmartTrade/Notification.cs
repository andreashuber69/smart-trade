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
    using Android.Graphics;

    using static Logger;
    using static System.Globalization.CultureInfo;

    internal enum Kind
    {
        NoPopup = -1,
        Trade = 0,
        Transfer,
        Warning,
        Error
    }

    internal sealed class Notification : IDisposable
    {
        public void Dispose() => this.manager.Cancel(this.id);

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        internal Notification(
            Context context, Type activityType, Action<Intent> addArguments, int titleFormatId)
            : this(context, activityType, addArguments, titleFormatId, null)
        {
        }

        internal Notification(
            Context context, Type activityType, Action<Intent> addArguments, int titleFormatId, string titleArgument)
        {
            this.activityType = activityType;
            this.addArguments = addArguments;
            this.manager = NotificationManager.FromContext(context);
            this.id = (int)(Java.Lang.JavaSystem.CurrentTimeMillis() & int.MaxValue);
            this.title = string.Format(CurrentCulture, context.Resources.GetString(titleFormatId), titleArgument);
        }

        internal string ContentText { get; private set; }

        internal void Update(
            Context context, Kind kind, NotifyEvents notifyEvents, int contentFormatId, params object[] args) =>
            this.Update(context, kind, notifyEvents, context.Resources.GetString(contentFormatId), args);

        internal void Update(
            Context context, Kind kind, NotifyEvents notifyEvents, string contentFormat, params object[] args)
        {
            // Sometimes exception messages contain curly braces, which confuses string.Format
            this.ContentText = args.Length > 0 ? string.Format(CurrentCulture, contentFormat, args) : contentFormat;
            var showPopup = (int)kind >= (int)notifyEvents;

            if (showPopup)
            {
                using (var builder = new Android.App.Notification.Builder(context))
                using (var intent = new Intent(context, this.activityType))
                using (var style = new Android.App.Notification.BigTextStyle())
                {
                    // This is necessary so that the intent object is going to be interpreted as a new intent rather than
                    // an update to an existing intent.
                    intent.SetAction(this.id.ToString());

                    this.addArguments(intent);
                    builder
                        .SetSmallIcon(Resource.Drawable.ic_stat_name)
                        .SetContentTitle(this.title)
                        .SetContentText(this.ContentText)
                        .SetContentIntent(PendingIntent.GetActivity(context, 0, intent, 0))
                        .SetStyle(style.BigText(this.ContentText))
                        .SetAutoCancel(true);
                    SetLights(builder, kind);
                    this.manager.Notify(this.id, builder.Build());
                }
            }

            Info("Notification{0}: {1}", showPopup ? string.Empty : " (shown in popup)", this.ContentText);
        }

        internal void Append(
            Context context, Kind kind, NotifyEvents notifyEvents, int contentFormatId, params object[] args) =>
            this.Append(context, kind, notifyEvents, context.Resources.GetString(contentFormatId), args);

        internal void Append(
            Context context, Kind kind, NotifyEvents notifyEvents, string contentFormat, params object[] args)
        {
            var previousContent = this.ContentText.Length > 0 ? this.ContentText + Environment.NewLine : string.Empty;
            this.Update(context, kind, notifyEvents, previousContent + contentFormat, args);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static void SetLights(Android.App.Notification.Builder builder, Kind kind)
        {
            switch (kind)
            {
                case Kind.Error:
                    builder.SetLights(Color.ParseColor("red"), 2000, 2000);
                    break;
                case Kind.Warning:
                    builder.SetLights(Color.ParseColor("yellow"), 2000, 2000);
                    break;
            }
        }

        private readonly Type activityType;
        private readonly Action<Intent> addArguments;
        private readonly NotificationManager manager;
        private readonly int id;
        private readonly string title;
    }
}
