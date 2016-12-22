namespace SmartTrade
{
    using System;
    using System.Runtime.CompilerServices;

    using Android.Content;

    internal sealed class Settings : IDisposable
    {
        private readonly ISharedPreferences preferences;
        private readonly ISharedPreferencesEditor editor;

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public void Dispose()
        {
            this.editor.Dispose();
            this.preferences.Dispose();
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        internal Settings(ContextWrapper context)
        {
            this.preferences = context.GetSharedPreferences(typeof(Settings).FullName, FileCreationMode.Private);
            this.editor = this.preferences.Edit();
        }

        internal bool IsStarted
        {
            get { return GetBoolean(); }
            set { this.SetBoolean(value); }
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private bool GetBoolean([CallerMemberName] string key = null) => this.preferences.GetBoolean(key, false);

        private void SetBoolean(bool value, [CallerMemberName] string key = null) =>
            this.editor.PutBoolean(key, value).Apply();
    }
}