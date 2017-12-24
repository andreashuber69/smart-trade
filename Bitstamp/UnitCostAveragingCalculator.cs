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
            this.minOptimalTradeAmount = Ceiling(minTradeAmount * this.feeStepsPerUnit) / this.feeStepsPerUnit;
        }

        /// <summary>Gets the amount to trade right now.</summary>
        /// <param name="startTime">The UTC time where unit cost averaging should start.</param>
        /// <param name="startBalance">The balance at the point in time represented by <paramref name="startTime"/>.
        /// </param>
        /// <param name="maxTradeAmount">The maximum amount to trade.</param>
        /// <returns>The amount to trade right now. The amount is <c>null</c>, if <paramref name="startBalance"/> is
        /// smaller than the minimal optimal trade amount. The amount is <c>0</c>, if there's nothing to trade yet, but
        /// there will be something to trade later.</returns>
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

            if (tradeAmount < this.minOptimalTradeAmount)
            {
                tradeAmount = 0m;
            }
            else
            {
                if (this.IsLastTrade(startBalance, tradeAmount))
                {
                    tradeAmount = startBalance;
                }
            }

            return tradeAmount;
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

        private readonly DateTime periodEnd;
        private readonly decimal feeStep;
        private readonly decimal feeStepsPerUnit;
        private readonly decimal minOptimalTradeAmount;
    }
}
