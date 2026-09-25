using System.Globalization;
using System.Text;
using System.Text.Json;
using DaylioData.Models;

namespace DaylioData;

/// <summary>
/// Provides export and report generation utilities for Daylio datasets.
/// </summary>
public static class DaylioExport
{
    /// <summary>
    /// Generates a comprehensive Markdown report summarizing dataset metrics, mood distribution,
    /// top activities, habit impacts, and time-of-day trends.
    /// </summary>
    /// <param name="daylioData">The <see cref="DaylioData"/> instance.</param>
    /// <param name="title">Optional custom title for the report.</param>
    /// <returns>A formatted Markdown report string.</returns>
    public static string GenerateMarkdownReport(this DaylioData daylioData, string? title = null)
    {
        StringBuilder sb = new();
        string reportTitle = string.IsNullOrWhiteSpace(title) ? "Daylio Wellness & Habit Report" : title;

        sb.AppendLine(CultureInfo.InvariantCulture, $"# {reportTitle}");
        sb.AppendLine();

        DaylioDataSummary? summary = daylioData.DataSummary;
        StreakDetails streaks = daylioData.GetStreakDetails();

        sb.AppendLine("## Summary Overview");
        sb.AppendLine();
        sb.AppendLine(CultureInfo.InvariantCulture, $"- **Total Entries**: {summary?.TotalEntries ?? 0}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"- **Total Days Tracked**: {summary?.TotalDays ?? 0}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"- **Average Entries / Day**: {(summary?.AverageEntriesPerDay ?? 0.0):F2}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"- **Longest Daily Streak**: {streaks.LongestStreak} days");
        sb.AppendLine(CultureInfo.InvariantCulture, $"- **Current Daily Streak**: {streaks.CurrentStreak} days");

        if (summary?.EarliestEntry is not null && summary?.LatestEntry is not null)
        {
            sb.AppendLine(CultureInfo.InvariantCulture, $"- **Date Range**: {summary.EarliestEntry.FullDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)} to {summary.LatestEntry.FullDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}");
        }

        sb.AppendLine(CultureInfo.InvariantCulture, $"- **Distinct Activities**: {summary?.DistinctActivitiesCount ?? 0}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"- **Total Note Word Count**: {summary?.NoteTotalWordCount ?? 0}");
        sb.AppendLine();

        // Mood Distribution
        IReadOnlyDictionary<string, int> distribution = daylioData.GetMoodDistribution();
        int totalEntries = summary?.TotalEntries ?? 0;
        if (distribution.Count > 0)
        {
            sb.AppendLine("## Mood Distribution");
            sb.AppendLine();
            sb.AppendLine("| Mood | Count | Percentage |");
            sb.AppendLine("| :--- | :--- | :--- |");
            foreach (KeyValuePair<string, int> pair in distribution.OrderByDescending(x => x.Value))
            {
                double pct = totalEntries > 0 ? (pair.Value / (double)totalEntries) * 100.0 : 0.0;
                sb.AppendLine(CultureInfo.InvariantCulture, $"| {pair.Key} | {pair.Value} | {pct:F1}% |");
            }
            sb.AppendLine();
        }

        // Top Activities
        IReadOnlyList<KeyValuePair<string, int>> topActivities = daylioData.GetTopActivities(10);
        if (topActivities.Count > 0)
        {
            sb.AppendLine("## Top Activities");
            sb.AppendLine();
            sb.AppendLine("| Activity | Count |");
            sb.AppendLine("| :--- | :--- |");
            foreach (KeyValuePair<string, int> activity in topActivities)
            {
                sb.AppendLine(CultureInfo.InvariantCulture, $"| {activity.Key} | {activity.Value} |");
            }
            sb.AppendLine();
        }

        // Habit Mood Impacts
        IReadOnlyList<ActivityMoodImpact> impacts = daylioData.GetAllActivityMoodImpacts(minOccurrences: 2);
        if (impacts.Count > 0)
        {
            sb.AppendLine("## Activity Mood Impact (Min. 2 Occurrences)");
            sb.AppendLine();
            sb.AppendLine("| Activity | Avg With | Avg Without | Delta | Entries |");
            sb.AppendLine("| :--- | :--- | :--- | :--- | :--- |");
            foreach (ActivityMoodImpact impact in impacts.Take(15))
            {
                string sign = impact.Delta >= 0 ? "+" : string.Empty;
                sb.AppendLine(CultureInfo.InvariantCulture, $"| {impact.Activity} | {impact.AverageMoodWith:F2} | {impact.AverageMoodWithout:F2} | {sign}{impact.Delta:F2} | {impact.FrequencyWith} |");
            }
            sb.AppendLine();
        }

        // Time-of-Day Trends
        IReadOnlyDictionary<TimeOfDayPeriod, TimeOfDayMood> timeOfDay = daylioData.GetMoodByTimeOfDay();
        if (timeOfDay.Count > 0)
        {
            sb.AppendLine("## Time-of-Day Mood Trends");
            sb.AppendLine();
            sb.AppendLine("| Period | Average Mood | Entries |");
            sb.AppendLine("| :--- | :--- | :--- |");
            foreach (KeyValuePair<TimeOfDayPeriod, TimeOfDayMood> period in timeOfDay)
            {
                sb.AppendLine(CultureInfo.InvariantCulture, $"| {period.Key} | {period.Value.AverageMood:F2} | {period.Value.EntryCount} |");
            }
            sb.AppendLine();
        }

        // Recent Mood Trends (7-Day Rolling Average)
        IReadOnlyList<DailyRollingMood> trends = daylioData.GetRollingMoodTrends(windowDays: 7);
        if (trends.Count > 0)
        {
            sb.AppendLine("## Recent Mood Trends (7-Day Rolling Average)");
            sb.AppendLine();
            sb.AppendLine("| Date | Daily Avg | 7-Day Rolling Avg | Entries |");
            sb.AppendLine("| :--- | :--- | :--- | :--- |");
            foreach (DailyRollingMood day in trends.TakeLast(10))
            {
                sb.AppendLine(CultureInfo.InvariantCulture, $"| {day.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)} | {day.DailyAverageMood:F2} | {day.RollingAverageMood:F2} | {day.EntryCount} |");
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }

    /// <summary>
    /// Serializes key dataset metrics and analytics into structured JSON.
    /// </summary>
    /// <param name="daylioData">The <see cref="DaylioData"/> instance.</param>
    /// <param name="indented">Whether to format the JSON output with indentation.</param>
    /// <returns>A JSON string representation of the dataset analysis.</returns>
    public static string ToJson(this DaylioData daylioData, bool indented = true)
    {
        JsonSerializerOptions options = new()
        {
            WriteIndented = indented,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        DaylioDataSummary? summary = daylioData.DataSummary;
        StreakDetails streaks = daylioData.GetStreakDetails();

        DaylioExportData payload = new(
            TotalEntries: summary?.TotalEntries ?? 0,
            TotalDays: summary?.TotalDays ?? 0,
            AverageEntriesPerDay: summary?.AverageEntriesPerDay ?? 0.0,
            DistinctActivitiesCount: summary?.DistinctActivitiesCount ?? 0,
            TotalActivitiesCount: summary?.TotalActivitiesCount ?? 0,
            NoteTotalWordCount: summary?.NoteTotalWordCount ?? 0,
            EarliestDate: summary?.EarliestEntry?.FullDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            LatestDate: summary?.LatestEntry?.FullDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            Streaks: streaks,
            MoodDistribution: daylioData.GetMoodDistribution(),
            TopActivities: daylioData.GetTopActivities(15),
            ActivityImpacts: daylioData.GetAllActivityMoodImpacts(minOccurrences: 1),
            TimeOfDayTrends: daylioData.GetMoodByTimeOfDay(),
            RollingMoodTrends: daylioData.GetRollingMoodTrends(windowDays: 7));

        return JsonSerializer.Serialize(payload, options);
    }
}

/// <summary>
/// Data transfer record containing structured dataset analytics and metrics for JSON export.
/// </summary>
public record DaylioExportData(
    int TotalEntries,
    int TotalDays,
    double AverageEntriesPerDay,
    int DistinctActivitiesCount,
    int TotalActivitiesCount,
    int NoteTotalWordCount,
    string? EarliestDate,
    string? LatestDate,
    StreakDetails Streaks,
    IReadOnlyDictionary<string, int> MoodDistribution,
    IReadOnlyList<KeyValuePair<string, int>> TopActivities,
    IReadOnlyList<ActivityMoodImpact> ActivityImpacts,
    IReadOnlyDictionary<TimeOfDayPeriod, TimeOfDayMood> TimeOfDayTrends,
    IReadOnlyList<DailyRollingMood>? RollingMoodTrends = null);
