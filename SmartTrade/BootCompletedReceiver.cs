////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2016-2017 Andreas Huber Dönni.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace SmartTrade
{
    using System.Diagnostics.CodeAnalysis;

    using Android.App;
    using Android.Content;

    internal sealed partial class TradeService
    {
        /// <summary>Sets or cancels an alarm which calls the <see cref="TradeService"/> depending on whether trading
        /// is currently enabled.</summary>
        [BroadcastReceiver(Permission = "RECEIVE_BOOT_COMPLETED")]
        [IntentFilter(new string[] { Intent.ActionBootCompleted })]
        [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Instantiated through reflection.")]
        private sealed class BootCompletedReceiver : BroadcastReceiver
        {
            public sealed override void OnReceive(Context context, Intent intent)
            {
                ScheduleTrade();
                var id = TradeService.IsEnabled ? Resource.String.service_enabled : Resource.String.service_disabled;
                ShowNotification(context, id);
            }

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////

            [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "We want the notification to stay up.")]
            private static void ShowNotification(Context context, int messageId)
            {
                new NotificationPopup(
                    context,
                    new Notification.Builder(context).SetContentText(context.Resources.GetString(messageId))).ToString();
            }
        }
    }
}