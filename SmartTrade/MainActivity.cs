////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2016-2017 Andreas Huber Dönni.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace SmartTrade
{
    using System;
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
            var intent = new Intent(this, typeof(StatusActivity));
            this.StartActivity(intent);
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

            this.tickersListView = this.FindViewById<ListView>(Resource.Id.TickersListView);
            this.tickersListView.Adapter = new ArrayAdapter<string>(
                this, Android.Resource.Layout.SimpleListItem1, BitstampClient.TickerSymbols.ToArray());
            this.tickersListView.OnItemClickListener = this;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private ListView tickersListView;
    }
}
