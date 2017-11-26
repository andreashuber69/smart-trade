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
    using Android.OS;
    using Android.Views;
    using Android.Widget;
    using Bitstamp;

    using static Logger;
    using static System.Environment;

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

            this.settings =
                BitstampClient.TickerSymbols.Select(s => new Settings(s)).OrderBy(s => s.NextTradeTime == 0).ToArray();

            this.tickersListView = this.FindViewById<ListView>(Resource.Id.TickersListView);
            this.tickersListView.Adapter = new Adapter(this, this.LayoutInflater, this.settings);
            this.tickersListView.OnItemClickListener = this;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private Settings[] settings;
        private ListView tickersListView;

        private sealed class Adapter : ArrayAdapter<Settings>
        {
            public sealed override View GetView(int position, View convertView, ViewGroup parent)
            {
                if (convertView == null)
                {
                    convertView = this.inflater.Inflate(Resource.Layout.OverviewItem, null, false);
                    convertView.Tag = new ViewHolder(convertView);
                }

                var holder = (ViewHolder)convertView.Tag;
                var settings = this.GetItem(position);
                holder.TickerSymbolTextView.Text = settings.TickerSymbol;
                holder.FirstBalanceTextView.Text = settings.FirstCurrency + " " + settings.LastBalanceFirstCurrency;
                holder.SecondBalanceTextView.Text = settings.SecondCurrency + " " + settings.LastBalanceSecondCurrency;
                return convertView;
            }

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////

            internal Adapter(Context context, LayoutInflater inflater, IEnumerable<Settings> settings)
                : base(context, Resource.Layout.OverviewItem, settings.ToArray())
            {
                this.inflater = inflater;
            }

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////

            private readonly LayoutInflater inflater;

            private sealed class ViewHolder : Java.Lang.Object
            {
                internal ViewHolder(View row)
                {
                    this.TickerSymbolTextView = (TextView)row.FindViewById(Resource.Id.TickerSymbol);
                    this.FirstBalanceTextView = (TextView)row.FindViewById(Resource.Id.FirstBalance);
                    this.SecondBalanceTextView = (TextView)row.FindViewById(Resource.Id.SecondBalance);
                }

                internal TextView TickerSymbolTextView { get; }

                internal TextView FirstBalanceTextView { get; }

                internal TextView SecondBalanceTextView { get; }
            }
        }
    }
}
