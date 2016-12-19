namespace SmartTrade
{
    using System;

    internal sealed class UtcTimestamper
    {
        private readonly DateTime startTime = DateTime.UtcNow;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        internal string GetFormattedTimestamp()
        {
            TimeSpan duration = DateTime.UtcNow.Subtract(startTime);
            return $"Service started at {startTime} ({duration:c} ago).";
        }
    }
}
