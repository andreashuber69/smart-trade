////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2016-2017 Andreas Huber Dönni.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Bitstamp
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Json;
    using System.Linq;
    using System.Net.Http;
    using System.Runtime.Serialization.Json;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using static System.FormattableString;

    /// <summary>Represents a client for the Bitstamp API.</summary>
    public sealed partial class BitstampClient : IDisposable
    {
        /// <summary>Initializes a new instance of the <see cref="BitstampClient"/> class.</summary>
        /// <remarks>An instance initialized with this constructor can be used to access the public API only.</remarks>
        public BitstampClient()
        {
            this.BtcEur = new BtcEurExchange(this);
        }

        /// <summary>Initializes a new instance of the <see cref="BitstampClient"/> class.</summary>
        /// <remarks>An instance initialized with this constructor can be used to access the public and the private
        /// API.</remarks>
        public BitstampClient(int customerId, string apiKey, string apiSecret)
            : this()
        {
            if (customerId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(customerId), "Positive number required.");
            }

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new ArgumentException("Invalid key.", nameof(apiKey));
            }

            if (string.IsNullOrWhiteSpace(apiSecret))
            {
                throw new ArgumentException("Invalid secret.", nameof(apiSecret));
            }

            this.customerId = customerId;
            this.apiKey = apiKey;
            this.sha256 = new HMACSHA256(Encoding.ASCII.GetBytes(apiSecret));
        }

        /// <summary>Gets the exchange for BTCEUR.</summary>
        public ICurrencyExchange BtcEur { get; }

        /// <summary>Releases all resources used by the <see cref="BitstampClient"/>.</summary>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Dispose must never throw.")]
        public void Dispose()
        {
            try
            {
                if (this.sha256 != null)
                {
                    this.sha256.Dispose();
                }

                this.httpClient.Dispose();
            }
            catch
            {
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Long-running operation.")]
        internal Task<Balance> GetBalanceAsync() =>
            this.ExecutePostAsync<Balance>("/api/v2/balance/", Enumerable.Empty<KeyValuePair<string, string>>());

        internal Task<IReadOnlyList<Transaction>> GetTransactionsAsync() => this.GetTransactionsAsync(0);

        internal Task<IReadOnlyList<Transaction>> GetTransactionsAsync(int offset) => this.GetTransactionsAsync(offset, 100);

        internal async Task<IReadOnlyList<Transaction>> GetTransactionsAsync(int offset, int limit)
        {
            var args =
                new Dictionary<string, string>()
                {
                    { "offset", ToString(offset) },
                    { "limit", ToString(limit) }
                };

            return await this.ExecutePostAsync<TransactionCollection>("/api/v2/user_transactions/", args);
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Long-running operation.")]
        internal Task<OrderBook> GetOrderBookAsync(string tickerSymbol) =>
            this.ExecuteGetAsync<OrderBook>(Invariant($"/api/v2/order_book/{tickerSymbol}/"));

        internal Task<PrivateOrder> CreateBuyOrderAsync(string currencyPair, decimal amount, decimal price) =>
            this.CreateBuyOrderAsync(currencyPair, amount, price, null);

        internal Task<PrivateOrder> CreateBuyOrderAsync(
            string tickerSymbol, decimal amount, decimal price, decimal? limitPrice)
        {
            var args =
                new Dictionary<string, string>()
                {
                    { "amount", ToString(amount) },
                    { "price", ToString(price) },
                    { "limit_price", ToString(limitPrice) }
                };

            return this.ExecutePostAsync<PrivateOrder>(Invariant($"/api/v2/buy/{tickerSymbol}/"), args);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private const string BaseUri = "https://www.bitstamp.net";
        private static readonly DataContractJsonSerializerSettings SerializerSettings =
            new DataContractJsonSerializerSettings() { DataContractSurrogate = new OrderSurrogate() };

        private static long nonce = DateTime.UtcNow.Ticks;

        private static string ToString(IFormattable value) => value?.ToString(null, CultureInfo.InvariantCulture);

        private static long GetNonce() => Interlocked.Increment(ref nonce);

        private static async Task<T> GetResponseBodyAsync<T>(HttpResponseMessage response)
        {
            using (var memoryStream = new MemoryStream())
            {
                await response.Content.CopyToAsync(memoryStream);
                var json = JsonValue.Load(memoryStream);

                if (json["Status"] == "error")
                {
                    throw new BitstampException(json["Reason"].ToString());
                }

                response.EnsureSuccessStatusCode();
                memoryStream.Position = 0;
                return DeserializeJson<T>(memoryStream);
            }
        }

        private static T DeserializeJson<T>(Stream stream) =>
            (T)new DataContractJsonSerializer(typeof(T), SerializerSettings).ReadObject(stream);

        private readonly HttpClient httpClient = new HttpClient();
        private readonly int customerId;
        private readonly string apiKey;
        private readonly HMACSHA256 sha256;

        private async Task<T> ExecuteGetAsync<T>(string uri)
        {
            using (var response = await this.httpClient.GetAsync(BaseUri + uri))
            {
                return await GetResponseBodyAsync<T>(response);
            }
        }

        private async Task<T> ExecutePostAsync<T>(string uri, IEnumerable<KeyValuePair<string, string>> args)
        {
            var nonce = GetNonce();
            var signatureArgs =
                new Dictionary<string, string>()
                {
                    { "key", this.apiKey },
                    { "signature", this.GetSignature(nonce) },
                    { "nonce", ToString(nonce) }
                };

            var allArgs = signatureArgs.Concat(args);

            using (var response = await this.httpClient.PostAsync(BaseUri + uri, new FormUrlEncodedContent(allArgs)))
            {
                return await GetResponseBodyAsync<T>(response);
            }
        }

        private string GetSignature(long nonce)
        {
            if (this.sha256 == null)
            {
                throw new InvalidOperationException("The private API cannot be accessed with this instance.");
            }

            FormattableString message = $"{nonce}{this.customerId}{this.apiKey}";
            var hash = this.sha256.ComputeHash(Encoding.ASCII.GetBytes(Invariant(message)));
            return BitConverter.ToString(hash).Replace("-", string.Empty).ToUpperInvariant();
        }
    }
}
