////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2016-2017 Andreas Huber Dönni.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace SmartTrade
{
    using System;
    using System.Threading.Tasks;

    using Android.App;
    using Android.Content;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>Buys or sells according to the configured schedule.</summary>
    /// <remarks>Reschedules itself after each buy/sell attempt.</remarks>
    [Service]
    internal sealed partial class TradeService : IntentService
    {
        internal static bool IsEnabled
        {
            get { return Settings.NextTradeTime > 0; }
            set { ScheduleTrade(value ? Java.Lang.JavaSystem.CurrentTimeMillis() : 0); }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        protected sealed override async void OnHandleIntent(Intent intent)
        {
            var notificationBuilder =
                new Notification.Builder(this).SetContentText(Resources.GetString(Resource.String.service_buying));

            using (new NotificationPopup(this, notificationBuilder))
            {
                await Task.Delay(5000);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static void ScheduleTrade(long time)
        {
            Settings.NextTradeTime = time;
            ScheduleTrade();
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The disposables are passed to API methods, TODO.")]
        private static void ScheduleTrade()
        {
            var context = Application.Context;
            var manager = AlarmManager.FromContext(context);
            var alarmIntent = PendingIntent.GetService(
                context, 0, new Intent(context, typeof(TradeService)), PendingIntentFlags.UpdateCurrent);

            if (Settings.NextTradeTime > 0)
            {
                var earliestNextTradeTime = Java.Lang.JavaSystem.CurrentTimeMillis() + 10 * 1000;
                var nextTradeTime = Math.Max(earliestNextTradeTime, Settings.NextTradeTime);
                manager.Set(AlarmType.RtcWakeup, nextTradeTime, alarmIntent);
            }
            else
            {
                manager.Cancel(alarmIntent);
            }
        }
    }
}
