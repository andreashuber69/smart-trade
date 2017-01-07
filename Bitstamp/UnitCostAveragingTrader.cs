////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2016-2017 Andreas Huber Dönni.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Bitstamp
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    using static System.Math;

    /// <summary>Represents a trader that spends a given balance uniformly over a defined amount of time.</summary>
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Temporary, TODO.")]
    public sealed class UnitCostAveragingTrader
    {
        /// <summary>Gets the minimal spendable amount.</summary>
        /// <returns>The smallest value greater than or equal to <paramref name="minAmount"/> such that the fee amounts
        /// to whole cents.</returns>
        public static decimal GetMinSpendableAmount(decimal minAmount, decimal feePercent) =>
            Ceiling(minAmount * feePercent) / feePercent;

        /// <summary>Initializes a new instance of the <see cref="UnitCostAveragingTrader"/> class.</summary>
        /// <param name="endTime">The UTC point in time when the balance should reach zero.</param>
        /// <param name="minAmount">The minimal amount that can be spent.</param>
        /// <param name="feePercent">The fee that will be added to the spent amount in percent.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="minAmount"/> and/or
        /// <paramref name="feePercent"/> are outside of the allowed range.</exception>
        public UnitCostAveragingTrader(DateTime endTime, decimal minAmount, decimal feePercent)
        {
            if (minAmount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(minAmount), "Must not be negative.");
            }

            if (feePercent < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(feePercent), "Must not be negative.");
            }

            this.endTime = endTime;
            this.minAmount = minAmount;
            this.feePercent = feePercent;
        }

        /// <summary>Gets the amount to spend right now.</summary>
        /// <param name="lastTradeTime">The UTC point in time of the last trade.</param>
        /// <param name="currentBalance">The current balance.</param>
        /// <param name="maxAmount">The maximum amount to spend.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="lastTradeTime"/> is greater than the current
        /// time.</exception>
        public decimal GetAmount(DateTime lastTradeTime, decimal currentBalance, decimal maxAmount)
        {
            var elapsed = DateTime.UtcNow - lastTradeTime;

            if (elapsed < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(lastTradeTime), "Time must be in the past.");
            }

            var duration = this.endTime - lastTradeTime;
            var balanceTarget = (1M - ((decimal)elapsed.Ticks / duration.Ticks)) * currentBalance;
            var amountTarget = Min(currentBalance - balanceTarget, Min(maxAmount, currentBalance));
            var amountToSpend = Floor(amountTarget * this.feePercent) / this.feePercent;

            if (amountToSpend < this.MinSpendableAmount)
            {
                amountToSpend = 0;
            }
            else
            {
                if ((currentBalance - amountToSpend) < this.MinSpendableAmount)
                {
                    amountToSpend = currentBalance;
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
                var duration = this.endTime - lastTradeTime;
                var durationTarget = new TimeSpan((long)((1M - (balanceTarget / currentBalance)) * duration.Ticks));
                return lastTradeTime + durationTarget;
            }

            return null;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private readonly DateTime endTime;
        private readonly decimal minAmount;
        private readonly decimal feePercent;

        private decimal MinSpendableAmount => GetMinSpendableAmount(this.minAmount, this.feePercent);
    }
}
