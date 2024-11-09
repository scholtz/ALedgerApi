namespace ALedgerBFFApi.Extensions
{
    public static class NormalizeDateExtension
    {
        /// <summary>
        /// This method converts localized datetimeoffset? to utc while modifying the time
        /// 
        /// Converts
        /// 2000-01-01T13:00:00+01:00 to 2000-01-01T13:00:00+00:00
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static DateTimeOffset? ToUtcPreservingTime(this DateTimeOffset? dateTimeOffset)
        {
            if (!dateTimeOffset.HasValue)
            {
                return null;
            }
            // Convert to UTC offset while keeping the local time components the same
            return new DateTimeOffset(dateTimeOffset.Value.DateTime, TimeSpan.Zero);
        }

        /// <summary>
        /// This method converts localized datetimeoffset to utc while modifying the time
        /// 
        /// Converts
        /// 2000-01-01T13:00:00+01:00 to 2000-01-01T13:00:00+00:00
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static DateTimeOffset ToUtcPreservingTime(this DateTimeOffset dateTimeOffset)
        {
            DateTimeOffset? date = dateTimeOffset;
            return ToUtcPreservingTime(date) ?? throw new Exception("Error occured while converting");
        }

    }
}
