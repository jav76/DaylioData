using DaylioData.Models;
using System.Runtime.InteropServices;

namespace DaylioData
{
    public static class Methods
    {
        private static DaylioData? _daylioData;

        public static void InitData(DaylioData daylioData)
        {
            _daylioData = daylioData;
        }

        /// <summary>
        /// Gets the <see cref="DaylioCSVDataModel"/> with the earliest entry date.
        /// Assumes that <see cref="DaylioData"/> has been initialized, otherwise returns null.
        /// </summary>
        /// <returns>The <see cref="DaylioCSVDataModel"/> with the earliest entry date</returns>
        public static DaylioCSVDataModel? GetEarliestEntry()
        {
            return _daylioData?.DataSummary?.EarliestEntry;
        }

        /// <summary>
        /// Gets the <see cref="DaylioCSVDataModel"/> with the earliest entry date.
        /// </summary>
        /// <param name="daylioData">The <see cref="DaylioData"/> instance to use.</param>
        /// <returns>The <see cref="DaylioCSVDataModel"/> with the earliest entry date.</returns>
        public static DaylioCSVDataModel? GetEarliestEntry(DaylioData daylioData)
        {
            InitData(daylioData);
            return GetEarliestEntry();
        }

        /// <summary>
        /// Gets the <see cref="DaylioCSVDataModel"/> with the latest entry date.
        /// Assumes that <see cref="DaylioData"/> has been initialized, otherwise returns null.
        /// </summary>
        /// <returns>The <see cref="DaylioCSVDataModel"/> with the earliest entry date.</returns>
        public static DaylioCSVDataModel? GetLatestEntry()
        {
            return _daylioData?.DataSummary?.LatestEntry;
        }
        
        /// <summary>
        /// Gets the <see cref="DaylioCSVDataModel"/> with the latest entry date.
        /// </summary>
        /// <param name="daylioData">The <see cref="DaylioData"/> instance to use.</param>
        /// <returns>The <see cref="DaylioCSVDataModel"/> with the latest entry date.</returns>
        public static DaylioCSVDataModel? GetLatestEntry(DaylioData daylioData)
        {
            InitData(daylioData);
            return GetLatestEntry();
        }

        /// <summary>
        /// Gets <see cref="DaylioCSVDataModel"/> entries in a specified date range.
        /// Assumes that <see cref="DaylioData"/> has been initialized, otherwise returns null.
        /// </summary>
        /// <param name="startDate">The earliest (inclusive) <see cref="DateTime"/> of entries.</param>
        /// <param name="endDate">The latest (inclusive) <see cref="DateTime"/> of entries.</param>
        /// <returns>An <see cref="IEnumerable{DaylioCSVDataModel}"/> of entries within the specified date range.</returns>
        public static IEnumerable<DaylioCSVDataModel>? GetEntriesInRange(DateTime startDate, DateTime endDate)
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
        /// Gets <see cref="DaylioCSVDataModel"/> entries in a specified date range.
        /// </summary>
        /// <param name="daylioData">The <see cref="DaylioData"/> instance to use.</param>
        /// <param name="startDate">The earliest (inclusive) <see cref="DateTime"/> of entries.</param>
        /// <param name="endDate">The latest (inclusive) <see cref="DateTime"/> of entries.</param>
        /// <returns>An <see cref="IEnumerable{DaylioCSVDataModel}"/> of entries within the specified date range.</returns>
        public static IEnumerable<DaylioCSVDataModel>? GetEntriesInRange(DaylioData daylioData, DateTime startDate, DateTime endDate)
        {
            InitData(daylioData);
            return GetEntriesInRange(startDate, endDate);
        }

        /// <summary>
        /// Gets <see cref="DaylioCSVDataModel"/> entries that include a specified activity
        /// Assumes that <see cref="DaylioData"/> has been initialized, otherwise returns null.
        /// </summary>
        /// <param name="activity">An activity string</param>
        /// <returns>An <see cref="IEnumerable{DaylioCSVDataModel}"/> of entries that contain the specified activity.</returns>
        public static IEnumerable<DaylioCSVDataModel>? GetEntriesWithActivity(string activity)
        {
            if (string.IsNullOrWhiteSpace(activity) ||
                _daylioData?.DataRepo?.Activities.Where(entry => entry.Equals(activity)).Any() == false)
            {
                return null;
            }

            return _daylioData?.DataRepo?.CSVData?.Where(entry => entry.ActivitiesCollection
                .Any(entryActivity => entryActivity.Equals(activity, StringComparison.InvariantCultureIgnoreCase)));
        }

        /// <summary>
        /// Gets <see cref="DaylioCSVDataModel"/> entries that include a specified activity
        /// </summary>
        /// <param name="daylioData">The <see cref="DaylioData"/> instance to use.</param>
        /// <param name="activity">An activity string</param>
        /// <returns>An <see cref="IEnumerable{DaylioCSVDataModel}"/> of entries that contain the specified activity.</returns>
        public static IEnumerable<DaylioCSVDataModel>? GetEntriesWithActivity(DaylioData daylioData,string activity)
        {
            InitData(daylioData);
            return GetEntriesWithActivity(activity);
        }

        /// <summary>
        /// Gets <see cref="DaylioCSVDataModel"/> entries that have a specified mood.
        /// Assumes that <see cref="DaylioData"/> has been initialized, otherwise returns null.
        /// </summary>
        /// <param name="mood">A mood string</param>
        /// <returns>An <see cref="IEnumerable{DaylioCSVDataModel}"/> of entries that have the specified mood.</returns>
        public static IEnumerable<DaylioCSVDataModel>? GetEntriesWithMood(string mood)
        {
            if (string.IsNullOrWhiteSpace(mood) ||
                _daylioData?.DataRepo?.Moods.Where(entry => entry.Equals(mood)).Any() == false)
            {
                return null;
            }

            return _daylioData?.DataRepo?.CSVData?.Where(entry => entry.Mood?.Equals(mood, StringComparison.InvariantCultureIgnoreCase) == true);
        }

        /// <summary>
        /// Gets <see cref="DaylioCSVDataModel"/> entries that have a specified mood.
        /// </summary>
        /// <param name="daylioData">The <see cref="DaylioData"/> instance to use.</param>
        /// <param name="mood">A mood string</param>
        /// <returns>An <see cref="IEnumerable{DaylioCSVDataModel}"/> of entries that have the specified mood.</returns>
        public static IEnumerable<DaylioCSVDataModel>? GetEntriesWithMood(DaylioData daylioData, string mood)
        {
            InitData(daylioData);
            return GetEntriesWithMood(mood);
        }

        /// <summary>
        /// Gets the number of entries that include a specified activity.
        /// Assumes that <see cref="DaylioData"/> has been initialized, otherwise returns null.
        /// </summary>
        /// <param name="activity">An activity string</param>
        /// <returns>The <see cref="int"/> number of activities that include a specified activity.</returns>
        public static int? GetActivityCount(string activity)
        {
            return _daylioData?.DataRepo?.CSVData?.Count(entry => entry.ActivitiesCollection
                .Any(entryActivity => entryActivity.Equals(activity, StringComparison.InvariantCultureIgnoreCase)));
        }

        /// <summary>
        /// Gets the number of entries that include a specified activity.
        /// </summary>
        /// <param name="activity">An activity string</param>
        /// <returns>The <see cref="int"/> number of activities that include a specified activity.</returns>
        public static int? GetActivityCount(DaylioData daylioData, string activity)
        {
            InitData(daylioData);
            return GetActivityCount(activity);
        }

        /// <summary>
        /// Gets <see cref="DaylioCSVDataModel"/> entries that contain a specified string in the note.
        /// Assumes that <see cref="DaylioData"/> has been initialized, otherwise returns null.
        /// </summary>
        /// <param name="searchString">The <see cref="string"/> to search for within entries.</param>
        /// <param name="comparisonMethod">The <see cref="StringComparison"/> method to use.</param>
        /// <returns>An <see cref="IEnumerable{DaylioCSVDataModel}"/> of entries that contain the specified search string.</returns>
        public static IEnumerable<DaylioCSVDataModel>? GetEntriesWithString(string searchString, StringComparison comparisonMethod = StringComparison.CurrentCulture)
        {
            return _daylioData?.DataRepo?.CSVData?.Where(entry => !string.IsNullOrWhiteSpace(entry.Note) &&
                entry.Note.Contains(searchString, comparisonMethod));
        }

        /// <summary>
        /// Gets <see cref="DaylioCSVDataModel"/> entries that contain a specified string in the note.
        /// Assumes that <see cref="DaylioData"/> has been initialized, otherwise returns null.
        /// </summary>
        /// <param name="daylioData">The <see cref="DaylioData"/> instance to use.</param>
        /// <param name="searchString">The <see cref="string"/> to search for within entries.</param>
        /// <param name="comparisonMethod">The <see cref="StringComparison"/> method to use.</param>
        /// <returns>An <see cref="IEnumerable{DaylioCSVDataModel}"/> of entries that contain the specified search string.</returns>
        public static IEnumerable<DaylioCSVDataModel>? GetEntriesWithString(DaylioData daylioData, string searchString, StringComparison comparisonMethod = StringComparison.CurrentCulture)
        {
            InitData(daylioData);
            return GetEntriesWithString(searchString, comparisonMethod);
        }
    }
}
