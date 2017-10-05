////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2016-2017 Andreas Huber Dönni.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace SmartTrade
{
    using Android.App;
    using Android.Content;
    using Android.Content.PM;
    using Android.OS;
    using Android.Views;
    using Android.Widget;

    [Activity(Label = "@string/AppName", MainLauncher = true, Icon = "@mipmap/icon", ScreenOrientation = ScreenOrientation.Portrait)]
    internal sealed class MainActivity : Activity, AdapterView.IOnItemClickListener
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
            this.SetContentView(Resource.Layout.Main);

            this.tickersListView = this.FindViewById<ListView>(Resource.Id.TickersListView);
            this.tickersListView.Adapter =
                new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, new[] { "BTC/EUR" });
            this.tickersListView.OnItemClickListener = this;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private ListView tickersListView;
    }
}
