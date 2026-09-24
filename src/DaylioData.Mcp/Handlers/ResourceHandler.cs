using System.Globalization;
using System.Text.Json;
using DaylioData.Mcp.Protocol;
using DaylioData.Models;

namespace DaylioData.Mcp.Handlers;

/// <summary>
/// Handles MCP resource listing and reading for Daylio data.
/// </summary>
public static class ResourceHandler
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Gets definitions for all resources exposed by the MCP server.
    /// </summary>
    /// <returns>A list of <see cref="McpResource"/>.</returns>
    public static List<McpResource> GetResourceDefinitions()
    {
        return new List<McpResource>
        {
            new(
                "daylio://summary",
                "Daylio Summary Metrics",
                "Summary statistics of the loaded Daylio tracking dataset.",
                "application/json"),
            new(
                "daylio://activities",
                "Daylio Distinct Activities",
                "Alphabetical list of all distinct activity tags logged in the dataset.",
                "application/json"),
            new(
                "daylio://moods",
                "Daylio Configured Moods",
                "Listing of all recorded moods and their numeric rating levels.",
                "application/json")
        };
    }

    /// <summary>
    /// Reads the content of an MCP resource by URI.
    /// </summary>
    /// <param name="uri">The resource URI.</param>
    /// <param name="getCurrentData">Function to retrieve the active DaylioData instance.</param>
    /// <returns>A <see cref="ResourceReadResult"/>.</returns>
    public static ResourceReadResult ReadResource(string uri, Func<DaylioData?> getCurrentData)
    {
        DaylioData? data = getCurrentData();
        if (data is null || data.DataRepo?.CSVData is null)
        {
            return new ResourceReadResult(
                new List<ResourceContent>
                {
                    new(uri, "text/plain", "No Daylio dataset loaded.")
                });
        }

        string text = uri.ToLowerInvariant() switch
        {
            "daylio://summary" => GetSummaryJson(data),
            "daylio://activities" => GetActivitiesJson(data),
            "daylio://moods" => GetMoodsJson(data),
            _ => $"{{\"error\": \"Resource not found: '{uri}'\"}}"
        };

        return new ResourceReadResult(
            new List<ResourceContent>
            {
                new(uri, "application/json", text)
            });
    }

    private static string GetSummaryJson(DaylioData data)
    {
        DaylioDataSummary? s = data.DataSummary;
        StreakDetails streaks = data.GetStreakDetails();

        object summaryObj = new
        {
            totalEntries = s?.TotalEntries ?? 0,
            totalDays = s?.TotalDays ?? 0,
            averageEntriesPerDay = s?.AverageEntriesPerDay ?? 0.0,
            distinctActivitiesCount = s?.DistinctActivitiesCount ?? 0,
            totalActivitiesCount = s?.TotalActivitiesCount ?? 0,
            noteTotalWordCount = s?.NoteTotalWordCount ?? 0,
            earliestDate = s?.EarliestEntry?.FullDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            latestDate = s?.LatestEntry?.FullDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            longestStreak = streaks.LongestStreak,
            currentStreak = streaks.CurrentStreak
        };

        return JsonSerializer.Serialize(summaryObj, JsonOptions);
    }

    private static string GetActivitiesJson(DaylioData data)
    {
        List<string> activities = data.DataRepo?.Activities
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToList() ?? new List<string>();

        return JsonSerializer.Serialize(activities, JsonOptions);
    }

    private static string GetMoodsJson(DaylioData data)
    {
        Dictionary<string, short?> moods = data.DataRepo?.Moods ?? new Dictionary<string, short?>();
        return JsonSerializer.Serialize(moods, JsonOptions);
    }
}
