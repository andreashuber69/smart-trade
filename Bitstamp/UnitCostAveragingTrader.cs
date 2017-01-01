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
        /// <param name="initialBalanceTime">The UTC point in time of the initial balance.</param>
        /// <param name="initialBalance">The initial balance.</param>
        /// <param name="duration">The duration over which to spend, such that the balance is zero when the current time
        /// equals <paramref name="initialBalanceTime"/> + <paramref name="duration"/>.</param>
        /// <param name="minAmount">The minimal amount that can be spent.</param>
        /// <param name="feePercent">The fee that will be added to the spent amount in percent.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="initialBalanceTime"/>,
        /// <paramref name="duration"/>, <paramref name="minAmount"/> and/or <paramref name="feePercent"/> are
        /// outside of the allowed range.</exception>
        public UnitCostAveragingTrader(
            DateTime initialBalanceTime,
            decimal initialBalance,
            TimeSpan duration,
            decimal minAmount,
            decimal feePercent)
        {
            if (initialBalanceTime >= DateTime.UtcNow)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(initialBalanceTime), "Must be less than the current time.");
            }

            if (duration <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(duration), "Must be positive.");
            }

            if (minAmount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(minAmount), "Must not be negative.");
            }

            if (feePercent < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(feePercent), "Must not be negative.");
            }

            this.initialBalanceTime = initialBalanceTime;
            this.initialBalance = initialBalance;
            this.duration = duration;
            this.minAmount = minAmount;
            this.feePercent = feePercent;
        }

        /// <summary>Gets the amount to spend right now.</summary>
        /// <param name="currentBalance">The current balance.</param>
        /// <param name="maxAmount">The maximum amount to spend.</param>
        public decimal GetAmount(decimal currentBalance, decimal maxAmount)
        {
            var elapsed = DateTime.UtcNow - this.initialBalanceTime;
            var balanceTarget = (1M - ((decimal)elapsed.Ticks / this.duration.Ticks)) * this.initialBalance;
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

        /// <summary>Gets the amount with the fee subtracted.</summary>
        public decimal SubtractFee(decimal amount) => amount * (1M - (this.feePercent / 100));

        /// <summary>Gets the UTC time at which <see cref="GetAmount"/> can return a non-zero number.</summary>
        /// <param name="currentBalance">The current balance.</param>
        /// <returns>The point in time <see cref="GetAmount"/> should be called; or, <c>null</c> if no such
        /// point exists (e.g. if the current balance is already below the minimal amount).</returns>
        public DateTime? GetNextTime(decimal currentBalance)
        {
            if (currentBalance >= this.MinSpendableAmount)
            {
                var balanceTarget = currentBalance - this.MinSpendableAmount;
                var durationTarget =
                    new TimeSpan((long)((1M - (balanceTarget / this.initialBalance)) * this.duration.Ticks));
                return this.initialBalanceTime + durationTarget;
            }

            return null;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private readonly DateTime initialBalanceTime;
        private readonly decimal initialBalance;
        private readonly TimeSpan duration;
        private readonly decimal minAmount;
        private readonly decimal feePercent;

        private decimal MinSpendableAmount => GetMinSpendableAmount(this.minAmount, this.feePercent);
    }
}
