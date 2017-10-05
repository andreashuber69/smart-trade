////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2016-2017 Andreas Huber Dönni.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace SmartTrade
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;
    using System.Text;
    using Android.App;
    using Android.Content;
    using Android.Preferences;
    using Android.Security;
    using Android.Security.Keystore;
    using Java.Math;
    using Java.Security;
    using Java.Util;
    using Javax.Crypto;
    using Javax.Security.Auth.X500;

    using static Logger;

    /// <summary>Holds the settings for a given ticker symbol.</summary>
    /// <remarks>The object implementing <see cref="ISharedPreferencesOnSharedPreferenceChangeListener"/> apparently
    /// needs to derive from <see cref="Java.Lang.Object"/>.</remarks>
    internal sealed class Settings : Java.Lang.Object, ISettings, ISharedPreferencesOnSharedPreferenceChangeListener
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string Ticker => this.FirstCurrency + "/" + this.SecondCurrency;

        public string FirstCurrency => this.groupName.Substring(0, 3).ToUpperInvariant();

        public string SecondCurrency => this.groupName.Substring(3).ToUpperInvariant();

        public int CustomerId
        {
            get { return (int)this.GetLong(); }
            set { this.SetLong(value); }
        }

        public string ApiKey
        {
            get { return this.GetString(); }
            set { this.SetString(value); }
        }

        public string ApiSecret
        {
            get { return this.GetPrivateString(); }
            set { this.SetPrivateString(value); }
        }

        public bool Buy
        {
            get { return this.GetLong() != 0; }
            set { this.SetLong(value ? 1 : 0); }
        }

        public float TradePeriod
        {
            get { return this.GetFloat(); }
            set { this.SetFloat(value); }
        }

        public DateTime? LastTradeTime
        {
            get { return this.GetDateTime(); }
            set { this.SetDateTime(value); }
        }

        public string LastResult
        {
            get { return this.GetString(); }
            set { this.SetString(value); }
        }

        public float LastBalanceFirstCurrency
        {
            get { return this.GetFloat(); }
            set { this.SetFloat(value); }
        }

        public float LastBalanceSecondCurrency
        {
            get { return this.GetFloat(); }
            set { this.SetFloat(value); }
        }

        public long NextTradeTime
        {
            get
            {
                return this.GetLong();
            }

            set
            {
                if ((this.GetLong() != 0) && (value == 0))
                {
                    this.ClearSettings();
                }

                this.SetLong(value);
            }
        }

        public DateTime? SectionStart
        {
            get { return this.GetDateTime(); }
            set { this.SetDateTime(value); }
        }

        public DateTime? PeriodEnd
        {
            get { return this.GetDateTime(); }
            set { this.SetDateTime(value); }
        }

        public DateTime LastTransactionTimestamp
        {
            get { return this.GetDateTime() ?? DateTime.MinValue; }
            set { this.SetDateTime(value); }
        }

        public long RetryIntervalMilliseconds
        {
            get { return this.GetLong(); }
            set { this.SetLong(value); }
        }

        public void LogCurrentValues()
        {
            this.LogCurrentValue(nameof(this.CustomerId), this.CustomerId);
            this.LogCurrentValue(nameof(this.ApiKey), this.ApiKey);
            this.LogCurrentValue(nameof(this.ApiSecret), "<secret>");
            this.LogCurrentValue(nameof(this.Buy), this.Buy);
            this.LogCurrentValue(nameof(this.TradePeriod), this.TradePeriod);
            this.LogCurrentValue(nameof(this.LastTradeTime), this.LastTradeTime, ":o");
            this.LogCurrentValue(nameof(this.LastResult), this.LastResult);
            this.LogCurrentValue(nameof(this.LastBalanceFirstCurrency), this.LastBalanceFirstCurrency, ":f8");
            this.LogCurrentValue(nameof(this.LastBalanceSecondCurrency), this.LastBalanceSecondCurrency, ":f8");
            this.LogCurrentValue(nameof(this.NextTradeTime), this.NextTradeTime);
            this.LogCurrentValue(nameof(this.SectionStart), this.SectionStart, ":o");
            this.LogCurrentValue(nameof(this.PeriodEnd), this.PeriodEnd, ":o");
            this.LogCurrentValue(nameof(this.LastTransactionTimestamp), this.LastTransactionTimestamp, ":o");
            this.LogCurrentValue(nameof(this.RetryIntervalMilliseconds), this.RetryIntervalMilliseconds);
        }

        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Method is not extrenally visible, CA bug?")]
        public void OnSharedPreferenceChanged(ISharedPreferences sharedPreferences, string key)
        {
            if (key.StartsWith(this.groupName, StringComparison.Ordinal))
            {
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(key.Substring(this.groupName.Length)));
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        internal Settings(string groupName)
        {
            this.groupName = groupName;
            this.preferences = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
            this.preferences.RegisterOnSharedPreferenceChangeListener(this);
            this.keyStore = KeyStore.GetInstance(KeyStoreName);
            this.keyStore.Load(null);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        protected sealed override void Dispose(bool disposing)
        {
            this.preferences.UnregisterOnSharedPreferenceChangeListener(this);
            this.preferences.Dispose();
            base.Dispose(disposing);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private const string KeyStoreName = "AndroidKeyStore";
        private const string KeyName = "BitstampApiCredentials";

        private static void GenerateKey()
        {
            var keyGenerator = KeyPairGenerator.GetInstance(KeyProperties.KeyAlgorithmRsa, KeyStoreName);

// KeyPairGeneratorSpec seems to offer the only way to generate RSA keys with the API level we're currently targeting
// (level 18). Since this has been deprecated in API level 23 and later and we're always compiling against the latest
// platform, we're bound to get CS0618 here.
#pragma warning disable 0618
            using (var builder = new KeyPairGeneratorSpec.Builder(Application.Context))
#pragma warning restore
            using (var principal = new X500Principal("CN=" + KeyName))
            {
                var cal = Calendar.Instance;
                var start = cal.Time;
                cal.Add(CalendarField.Year, 10);
                var end = cal.Time;

                var spec = builder
                    .SetAlias(KeyName)
                    .SetStartDate(start)
                    .SetEndDate(end)
                    .SetSerialNumber(BigInteger.One)
                    .SetSubject(principal).Build();
                keyGenerator.Initialize(spec);
                keyGenerator.GenerateKeyPair();
            }
        }

        private readonly string groupName;
        private readonly ISharedPreferences preferences;
        private readonly KeyStore keyStore;

        private DateTime? GetDateTime([CallerMemberName] string key = null)
        {
            var ticks = this.GetLong(key);
            return ticks == 0 ? (DateTime?)null : new DateTime(ticks, DateTimeKind.Utc);
        }

        private void SetDateTime(DateTime? value, [CallerMemberName] string key = null)
        {
            if (value.HasValue && (value.Value.Kind != DateTimeKind.Utc))
            {
                throw new ArgumentException("UTC kind expected.", nameof(value));
            }

            this.SetLong(value?.Ticks ?? 0, key);
            this.LogSetValue(key, value, ":o");
        }

        private long GetLong([CallerMemberName] string key = null) => this.GetValue((p, k) => p.GetLong(k, 0), key);

        private void SetLong(long value, [CallerMemberName] string key = null) =>
            this.SetValue((p, k, v) => p.PutLong(k, v), key, this.GetLong(key), value);

        private float GetFloat([CallerMemberName] string key = null) =>
            this.GetValue((p, k) => p.GetFloat(k, 0.0f), key);

        private void SetFloat(float value, [CallerMemberName] string key = null) =>
            this.SetValue((p, k, v) => p.PutFloat(k, v), key, this.GetFloat(key), value, ":f8");

        private string GetString([CallerMemberName] string key = null) =>
            this.GetValue((p, k) => p.GetString(k, string.Empty), key);

        private void SetString(string value, [CallerMemberName] string key = null) =>
            this.SetValue((p, k, v) => p.PutString(k, v), key, this.GetString(key), value);

        private void ClearSettings()
        {
            this.SetLong(0, nameof(this.LastTradeTime));
            this.SetString(string.Empty, nameof(this.LastResult));
            this.SetFloat(0.0f, nameof(this.LastBalanceFirstCurrency));
            this.SetFloat(0.0f, nameof(this.LastBalanceSecondCurrency));
            this.SetLong(0, nameof(this.SectionStart));
            this.SetLong(0, nameof(this.PeriodEnd));
            this.SetLong(0, nameof(this.LastTransactionTimestamp));
            this.SetLong(0, nameof(this.RetryIntervalMilliseconds));
        }

        private T GetValue<T>(Func<ISharedPreferences, string, T> getValue, string key) =>
            getValue(this.preferences, this.GetGroupedKey(key));

        private void SetValue<T>(
            Action<ISharedPreferencesEditor, string, T> setValue,
            string key,
            T currentValue,
            T newValue,
            string valueFormat = null)
        {
            if (object.Equals(currentValue, newValue))
            {
                return;
            }

            using (var editor = this.preferences.Edit())
            {
                setValue(editor, this.GetGroupedKey(key), newValue);
                editor.Apply();
            }

            this.LogSetValue(key, newValue, valueFormat);
        }

        private string GetPrivateString([CallerMemberName] string key = null) =>
            this.Decrypt(this.GetValue((p, k) => p.GetString(k, string.Empty), key));

        private void SetPrivateString(string value, [CallerMemberName] string key = null)
        {
            // Because two calls to Encrypt passing the same value will always yield a different cipher text, we have to
            // check for equality before encryption.
            if (this.GetPrivateString(key) == value)
            {
                return;
            }

            this.SetValue((p, k, v) => p.PutString(k, v), key, null, this.Encrypt(value));
        }

        private string Decrypt(string encryptedValue) =>
            Encoding.UTF8.GetString(this.Crypt(Convert.FromBase64String(encryptedValue), CipherMode.DecryptMode));

        private string Encrypt(string value) =>
            Convert.ToBase64String(this.Crypt(Encoding.UTF8.GetBytes(value), CipherMode.EncryptMode));

        private byte[] Crypt(byte[] input, CipherMode mode)
        {
            var transformation = KeyProperties.KeyAlgorithmRsa + '/' +
                KeyProperties.BlockModeEcb + '/' + KeyProperties.EncryptionPaddingRsaPkcs1;

            using (var cipher = Cipher.GetInstance(transformation))
            {
                var key = this.GetKey();

                if (mode == CipherMode.EncryptMode)
                {
                    cipher.Init(mode, key.Certificate);
                }
                else
                {
                    if (input.Length == 0)
                    {
                        return input;
                    }

                    cipher.Init(mode, key.PrivateKey);
                }

                return cipher.DoFinal(input);
            }
        }

        private KeyStore.PrivateKeyEntry GetKey()
        {
            if (!this.keyStore.ContainsAlias(KeyName))
            {
                GenerateKey();

                // If we generate a new key, old encrypted data is useless.
                this.SetValue((p, k, v) => p.PutString(k, v), nameof(this.ApiSecret), null, string.Empty);
            }

            return (KeyStore.PrivateKeyEntry)this.keyStore.GetEntry(KeyName, null);
        }

        private void LogSetValue<T>(string key, T value, string valueFormat = null) =>
            this.LogValue("Set", key, value, valueFormat);

        private void LogCurrentValue<T>(string key, T value, string valueFormat = null) =>
            this.LogValue("Current Value", key, value, valueFormat);

        private void LogValue<T>(string prefix, string key, T value, string valueFormat = null) =>
            Info("{0} {1}.{2}{3} = {4" + valueFormat + "}.", prefix, nameof(Settings), this.groupName, key, value);

        private string GetGroupedKey(string key) => this.groupName + key;
    }
}