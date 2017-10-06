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
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using static System.FormattableString;

    /// <summary>Represents a client for the Bitstamp API.</summary>
    public sealed partial class BitstampClient : IDisposable
    {
        /// <summary>Gets the BTC/EUR ticker symbol.</summary>
        public static string BtcEurSymbol => "BTC/EUR";

        /// <summary>Gets the ticker symbol for BTC/EUR.</summary>
        public static IReadOnlyList<string> TickerSymbols { get; } = new[] { BtcEurSymbol };

        /// <summary>Initializes a new instance of the <see cref="BitstampClient"/> class.</summary>
        /// <remarks>An instance initialized with this constructor can be used to access the public API only.</remarks>
        public BitstampClient()
        {
            this.Exchanges =
                new Dictionary<string, ICurrencyExchange>()
                {
                    { BtcEurSymbol, new BtcEurExchange(this) }
                };
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

        /// <summary>Gets the supported currency exchanges.</summary>
        public IReadOnlyDictionary<string, ICurrencyExchange> Exchanges { get; }

        /// <summary>Releases all resources used by the <see cref="BitstampClient"/>.</summary>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Dispose must never throw.")]
        public void Dispose()
        {
            try
            {
                this.sha256?.Dispose();
                this.httpClient.Dispose();
            }
            catch
            {
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Long-running operation.")]
        internal Task<Balance> GetBalanceAsync() => this.ExecutePostAsync(
            "/api/v2/balance/", Enumerable.Empty<KeyValuePair<string, string>>(), d => new Balance(d));

        internal async Task<IReadOnlyList<Transaction>> GetTransactionsAsync(int offset, int limit)
        {
            var args =
                new Dictionary<string, string>() { { "offset", ToString(offset) }, { "limit", ToString(limit) } };
            return await this.ExecutePostAsync("/api/v2/user_transactions/", args, d => new TransactionCollection(d));
        }

        internal Task<Ticker> GetTickerAsync(string currencyPair) =>
            this.ExecuteGetAsync(Invariant($"/api/v2/ticker/{currencyPair}/"), d => new Ticker(d));

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Long-running operation.")]
        internal Task<OrderBook> GetOrderBookAsync(string currencyPair) =>
            this.ExecuteGetAsync(Invariant($"/api/v2/order_book/{currencyPair}/"), d => new OrderBook(d));

        internal Task<PrivateOrder> CreateBuyOrderAsync(string currencyPair, decimal amount, decimal price) =>
            this.CreateBuyOrderAsync(currencyPair, amount, price, null);

        internal Task<PrivateOrder> CreateBuyOrderAsync(
            string currencyPair, decimal amount, decimal price, decimal? limitPrice)
        {
            return this.CreateOrderAsync("buy", currencyPair, amount, price, limitPrice);
        }

        internal Task<PrivateOrder> CreateBuyOrderAsync(string currencyPair, decimal amount) =>
            this.CreateOrderAsync("buy", currencyPair, amount);

        internal Task<PrivateOrder> CreateSellOrderAsync(string currencyPair, decimal amount, decimal price) =>
            this.CreateSellOrderAsync(currencyPair, amount, price, null);

        internal Task<PrivateOrder> CreateSellOrderAsync(
            string currencyPair, decimal amount, decimal price, decimal? limitPrice)
        {
            return this.CreateOrderAsync("sell", currencyPair, amount, price, limitPrice);
        }

        internal Task<PrivateOrder> CreateSellOrderAsync(string currencyPair, decimal amount) =>
            this.CreateOrderAsync("sell", currencyPair, amount);

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private const string BaseUri = "https://www.bitstamp.net";

        private static long nonce = DateTime.UtcNow.Ticks;

        private static string ToString(IFormattable value) => value?.ToString(null, CultureInfo.InvariantCulture);

        private static long GetNonce() => Interlocked.Increment(ref nonce);

        private static async Task<T> GetResponseBodyAsync<T>(
            HttpResponseMessage response, Func<JsonValue, T> createResult)
        {
            using (var memoryStream = new MemoryStream())
            {
                try
                {
                    await response.Content.CopyToAsync(memoryStream);
                }
                catch (ObjectDisposedException ex)
                {
                    throw new HttpRequestException("Failed to retrieve response.", ex);
                }

                memoryStream.Position = 0;
                var data = JsonValue.Load(memoryStream);
                const string StatusName = "status";

                if ((data is JsonObject) && data.ContainsKey(StatusName) && (data[StatusName] == "error"))
                {
                    throw new BitstampException(data["reason"].ToString());
                }

                response.EnsureSuccessStatusCode();
                return createResult(data);
            }
        }

        private readonly HttpClient httpClient = new HttpClient();
        private readonly int customerId;
        private readonly string apiKey;
        private readonly HMACSHA256 sha256;

        private Task<PrivateOrder> CreateOrderAsync(
            string command, string tickerSymbol, decimal amount, decimal price, decimal? limitPrice)
        {
            var args =
                new Dictionary<string, string>()
                {
                    { "amount", ToString(amount) },
                    { "price", ToString(price) },
                    { "limit_price", ToString(limitPrice) }
                };

            return this.ExecutePostAsync(
                Invariant($"/api/v2/{command}/{tickerSymbol}/"), args, d => new PrivateOrder(d));
        }

        private Task<PrivateOrder> CreateOrderAsync(string command, string tickerSymbol, decimal amount)
        {
            var args = new Dictionary<string, string>() { { "amount", ToString(amount) } };
            return this.ExecutePostAsync(
                Invariant($"/api/v2/{command}/market/{tickerSymbol}/"), args, d => new PrivateOrder(d));
        }

        private async Task<T> ExecuteGetAsync<T>(string uri, Func<JsonValue, T> createResult)
        {
            using (var response = await this.httpClient.GetAsync(BaseUri + uri))
            {
                return await GetResponseBodyAsync<T>(response, createResult);
            }
        }

        private async Task<T> ExecutePostAsync<T>(
            string uri, IEnumerable<KeyValuePair<string, string>> args, Func<JsonValue, T> createResult)
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
                return await GetResponseBodyAsync(response, createResult);
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
