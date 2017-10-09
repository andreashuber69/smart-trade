﻿////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
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
            this.minTradeAmount = minTradeAmount;
            this.feeStepsPerUnit = feePercent / 100m / feeStep;
            this.feeStep = feeStep;
        }

        /// <summary>Gets the minimal optimal trade amount.</summary>
        /// <returns>The smallest value greater than or equal to the minimal trade amount such that the fee amounts
        /// to a whole number of fee steps.</returns>
        public decimal MinOptimalTradeAmount =>
            Ceiling(this.minTradeAmount * this.feeStepsPerUnit) / this.feeStepsPerUnit;

        /// <summary>Gets the amount to trade right now.</summary>
        /// <param name="startTime">The UTC time where unit cost averaging should start.</param>
        /// <param name="startBalance">The balance at the point in time represented by <paramref name="startTime"/>.
        /// </param>
        /// <param name="maxTradeAmount">The maximum amount to trade.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="startTime"/> is greater than the current
        /// time.</exception>
        public decimal GetTradeAmount(DateTime startTime, decimal startBalance, decimal maxTradeAmount)
        {
            var elapsed = DateTime.UtcNow - startTime;

            if (elapsed < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(startTime), "Time must be in the past.");
            }

            var duration = this.periodEnd - startTime;
            var targetBalance = (1M - ((decimal)elapsed.Ticks / duration.Ticks)) * startBalance;
            var targetAmount = Min(startBalance - targetBalance, Min(maxTradeAmount, startBalance));
            var tradeAmount = Floor(targetAmount * this.feeStepsPerUnit) / this.feeStepsPerUnit;

            if (tradeAmount < this.MinOptimalTradeAmount)
            {
                tradeAmount = 0m;
            }
            else
            {
                if ((startBalance - tradeAmount) < this.MinOptimalTradeAmount)
                {
                    tradeAmount = startBalance;
                }
            }

            return tradeAmount;
        }

        /// <summary>Gets the trading fee for <paramref name="tradeAmount"/>.</summary>
        public decimal GetFee(decimal tradeAmount) => Ceiling(tradeAmount * this.feeStepsPerUnit) * this.feeStep;

        /// <summary>Gets the UTC time at which <see cref="GetTradeAmount"/> can return a non-zero number.</summary>
        /// <param name="lastTradeTime">The UTC point in time of the last trade.</param>
        /// <param name="currentBalance">The current balance.</param>
        /// <returns>The point in time <see cref="GetTradeAmount"/> should be called; or, <c>null</c> if no such
        /// point exists (e.g. if the current balance is already below the minimal amount).</returns>
        public DateTime? GetNextTime(DateTime lastTradeTime, decimal currentBalance)
        {
            if (currentBalance >= this.MinOptimalTradeAmount)
            {
                var targetBalance = currentBalance - this.MinOptimalTradeAmount;
                var duration = this.periodEnd - lastTradeTime;
                var durationTarget = new TimeSpan((long)((1M - (targetBalance / currentBalance)) * duration.Ticks));
                return lastTradeTime + durationTarget;
            }

            return null;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private readonly DateTime periodEnd;
        private readonly decimal minTradeAmount;
        private readonly decimal feeStepsPerUnit;
        private readonly decimal feeStep;
    }
}
