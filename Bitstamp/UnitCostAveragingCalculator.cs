////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2016-2017 Andreas Huber Dönni.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Bitstamp
{
    using System;

    using static System.Math;

    /// <summary>Provides methods to calculate how much an when to trade such that a given balance will be spent
    /// uniformly over a given period of time.</summary>
    /// <remarks>A traded amount refers to the amount actually traded. A spent amount refers to the amount traded +
    /// exchange fees.</remarks>
    public sealed class UnitCostAveragingCalculator
    {
        /// <summary>Gets the minimal spendable amount.</summary>
        /// <returns>The smallest value greater than or equal to <paramref name="minAmount"/> such that the fee amounts
        /// to whole cents.</returns>
        public static decimal GetMinSpendableAmount(decimal minAmount, decimal feePercent) =>
            Ceiling(minAmount * feePercent) / feePercent;

        /// <summary>Initializes a new instance of the <see cref="UnitCostAveragingCalculator"/> class.</summary>
        /// <param name="periodEnd">The UTC point in time when the balance should reach zero.</param>
        /// <param name="minAmount">The minimal amount that can be traded.</param>
        /// <param name="feePercent">The fee that will be added to the traded amount in percent.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="minAmount"/> and/or
        /// <paramref name="feePercent"/> are outside of the allowed range.</exception>
        public UnitCostAveragingCalculator(DateTime periodEnd, decimal minAmount, decimal feePercent)
        {
            if (minAmount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(minAmount), "Must not be negative.");
            }

            if (feePercent < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(feePercent), "Must not be negative.");
            }

            this.periodEnd = periodEnd;
            this.minAmount = minAmount;
            this.feePercent = feePercent;
        }

        /// <summary>Gets the amount to spend right now.</summary>
        /// <param name="startTime">The UTC time where unit cost averaging should start.</param>
        /// <param name="startBalance">The balance at the point in time represented by <paramref name="startTime"/>.
        /// </param>
        /// <param name="maxAmount">The maximum amount to spend.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="startTime"/> is greater than the current
        /// time.</exception>
        public decimal GetAmount(DateTime startTime, decimal startBalance, decimal maxAmount)
        {
            var elapsed = DateTime.UtcNow - startTime;

            if (elapsed < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(startTime), "Time must be in the past.");
            }

            var duration = this.periodEnd - startTime;
            var balanceTarget = (1M - ((decimal)elapsed.Ticks / duration.Ticks)) * startBalance;
            var amountTarget = Min(startBalance - balanceTarget, Min(maxAmount, startBalance));
            var amountToSpend = Floor(amountTarget * this.feePercent) / this.feePercent;

            if (amountToSpend < this.MinSpendableAmount)
            {
                amountToSpend = 0;
            }
            else
            {
                if ((startBalance - amountToSpend) < this.MinSpendableAmount)
                {
                    amountToSpend = startBalance;
                }
            }

            return amountToSpend;
        }

        /// <summary>Gets the trading fee for <paramref name="amount"/>.</summary>
        public decimal GetFee(decimal amount) => Ceiling(amount * this.feePercent) / 100;

        /// <summary>Gets the UTC time at which <see cref="GetAmount"/> can return a non-zero number.</summary>
        /// <param name="lastTradeTime">The UTC point in time of the last trade.</param>
        /// <param name="currentBalance">The current balance.</param>
        /// <returns>The point in time <see cref="GetAmount"/> should be called; or, <c>null</c> if no such
        /// point exists (e.g. if the current balance is already below the minimal amount).</returns>
        public DateTime? GetNextTime(DateTime lastTradeTime, decimal currentBalance)
        {
            if (currentBalance >= this.MinSpendableAmount)
            {
                var balanceTarget = currentBalance - this.MinSpendableAmount;
                var duration = this.periodEnd - lastTradeTime;
                var durationTarget = new TimeSpan((long)((1M - (balanceTarget / currentBalance)) * duration.Ticks));
                return lastTradeTime + durationTarget;
            }

            return null;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private readonly DateTime periodEnd;
        private readonly decimal minAmount;
        private readonly decimal feePercent;

        private decimal MinSpendableAmount => GetMinSpendableAmount(this.minAmount, this.feePercent);
    }
}
