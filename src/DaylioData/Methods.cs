using DaylioData.Models;

namespace DaylioData
{
    public class Methods
    {

        private static DaylioData? _daylioData;

        public Methods(DaylioData daylioData)
        {
            _daylioData = daylioData;
        }

        /// <summary>
        /// Gets the <see cref="DaylioCSVDataModel"/> with the earliest entry date.
        /// Assumes that <see cref="DaylioData"/> has been initialized, otherwise returns null.
        /// </summary>
        /// <returns>The <see cref="DaylioCSVDataModel"/> with the earliest entry date</returns>
        public DaylioCSVDataModel? GetEarliestEntry()
        {
            return _daylioData?.DataSummary?.EarliestEntry;
        }

        /// <summary>
        /// Gets the <see cref="DaylioCSVDataModel"/> with the earliest entry date.
        /// </summary>
        /// <param name="daylioData">The <see cref="DaylioData"/> instance to use.</param>
        /// <returns>The <see cref="DaylioCSVDataModel"/> with the earliest entry date.</returns>
        public DaylioCSVDataModel? GetEarliestEntry(DaylioData daylioData)
        {
            _daylioData = daylioData;
            return _daylioData?.DataSummary?.EarliestEntry;
        }

        /// <summary>
        /// Gets the <see cref="DaylioCSVDataModel"/> with the latest entry date.
        /// Assumes that <see cref="DaylioData"/> has been initialized, otherwise returns null.
        /// </summary>
        /// <returns>The <see cref="DaylioCSVDataModel"/> with the earliest entry date.</returns>
        public DaylioCSVDataModel? GetLatestEntry()
        {
            return _daylioData?.DataSummary?.LatestEntry;
        }
        
        /// <summary>
        /// Gets the <see cref="DaylioCSVDataModel"/> with the latest entry date.
        /// </summary>
        /// <param name="daylioData">The <see cref="DaylioData"/> instance to use.</param>
        /// <returns>The <see cref="DaylioCSVDataModel"/> with the latest entry date.</returns>
        public DaylioCSVDataModel? GetLatestEntry(DaylioData daylioData)
        {
            _daylioData = daylioData;
            return _daylioData?.DataSummary?.LatestEntry;
        }

        /// <summary>
        /// Gets <see cref="DaylioCSVDataModel"/> entries in a specified date range.
        /// </summary>
        /// <param name="startDate">The earliest (inclusive) <see cref="DateTime"/> of entries.</param>
        /// <param name="endDate">The latest (inclusive) <see cref="DateTime"/> of entries.</param>
        /// <returns>An <see cref="IEnumerable{DaylioCSVDataModel}"/> of entries within the specified date range.</returns>
        public IEnumerable<DaylioCSVDataModel>? GetEntriesInRange(DateTime startDate, DateTime endDate)
        {
            TimeOnly startTime = TimeOnly.FromDateTime(startDate);
            TimeOnly endTime = TimeOnly.FromDateTime(endDate);
            return _daylioData?.DataRepo?.CSVData?.Where
            (
                x => 
                    (x.FullDate.ToDateTime(TimeOnly.MinValue) > startDate && // Date only comparison
                        x.FullDate.ToDateTime(TimeOnly.MinValue) < endDate) ||
                    ((x.FullDate.ToDateTime(TimeOnly.MinValue) == startDate || x.FullDate.ToDateTime(TimeOnly.MinValue) == endDate) && // Same date, Time comparison
                        x.Time.IsBetween(startTime, endTime))
            );
        }
    }
}
