﻿////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2016-2017 Andreas Huber Dönni.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace SmartTrade
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;

    using Android.App;
    using Android.Content;
    using Bitstamp;

    using static Logger;

    /// <summary>Calls <see cref="TradeService.ScheduleTrade()"/> for all ticker symbols.</summary>
    /// <remarks>Notifies the user about enabled and disabled services.</remarks>
    [BroadcastReceiver(Permission = "RECEIVE_BOOT_COMPLETED")]
    [IntentFilter(new string[] { Intent.ActionBootCompleted })]
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Instantiated through reflection.")]
    internal sealed class BootCompletedReceiver : BroadcastReceiver
    {
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Method is not externally visible, CA bug?")]
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Intentional, we want the popup to remain")]
        public sealed override void OnReceive(Context context, Intent intent)
        {
            Info("Reboot completed at: {0}", Java.Lang.JavaSystem.CurrentTimeMillis());
            var statuses = new StringBuilder();

            foreach (var tickerSymbol in BitstampClient.TickerSymbols)
            {
                using (var service = TradeService.Create(tickerSymbol))
                {
                    service.ScheduleTrade();

                    if (service.IsEnabled)
                    {
                        var message = context.GetString(Resource.String.ServiceIsEnabled);
                        statuses.AppendFormat("{0}: {1}{2}", tickerSymbol, message, Environment.NewLine);
                    }
                }
            }

            var id = statuses.Length == 0 ?
                Resource.String.BootNoServiceEnabledNotification : Resource.String.BootNotification;
            new Notification(context, typeof(MainActivity), i => { }, Resource.String.AppName).Update(
                context, Kind.Trade, NotifyEvents.TradesTransfersWarningsErrors, id, statuses.ToString());
        }
    }
}