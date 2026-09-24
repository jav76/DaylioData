using DaylioData.Models;

namespace DaylioData;

public static class Methods
{
    private static DaylioData? _daylioData;

    public static void InitData(DaylioData daylioData) => _daylioData = daylioData;

    /// <summary>
    /// Gets the <see cref="DaylioCSVDataModel"/> with the earliest entry date.
    /// Assumes that <see cref="DaylioData"/> has been initialized, otherwise returns null.
    /// </summary>
    /// <returns>The <see cref="DaylioCSVDataModel"/> with the earliest entry date</returns>
    public static DaylioCSVDataModel? GetEarliestEntry() => _daylioData?.DataSummary?.EarliestEntry;

    /// <summary>
    /// Gets the <see cref="DaylioCSVDataModel"/> with the earliest entry date.
    /// </summary>
    /// <param name="daylioData">The <see cref="DaylioData"/> instance to use.</param>
    /// <returns>The <see cref="DaylioCSVDataModel"/> with the earliest entry date.</returns>
    public static DaylioCSVDataModel? GetEarliestEntry(this DaylioData daylioData)
    {
        InitData(daylioData);
        return daylioData.DataSummary?.EarliestEntry;
    }

    /// <summary>
    /// Gets the <see cref="DaylioCSVDataModel"/> with the latest entry date.
    /// Assumes that <see cref="DaylioData"/> has been initialized, otherwise returns null.
    /// </summary>
    /// <returns>The <see cref="DaylioCSVDataModel"/> with the latest entry date.</returns>
    public static DaylioCSVDataModel? GetLatestEntry() => _daylioData?.DataSummary?.LatestEntry;

    /// <summary>
    /// Gets the <see cref="DaylioCSVDataModel"/> with the latest entry date.
    /// </summary>
    /// <param name="daylioData">The <see cref="DaylioData"/> instance to use.</param>
    /// <returns>The <see cref="DaylioCSVDataModel"/> with the latest entry date.</returns>
    public static DaylioCSVDataModel? GetLatestEntry(this DaylioData daylioData)
    {
        InitData(daylioData);
        return daylioData.DataSummary?.LatestEntry;
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
        return _daylioData?.DataRepo?.CSVData?.Where(entry =>
            entry.Timestamp >= startDate && entry.Timestamp <= endDate);
    }

    /// <summary>
    /// Gets <see cref="DaylioCSVDataModel"/> entries in a specified date range.
    /// </summary>
    /// <param name="daylioData">The <see cref="DaylioData"/> instance to use.</param>
    /// <param name="startDate">The earliest (inclusive) <see cref="DateTime"/> of entries.</param>
    /// <param name="endDate">The latest (inclusive) <see cref="DateTime"/> of entries.</param>
    /// <returns>An <see cref="IEnumerable{DaylioCSVDataModel}"/> of entries within the specified date range.</returns>
    public static IEnumerable<DaylioCSVDataModel>? GetEntriesInRange(
        this DaylioData daylioData,
        DateTime startDate,
        DateTime endDate)
    {
        InitData(daylioData);
        return daylioData?.DataRepo?.CSVData?.Where(entry =>
            entry.Timestamp >= startDate && entry.Timestamp <= endDate);
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
            _daylioData?.DataRepo?.Activities.Contains(activity) != true)
        {
            return null;
        }

        return _daylioData?.DataRepo?.CSVData?.Where(entry => entry.ActivitiesCollection
            .Any(entryActivity => entryActivity.Equals(activity, StringComparison.OrdinalIgnoreCase)));
    }

    /// <summary>
    /// Gets <see cref="DaylioCSVDataModel"/> entries that include a specified activity
    /// </summary>
    /// <param name="daylioData">The <see cref="DaylioData"/> instance to use.</param>
    /// <param name="activity">An activity string</param>
    /// <returns>An <see cref="IEnumerable{DaylioCSVDataModel}"/> of entries that contain the specified activity.</returns>
    public static IEnumerable<DaylioCSVDataModel>? GetEntriesWithActivity(this DaylioData daylioData, string activity)
    {
        InitData(daylioData);
        if (string.IsNullOrWhiteSpace(activity) ||
            daylioData?.DataRepo?.Activities.Contains(activity) != true)
        {
            return null;
        }

        return daylioData?.DataRepo?.CSVData?.Where(entry => entry.ActivitiesCollection
            .Any(entryActivity => entryActivity.Equals(activity, StringComparison.OrdinalIgnoreCase)));
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
            _daylioData?.DataRepo?.Moods.ContainsKey(mood) != true)
        {
            return null;
        }

        return _daylioData?.DataRepo?.CSVData?.Where(entry =>
            entry.Mood.Equals(mood, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets <see cref="DaylioCSVDataModel"/> entries that have a specified mood.
    /// </summary>
    /// <param name="daylioData">The <see cref="DaylioData"/> instance to use.</param>
    /// <param name="mood">A mood string</param>
    /// <returns>An <see cref="IEnumerable{DaylioCSVDataModel}"/> of entries that have the specified mood.</returns>
    public static IEnumerable<DaylioCSVDataModel>? GetEntriesWithMood(this DaylioData daylioData, string mood)
    {
        InitData(daylioData);
        if (string.IsNullOrWhiteSpace(mood) ||
            daylioData?.DataRepo?.Moods.ContainsKey(mood) != true)
        {
            return null;
        }

        return daylioData?.DataRepo?.CSVData?.Where(entry =>
            entry.Mood.Equals(mood, StringComparison.OrdinalIgnoreCase));
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
            .Any(entryActivity => entryActivity.Equals(activity, StringComparison.OrdinalIgnoreCase)));
    }

    /// <summary>
    /// Gets the number of entries that include a specified activity.
    /// </summary>
    /// <param name="daylioData">The <see cref="DaylioData"/> instance to use.</param>
    /// <param name="activity">An activity string</param>
    /// <returns>The <see cref="int"/> number of activities that include a specified activity.</returns>
    public static int? GetActivityCount(this DaylioData daylioData, string activity)
    {
        InitData(daylioData);
        return daylioData?.DataRepo?.CSVData?.Count(entry => entry.ActivitiesCollection
            .Any(entryActivity => entryActivity.Equals(activity, StringComparison.OrdinalIgnoreCase)));
    }

    /// <summary>
    /// Gets <see cref="DaylioCSVDataModel"/> entries that contain a specified string in the note.
    /// Assumes that <see cref="DaylioData"/> has been initialized, otherwise returns null.
    /// </summary>
    /// <param name="searchString">The <see cref="string"/> to search for within entries.</param>
    /// <param name="comparisonMethod">The <see cref="StringComparison"/> method to use.</param>
    /// <returns>An <see cref="IEnumerable{DaylioCSVDataModel}"/> of entries that contain the specified search string.</returns>
    public static IEnumerable<DaylioCSVDataModel>? GetEntriesWithString(
        string searchString,
        StringComparison comparisonMethod = StringComparison.CurrentCulture)
    {
        return _daylioData?.DataRepo?.CSVData?.Where(entry => !string.IsNullOrWhiteSpace(entry.Note) &&
            entry.Note.Contains(searchString, comparisonMethod));
    }

    /// <summary>
    /// Gets <see cref="DaylioCSVDataModel"/> entries that contain a specified string in the note.
    /// </summary>
    /// <param name="daylioData">The <see cref="DaylioData"/> instance to use.</param>
    /// <param name="searchString">The <see cref="string"/> to search for within entries.</param>
    /// <param name="comparisonMethod">The <see cref="StringComparison"/> method to use.</param>
    /// <returns>An <see cref="IEnumerable{DaylioCSVDataModel}"/> of entries that contain the specified search string.</returns>
    public static IEnumerable<DaylioCSVDataModel>? GetEntriesWithString(
        this DaylioData daylioData,
        string searchString,
        StringComparison comparisonMethod = StringComparison.CurrentCulture)
    {
        InitData(daylioData);
        return daylioData?.DataRepo?.CSVData?.Where(entry => !string.IsNullOrWhiteSpace(entry.Note) &&
            entry.Note.Contains(searchString, comparisonMethod));
    }

    /// <summary>
    /// Gets an average mood rating for a specified activity. Requires levels to be set for each mood.
    /// Assumes that <see cref="DaylioData"/> has been initialized, otherwise returns null.
    /// </summary>
    /// <param name="activity">The activity name to get an average mood rating for.</param>
    /// <returns>An average <see cref="decimal?"/> mood rating for the specified activity.</returns>
    public static decimal? GetAverageActivityMood(string activity)
    {
        if (_daylioData is null ||
            _daylioData.DataRepo is null ||
            string.IsNullOrWhiteSpace(activity) ||
            !_daylioData.DataRepo.Activities.Contains(activity))
        {
            return null;
        }

        return GetAverageActivityMood(_daylioData, activity);
    }

    /// <summary>
    /// Gets an average mood rating for a specified activity. Requires levels to be set for each mood.
    /// </summary>
    /// <param name="daylioData">The <see cref="DaylioData"/> instance to use.</param>
    /// <param name="activity">The activity name to get an average mood rating for.</param>
    /// <returns>An average <see cref="decimal?"/> mood rating for the specified activity.</returns>
    public static decimal? GetAverageActivityMood(this DaylioData daylioData, string activity)
    {
        InitData(daylioData);
        if (daylioData?.DataRepo is null ||
            string.IsNullOrWhiteSpace(activity) ||
            !daylioData.DataRepo.Activities.Contains(activity))
        {
            return null;
        }

        uint moodSum = 0;
        uint count = 0;

        foreach (DaylioCSVDataModel entry in daylioData.DataRepo.CSVData?.Where(entry =>
            entry.ActivitiesCollection.Any(entryActivity =>
                entryActivity.Equals(activity, StringComparison.OrdinalIgnoreCase)))
            ?? Enumerable.Empty<DaylioCSVDataModel>())
        {
            if (daylioData.DataRepo.Moods.TryGetValue(entry.Mood, out short? moodLevel) && moodLevel.HasValue)
            {
                moodSum += Convert.ToUInt32(moodLevel.Value);
                count++;
            }
        }

        return count == 0 ? null : (decimal)moodSum / count;
    }
}
