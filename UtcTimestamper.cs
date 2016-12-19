namespace SmartTrade
{
    using System;

    internal sealed class UtcTimestamper
    {
        private readonly DateTime startTime = DateTime.UtcNow;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        internal string GetFormattedTimestamp() =>
            $"Service started at {startTime} ({DateTime.UtcNow.Subtract(startTime):c} ago).";
    }
}
