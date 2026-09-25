using DaylioData.Models;

namespace DaylioData;

/// <summary>
/// Provides extension methods for querying and filtering Daylio datasets.
/// </summary>
public static class DaylioQueryExtensions
{
    /// <summary>
    /// Gets the <see cref="DaylioCSVDataModel"/> with the earliest entry date.
    /// </summary>
    /// <param name="daylioData">The <see cref="DaylioData"/> instance.</param>
    /// <returns>The earliest entry, or null if the dataset contains no entries.</returns>
    public static DaylioCSVDataModel? GetEarliestEntry(this DaylioData daylioData)
    {
        ArgumentNullException.ThrowIfNull(daylioData);
        return daylioData.GetEarliestEntry();
    }

    /// <summary>
    /// Gets the <see cref="DaylioCSVDataModel"/> with the latest entry date.
    /// </summary>
    /// <param name="daylioData">The <see cref="DaylioData"/> instance.</param>
    /// <returns>The latest entry, or null if the dataset contains no entries.</returns>
    public static DaylioCSVDataModel? GetLatestEntry(this DaylioData daylioData)
    {
        ArgumentNullException.ThrowIfNull(daylioData);
        return daylioData.GetLatestEntry();
    }

    /// <summary>
    /// Gets entries within a specified date range (inclusive).
    /// </summary>
    /// <param name="daylioData">The <see cref="DaylioData"/> instance.</param>
    /// <param name="startDate">The earliest (inclusive) <see cref="DateTime"/>.</param>
    /// <param name="endDate">The latest (inclusive) <see cref="DateTime"/>.</param>
    /// <returns>A read-only list of entries within the specified date range.</returns>
    public static IReadOnlyList<DaylioCSVDataModel> GetEntriesInRange(
        this DaylioData daylioData,
        DateTime startDate,
        DateTime endDate)
    {
        ArgumentNullException.ThrowIfNull(daylioData);
        return daylioData.GetEntriesInRange(startDate, endDate);
    }

    /// <summary>
    /// Gets entries that include the specified activity.
    /// </summary>
    /// <param name="daylioData">The <see cref="DaylioData"/> instance.</param>
    /// <param name="activity">The activity name to filter by.</param>
    /// <returns>A read-only list of matching entries.</returns>
    public static IReadOnlyList<DaylioCSVDataModel> GetEntriesWithActivity(
        this DaylioData daylioData,
        string activity)
    {
        ArgumentNullException.ThrowIfNull(daylioData);
        return daylioData.GetEntriesWithActivity(activity);
    }

    /// <summary>
    /// Gets entries that have the specified mood.
    /// </summary>
    /// <param name="daylioData">The <see cref="DaylioData"/> instance.</param>
    /// <param name="mood">The mood name to filter by.</param>
    /// <returns>A read-only list of matching entries.</returns>
    public static IReadOnlyList<DaylioCSVDataModel> GetEntriesWithMood(
        this DaylioData daylioData,
        string mood)
    {
        ArgumentNullException.ThrowIfNull(daylioData);
        return daylioData.GetEntriesWithMood(mood);
    }

    /// <summary>
    /// Gets the number of entries that include the specified activity.
    /// </summary>
    /// <param name="daylioData">The <see cref="DaylioData"/> instance.</param>
    /// <param name="activity">The activity name to count.</param>
    /// <returns>The count of matching entries.</returns>
    public static int GetActivityCount(this DaylioData daylioData, string activity)
    {
        ArgumentNullException.ThrowIfNull(daylioData);
        return daylioData.GetActivityCount(activity);
    }

    /// <summary>
    /// Gets entries that contain the search string in their note or note title.
    /// </summary>
    /// <param name="daylioData">The <see cref="DaylioData"/> instance.</param>
    /// <param name="searchString">The string to search for.</param>
    /// <param name="comparisonMethod">The <see cref="StringComparison"/> method to use.</param>
    /// <returns>A read-only list of matching entries.</returns>
    public static IReadOnlyList<DaylioCSVDataModel> GetEntriesWithString(
        this DaylioData daylioData,
        string searchString,
        StringComparison comparisonMethod = StringComparison.CurrentCulture)
    {
        ArgumentNullException.ThrowIfNull(daylioData);
        return daylioData.GetEntriesWithString(searchString, comparisonMethod);
    }

    /// <summary>
    /// Gets an average mood rating for a specified activity.
    /// </summary>
    /// <param name="daylioData">The <see cref="DaylioData"/> instance.</param>
    /// <param name="activity">The activity name to average.</param>
    /// <returns>The average rating, or null if unrated.</returns>
    public static decimal? GetAverageActivityMood(this DaylioData daylioData, string activity)
    {
        ArgumentNullException.ThrowIfNull(daylioData);
        return daylioData.GetAverageActivityMood(activity);
    }
}
