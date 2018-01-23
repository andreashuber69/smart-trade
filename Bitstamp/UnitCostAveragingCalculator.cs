////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2016-2017 Andreas Huber Dönni.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Bitstamp
{
    using System;

    using static System.Math;

    /// <summary>Provides methods to calculate how much and when to trade such that a given balance will be traded
    /// uniformly over a given period of time.</summary>
    public sealed class UnitCostAveragingCalculator
    {
        /// <summary>Initializes a new instance of the <see cref="UnitCostAveragingCalculator"/> class.</summary>
        /// <param name="periodEnd">The UTC point in time when the balance should reach zero.</param>
        /// <param name="minTradeAmount">The minimal amount that can be traded.</param>
        /// <param name="feePercent">The fee that will be charged in percent of the traded amount.</param>
        /// <param name="feeStep">The smallest amount the trading fee can be incremented by.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="minTradeAmount"/> and/or
        /// <paramref name="feePercent"/> and/or <paramref name="feeStep"/> are outside of the allowed range.
        /// </exception>
        public UnitCostAveragingCalculator(
            DateTime periodEnd, decimal minTradeAmount, decimal feePercent, decimal feeStep)
        {
            if (minTradeAmount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(minTradeAmount), "Must not be negative.");
            }

            if (feePercent < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(feePercent), "Must not be negative.");
            }

            if (feeStep <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(feeStep), "Must be positive.");
            }

            this.periodEnd = periodEnd;
            this.feeStep = feeStep;
            this.feeStepsPerUnit = feePercent / 100m / feeStep;
            this.minOptimalTradeAmount =
                Ceiling(minTradeAmount * this.feeStepsPerUnit) / this.feeStepsPerUnit * FeeOptimizationFactor;
        }

        /// <summary>Gets the amount to trade right now.</summary>
        /// <param name="startTime">The UTC time where unit cost averaging should start.</param>
        /// <param name="startBalance">The balance at the point in time represented by <paramref name="startTime"/>.
        /// </param>
        /// <param name="maxTradeAmount">The maximum amount to trade.</param>
        /// <returns>The amount to trade right now. The amount is <c>null</c>, if <paramref name="startBalance"/> is
        /// smaller than the minimal optimal trade amount.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="startTime"/> is greater than the current
        /// time.</exception>
        public decimal? GetTradeAmount(DateTime startTime, decimal startBalance, decimal maxTradeAmount)
        {
            if (startBalance < this.minOptimalTradeAmount)
            {
                return null;
            }

            var elapsed = DateTime.UtcNow - startTime;

            if (elapsed < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(startTime), "Time must be in the past.");
            }

            var duration = this.periodEnd - startTime;
            var targetBalance = (1M - ((decimal)elapsed.Ticks / duration.Ticks)) * startBalance;
            var targetAmount = Min(startBalance - targetBalance, Min(maxTradeAmount, startBalance));
            var tradeAmount = Floor(targetAmount * this.feeStepsPerUnit) / this.feeStepsPerUnit;

            if (this.IsLastTrade(startBalance, tradeAmount))
            {
                tradeAmount = startBalance;
            }

            return Max(this.minOptimalTradeAmount, tradeAmount * FeeOptimizationFactor);
        }

        /// <summary>Gets a value indicating whether a trade with <paramref name="tradeAmount"/> would be the last
        /// trade.</summary>
        public bool IsLastTrade(decimal startBalance, decimal tradeAmount) =>
            (startBalance - tradeAmount) < this.minOptimalTradeAmount;

        /// <summary>Gets the trading fee for <paramref name="tradeAmount"/>.</summary>
        public decimal GetFee(decimal tradeAmount) => Ceiling(tradeAmount * this.feeStepsPerUnit) * this.feeStep;

        /// <summary>Gets the UTC time at which <see cref="GetTradeAmount"/> can return a non-zero number.</summary>
        /// <param name="lastTradeTime">The UTC point in time of the last trade.</param>
        /// <param name="currentBalance">The current balance.</param>
        /// <returns>The point in time <see cref="GetTradeAmount"/> should be called; or, <c>null</c> if no such
        /// point exists (e.g. if the current balance is already below the minimal amount).</returns>
        public DateTime? GetNextTime(DateTime lastTradeTime, decimal currentBalance)
        {
            if (currentBalance >= this.minOptimalTradeAmount)
            {
                var targetBalance = currentBalance - this.minOptimalTradeAmount;
                var duration = this.periodEnd - lastTradeTime;
                var durationTarget = new TimeSpan((long)((1M - (targetBalance / currentBalance)) * duration.Ticks));
                return lastTradeTime + durationTarget;
            }

            return null;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        // When we trade on Bitstamp, the fee is calculated as implemented in GetFee. The fee is charged in discrete
        // steps (e.g. 0.01 for fiat and 0.00001 for BTC) and always rounded up to the next step. We therefore always
        // want to trade a bit less, otherwise we end up paying a fee step more than necessary.
        // Since the market can move between the time we query the price and the time our trade is executed, we cannot
        // just subtract a constant amount (like e.g. 0.001, as we did in tests). In general, we need to lower the
        // amount such that the average total paid to trade a given amount is as low as possible. The average total is
        // higher than optimal because a) additional trades need to be made due to the lowered per trade amount and
        // b) occasionly the amount traded goes over the fee threshold due to the moving market.
        // Examples:
        // - If we lowered the trade amount by just one satoshi, we would expect that roughly half of the
        // trades pay higher fees than intended. With a 0.25% fee, for a goal of buying EUR 8000 worth of
        // BTC we'd thus end up with 500 EUR 8 trades paying 3 cents in fees and 500 EUR 8 trades paying 2
        // cents in fees. We'd therefore pay EUR 8025 for EUR 8000 worth of BTC.
        // - If we lowered the amount per trade by 0.1%, we end up having to put in 1001 EUR 7.992 trades.
        // If that reduced the number of trades going over the threshold to 20%, we'd get 200 trades paying
        // 3 cents in fees and 801 trades paying 2 cents in fees. We'd therefore pay ~EUR 8022 for EUR 8000
        // worth of BTC.
        // We therefore need to lower the per trade amount such that the fees paid for the additional number
        // of trades *and* the fees paid for the trades that go over the fee threshold reaches a minimum.
        // Tests with 0.6% resulted in more than 1% of the trades still going over the threshold, which is
        // why we try with 1% for now.
        private const decimal FeeOptimizationFactor = .99m;

        private readonly DateTime periodEnd;
        private readonly decimal feeStep;
        private readonly decimal feeStepsPerUnit;
        private readonly decimal minOptimalTradeAmount;
    }
}
