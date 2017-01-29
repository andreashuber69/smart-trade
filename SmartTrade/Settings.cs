////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2016-2017 Andreas Huber Dönni.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace SmartTrade
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Text;
    using Android.App;
    using Android.Content;
    using Android.Preferences;
    using Android.Security.Keystore;
    using Java.Security;
    using Javax.Crypto;

    using static Logger;

    internal sealed class Settings : ISettings
    {
        public Settings()
        {
            this.keyStore = KeyStore.GetInstance(KeyStoreName);
            this.keyStore.Load(null);
        }

        public long NextTradeTime
        {
            get { return GetLong(); }
            set { SetLong(value); }
        }

        public DateTime? SectionStart
        {
            get { return GetDateTime(); }
            set { SetDateTime(value); }
        }

        public DateTime? PeriodEnd
        {
            get { return GetDateTime(); }
            set { SetDateTime(value); }
        }

        public DateTime LastTransactionTimestamp
        {
            get { return GetDateTime() ?? DateTime.MinValue; }
            set { SetDateTime(value); }
        }

        public long RetryIntervalMilliseconds
        {
            get { return GetLong(); }
            set { SetLong(value); }
        }

        public int CustomerId
        {
            get { return (int)GetLong(); }
            set { SetLong(value); }
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

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private const string KeyStoreName = "AndroidKeyStore";
        private const string KeyName = "BitstampApiCredentials";

        private static DateTime? GetDateTime([CallerMemberName] string key = null)
        {
            var ticks = GetLong(key);
            return ticks == 0 ? (DateTime?)null : new DateTime(ticks, DateTimeKind.Utc);
        }

        private static void SetDateTime(DateTime? value, [CallerMemberName] string key = null)
        {
            if (value.HasValue && (value.Value.Kind != DateTimeKind.Utc))
            {
                throw new ArgumentException("UTC kind expected.", nameof(value));
            }

            SetLong(value?.Ticks ?? 0, key);
            Info("Set {0}.{1} = {2:o}.", nameof(Settings), key, value);
        }

        private static long GetLong([CallerMemberName] string key = null) => GetValue(p => p.GetLong(key, 0));

        private static void SetLong(long value, [CallerMemberName] string key = null)
        {
            SetValue(p => p.PutLong(key, value));
            Info("Set {0}.{1} = {2}.", nameof(Settings), key, value);
        }

        private static void SetValue(Action<ISharedPreferencesEditor> setValue)
        {
            GetValue(
                p =>
                {
                    using (var editor = p.Edit())
                    {
                        setValue(editor);
                        editor.Apply();
                        return false;
                    }
                });
        }

        private static T GetValue<T>(Func<ISharedPreferences, T> getValue)
        {
            using (var preferences = PreferenceManager.GetDefaultSharedPreferences(Application.Context))
            {
                return getValue(preferences);
            }
        }

        private static IKey GenerateKey()
        {
            using (var builder = new KeyGenParameterSpec.Builder(KeyName, KeyStorePurpose.Encrypt))
            {
                var spec = builder
                    .SetBlockModes(KeyProperties.BlockModeCbc)
                    .SetEncryptionPaddings(KeyProperties.EncryptionPaddingPkcs7).Build();
                var keyGenerator = KeyGenerator.GetInstance(KeyProperties.KeyAlgorithmAes, KeyStoreName);
                keyGenerator.Init(spec);
                return keyGenerator.GenerateKey();
            }
        }

        private readonly KeyStore keyStore;

        private string GetPrivateString([CallerMemberName] string key = null) =>
            this.Decrypt(GetValue(p => p.GetString(key, string.Empty)));

        private void SetPrivateString(string value, [CallerMemberName] string key = null)
        {
            SetValue(p => p.PutString(key, this.Encrypt(value)));
            Info("Set {0}.{1} = <private>.", nameof(Settings), key);
        }

        private string Decrypt(string encryptedValue) =>
            Encoding.UTF8.GetString(this.Crypt(Convert.FromBase64String(encryptedValue), CipherMode.DecryptMode));

        private string Encrypt(string value) =>
            Convert.ToBase64String(this.Crypt(Encoding.UTF8.GetBytes(value), CipherMode.EncryptMode));

        private byte[] Crypt(byte[] input, CipherMode mode)
        {
            var transformation = KeyProperties.KeyAlgorithmAes + '/' +
                KeyProperties.BlockModeCbc + '/' + KeyProperties.EncryptionPaddingPkcs7;

            using (var cipher = Cipher.GetInstance(transformation))
            {
                cipher.Init(mode, this.GetKey());
                return cipher.DoFinal(input);
            }
        }

        private IKey GetKey() =>
            this.keyStore.ContainsAlias(KeyName) ? this.keyStore.GetKey(KeyName, null) : GenerateKey();
    }
}