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

    internal abstract class Settings : ISettings
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public int CustomerId
        {
            get { return (int)this.GetLong(); }
            set { this.SetLong(value); }
        }

        public string ApiKey
        {
            get { return this.GetPrivateString(); }
            set { this.SetPrivateString(value); }
        }

        public string ApiSecret
        {
            get { return this.GetPrivateString(); }
            set { this.SetPrivateString(value); }
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
            get { return this.GetLong(); }
            set { this.SetLong(value); }
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

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Ctor is being called, CA bug?")]
        protected Settings(string groupName)
        {
            this.groupName = groupName;
            this.keyStore = KeyStore.GetInstance(KeyStoreName);
            this.keyStore.Load(null);
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
            Info("Set {0}.{1} = {2:o}.", nameof(Settings), key, value);
        }

        private long GetLong([CallerMemberName] string key = null) => this.GetValue((p, k) => p.GetLong(k, 0), key);

        private void SetLong(long value, [CallerMemberName] string key = null) =>
            this.SetValue((p, k, v) => p.PutLong(k, v), key, value);

        private float GetFloat([CallerMemberName] string key = null) =>
            this.GetValue((p, k) => p.GetFloat(k, 0.0f), key);

        private void SetFloat(float value, [CallerMemberName] string key = null) =>
            this.SetValue((p, k, v) => p.PutFloat(k, v), key, value);

        private string GetString([CallerMemberName] string key = null) =>
            this.GetValue((p, k) => p.GetString(k, string.Empty), key);

        private void SetString(string value, [CallerMemberName] string key = null) =>
            this.SetValue((p, k, v) => p.PutString(k, v), key, value);

        private T GetValue<T>(Func<ISharedPreferences, string, T> getValue, string key)
        {
            using (var preferences = PreferenceManager.GetDefaultSharedPreferences(Application.Context))
            {
                return getValue(preferences, this.groupName + key);
            }
        }

        private void SetValue<T>(Action<ISharedPreferencesEditor, string, T> setValue, string key, T value)
        {
            var groupedKey = this.groupName + key;

            using (var preferences = PreferenceManager.GetDefaultSharedPreferences(Application.Context))
            using (var editor = preferences.Edit())
            {
                setValue(editor, groupedKey, value);
                editor.Apply();
            }

            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(key));
            Info("Set {0}.{1} = {2}.", nameof(Settings), groupedKey, value);
        }

        private string GetPrivateString([CallerMemberName] string key = null) =>
            this.Decrypt(this.GetValue((p, k) => p.GetString(k, string.Empty), key));

        private void SetPrivateString(string value, [CallerMemberName] string key = null) =>
            this.SetValue((p, k, v) => p.PutString(k, v), key, this.Encrypt(value));

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
                this.SetValue((p, k, v) => p.PutString(k, v), nameof(this.ApiKey), string.Empty);
                this.SetValue((p, k, v) => p.PutString(k, v), nameof(this.ApiSecret), string.Empty);
            }

            return (KeyStore.PrivateKeyEntry)this.keyStore.GetEntry(KeyName, null);
        }
    }
}