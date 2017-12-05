////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2016-2017 Andreas Huber Dönni.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace SmartTrade
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Android.App;
    using Android.Content;
    using Android.Content.PM;
    using Android.Graphics;
    using Android.OS;
    using Android.Views;
    using Android.Widget;
    using Bitstamp;

    using static Logger;
    using static System.Environment;
    using static System.Globalization.CultureInfo;

    [Activity(Label = "@string/AppName", MainLauncher = true, Icon = "@mipmap/icon", ScreenOrientation = ScreenOrientation.Portrait)]
    internal sealed class MainActivity : ActivityBase, AdapterView.IOnItemClickListener
    {
        public void OnItemClick(AdapterView parent, View view, int position, long id)
        {
            using (var intent = new Intent(this, typeof(StatusActivity)))
            {
                new StatusActivity.Data(this.settings[position].TickerSymbol).Put(intent);
                this.StartActivity(intent);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        protected sealed override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            AppDomain.CurrentDomain.UnhandledException +=
                (s, e) => Error("Unhandled Exception!{0}{1}", NewLine, e.ExceptionObject);

            TaskScheduler.UnobservedTaskException +=
                (s, e) => Error("Unobserved Task Exception!{0}{1}", NewLine, e.Exception);

            this.SetContentView(Resource.Layout.Main);

            this.settings = BitstampClient.TickerSymbols.Select(s => Settings.Create(s)).ToArray();
            this.tickersListView = this.FindViewById<ListView>(Resource.Id.TickersListView);
            this.tickersListView.Adapter = new Adapter(this, this.LayoutInflater, this.settings);
            this.tickersListView.OnItemClickListener = this;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private ISettings[] settings;
        private ListView tickersListView;

        private sealed class Adapter : ArrayAdapter<ISettings>
        {
            public sealed override View GetView(int position, View convertView, ViewGroup parent)
            {
                if (convertView == null)
                {
                    convertView = this.inflater.Inflate(Resource.Layout.OverviewItem, null, false);
                    convertView.Tag = new ViewHolder(convertView, this.GetItem(position));
                }

                return convertView;
            }

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////

            internal Adapter(Context context, LayoutInflater inflater, IEnumerable<ISettings> settings)
                : base(context, Resource.Layout.OverviewItem, settings.ToArray())
            {
                this.inflater = inflater;
            }

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////

            private readonly LayoutInflater inflater;

            private sealed class ViewHolder : Java.Lang.Object
            {
                internal ViewHolder(View row, ISettings settings)
                {
                    this.tickerSymbolTextView = (TextView)row.FindViewById(Resource.Id.TickerSymbol);
                    this.firstBalanceIntegralTextView = (TextView)row.FindViewById(Resource.Id.FirstBalanceIntegral);
                    this.firstBalanceFractionalTextView =
                        (TextView)row.FindViewById(Resource.Id.FirstBalanceFractional);
                    this.firstCurrencyTextView = (TextView)row.FindViewById(Resource.Id.FirstCurrency);
                    this.secondBalanceIntegralTextView = (TextView)row.FindViewById(Resource.Id.SecondBalanceIntegral);
                    this.secondBalanceFractionalTextView =
                        (TextView)row.FindViewById(Resource.Id.SecondBalanceFractional);
                    this.secondCurrencyTextView = (TextView)row.FindViewById(Resource.Id.SecondCurrency);
                    this.unknownColor = new Color(this.tickerSymbolTextView.TextColors.DefaultColor);
                    settings.PropertyChanged += (s, e) => this.Update((ISettings)s, e.PropertyName);
                    this.Update(settings, nameof(ISettings.TickerSymbol));
                }

                ////////////////////////////////////////////////////////////////////////////////////////////////////////

                private readonly TextView tickerSymbolTextView;
                private readonly TextView firstBalanceIntegralTextView;
                private readonly TextView firstBalanceFractionalTextView;
                private readonly TextView firstCurrencyTextView;
                private readonly TextView secondBalanceIntegralTextView;
                private readonly TextView secondBalanceFractionalTextView;
                private readonly TextView secondCurrencyTextView;
                private readonly Color unknownColor;

                private void Update(ISettings settings, string propertyName)
                {
                    switch (propertyName)
                    {
                        case nameof(ISettings.TickerSymbol):
                        case nameof(ISettings.FirstCurrency):
                        case nameof(ISettings.SecondCurrency):
                        case nameof(ISettings.LastBalanceFirstCurrency):
                        case nameof(ISettings.LastBalanceSecondCurrency):
                        case nameof(ISettings.Status):
                            this.tickerSymbolTextView.Text = settings.TickerSymbol;
                            var statusColor = GuiHelper.GetStatusColor(settings.Status, this.unknownColor);
                            this.tickerSymbolTextView.SetTextColor(statusColor);
                            this.firstBalanceIntegralTextView.SetTextColor(statusColor);
                            this.firstBalanceFractionalTextView.SetTextColor(statusColor);
                            this.firstCurrencyTextView.SetTextColor(statusColor);
                            this.secondBalanceIntegralTextView.SetTextColor(statusColor);
                            this.secondBalanceFractionalTextView.SetTextColor(statusColor);
                            this.secondCurrencyTextView.SetTextColor(statusColor);

                            if (settings.NextTradeTime == 0)
                            {
                                this.firstBalanceIntegralTextView.Text = null;
                                this.firstBalanceFractionalTextView.Text = null;
                                this.firstCurrencyTextView.Text = null;
                                this.secondBalanceIntegralTextView.Text = null;
                                this.secondBalanceFractionalTextView.Text = null;
                                this.secondCurrencyTextView.Text = null;
                            }
                            else
                            {
                                GuiHelper.SetBalance(
                                    this.firstBalanceIntegralTextView,
                                    this.firstBalanceFractionalTextView,
                                    this.firstCurrencyTextView,
                                    settings.LastBalanceFirstCurrency,
                                    settings.FirstCurrency);
                                GuiHelper.SetBalance(
                                    this.secondBalanceIntegralTextView,
                                    this.secondBalanceFractionalTextView,
                                    this.secondCurrencyTextView,
                                    settings.LastBalanceSecondCurrency,
                                    settings.SecondCurrency);
                            }

                            break;
                    }
                }
            }
        }
    }
}
