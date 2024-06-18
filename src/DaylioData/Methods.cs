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
                entry => 
                    (entry.FullDate.ToDateTime(TimeOnly.MinValue) > startDate && // Date only comparison
                        entry.FullDate.ToDateTime(TimeOnly.MinValue) < endDate) ||
                    ((entry.FullDate.ToDateTime(TimeOnly.MinValue) == startDate || entry.FullDate.ToDateTime(TimeOnly.MinValue) == endDate) && // Same date, Time comparison
                        entry.Time.IsBetween(startTime, endTime))
            );
        }

        /// <summary>
        /// Gets <see cref="DaylioCSVDataModel"/> entries that include a given activity
        /// </summary>
        /// <param name="activity">An activity string</param>
        /// <returns>An <see cref="IEnumerable{DaylioCSVDataModel}"/> of entries that contain the specified activity.</returns>
        /// <exception cref="ArgumentException"></exception>
        public IEnumerable<DaylioCSVDataModel>? GetEntriesWithActivity(string activity)
        {
            if (string.IsNullOrWhiteSpace(activity) ||
                _daylioData?.DataRepo?.Activities.Where(entry => entry.Equals(activity)).Any() == false)
            {
                throw new ArgumentException("Activity not found in data.");
            }

            return _daylioData?.DataRepo?.CSVData?.Where(entry => entry.ActivitiesCollection
                .Any(entryActivity => entryActivity.Equals(activity, StringComparison.InvariantCultureIgnoreCase)));
        }

        /// <summary>
        /// Gets <see cref="DaylioCSVDataModel"/> entries that have a specified mood.
        /// </summary>
        /// <param name="mood">A mood string</param>
        /// <returns>An <see cref="IEnumerable{DaylioCSVDataModel}"/> of entries that have the specified mood.</returns>
        /// <exception cref="ArgumentException"></exception>
        public IEnumerable<DaylioCSVDataModel>? GetEntriesWithMood(string mood)
        {
            if (string.IsNullOrWhiteSpace(mood) ||
                               _daylioData?.DataRepo?.Moods.Where(entry => entry.Equals(mood)).Any() == false)
            {
                throw new ArgumentException("Mood not found in data.");
            }

            return _daylioData?.DataRepo?.CSVData?.Where(entry => entry.Mood?.Equals(mood, StringComparison.InvariantCultureIgnoreCase) == true);
        }
    }
}
