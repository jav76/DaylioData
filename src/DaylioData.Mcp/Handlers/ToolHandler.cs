using System.Globalization;
using System.Text.Json;
using DaylioData.Mcp.Protocol;
using DaylioData.Models;

namespace DaylioData.Mcp.Handlers;

/// <summary>
/// Handles MCP tool registration and execution for Daylio datasets.
/// </summary>
public static class ToolHandler
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Gets the list of all available MCP tools supported by the server.
    /// </summary>
    /// <returns>A list of <see cref="McpTool"/> definitions.</returns>
    public static List<McpTool> GetToolDefinitions()
    {
        return new List<McpTool>
        {
            new(
                "load_dataset",
                "Loads or switches a Daylio CSV export file into the server's in-memory repository.",
                new
                {
                    type = "object",
                    properties = new
                    {
                        filePath = new
                        {
                            type = "string",
                            description = "The absolute or relative file path to the Daylio CSV export."
                        }
                    },
                    required = new[] { "filePath" }
                }),
            new(
                "get_summary",
                "Returns high-level summary metrics of the loaded Daylio dataset (total days, entries, earliest/latest dates, distinct activities).",
                new
                {
                    type = "object",
                    properties = new { }
                }),
            new(
                "query_entries",
                "Queries Daylio journal entries with optional filtering by date range, mood, activity, or note keyword.",
                new
                {
                    type = "object",
                    properties = new
                    {
                        startDate = new
                        {
                            type = "string",
                            description = "Earliest date in YYYY-MM-DD format (inclusive)."
                        },
                        endDate = new
                        {
                            type = "string",
                            description = "Latest date in YYYY-MM-DD format (inclusive)."
                        },
                        mood = new
                        {
                            type = "string",
                            description = "Specific mood string to filter by (e.g. 'rad', 'good', 'meh', 'bad', 'awful')."
                        },
                        activity = new
                        {
                            type = "string",
                            description = "Specific activity to filter by (case-insensitive)."
                        },
                        keyword = new
                        {
                            type = "string",
                            description = "Search term to match against notes or note titles."
                        },
                        limit = new
                        {
                            type = "integer",
                            description = "Maximum number of entries to return (defaults to 20)."
                        }
                    }
                }),
            new(
                "get_mood_distribution",
                "Returns the distribution and frequency of all recorded moods in the dataset.",
                new
                {
                    type = "object",
                    properties = new { }
                }),
            new(
                "get_top_activities",
                "Returns the most frequently logged activities and their occurrence counts.",
                new
                {
                    type = "object",
                    properties = new
                    {
                        count = new
                        {
                            type = "integer",
                            description = "Number of top activities to return (defaults to 10)."
                        }
                    }
                }),
            new(
                "get_activity_mood_impact",
                "Calculates the statistical impact of activities on mood ratings (average mood with vs. without activity, and delta).",
                new
                {
                    type = "object",
                    properties = new
                    {
                        activity = new
                        {
                            type = "string",
                            description = "Optional specific activity name. If omitted, returns all activities ranked by net delta."
                        },
                        minOccurrences = new
                        {
                            type = "integer",
                            description = "Minimum occurrence count for an activity to be included (defaults to 1)."
                        }
                    }
                }),
            new(
                "get_time_of_day_trends",
                "Analyzes average mood ratings and logging frequencies across Morning, Afternoon, Evening, and Night periods.",
                new
                {
                    type = "object",
                    properties = new { }
                }),
            new(
                "get_streaks",
                "Calculates current and longest daily tracking streaks.",
                new
                {
                    type = "object",
                    properties = new { }
                }),
            new(
                "generate_report",
                "Generates a complete, structured Markdown overview report of the Daylio dataset.",
                new
                {
                    type = "object",
                    properties = new
                    {
                        title = new
                        {
                            type = "string",
                            description = "Optional custom title for the report."
                        }
                    }
                }),
            new(
                "get_activity_synergies",
                "Calculates mood synergies and co-occurrence frequency for pairs of activities logged together.",
                new
                {
                    type = "object",
                    properties = new
                    {
                        minOccurrences = new
                        {
                            type = "integer",
                            description = "Minimum number of co-occurrences required to include a pair (default: 2)."
                        },
                        count = new
                        {
                            type = "integer",
                            description = "Maximum number of synergy pairs to return (default: 15)."
                        }
                    }
                })
        };
    }

    /// <summary>
    /// Executes a tool by name with provided arguments against the active dataset.
    /// </summary>
    /// <param name="name">The tool name.</param>
    /// <param name="args">The arguments JSON element.</param>
    /// <param name="getCurrentData">Function to retrieve the currently active DaylioData instance.</param>
    /// <param name="setDataset">Action to update the active DaylioData instance and file path.</param>
    /// <returns>A <see cref="ToolCallResult"/> containing the output text.</returns>
    public static ToolCallResult ExecuteTool(
        string name,
        JsonElement? args,
        Func<DaylioData?> getCurrentData,
        Action<DaylioData, string> setDataset)
    {
        if (name.Equals("load_dataset", StringComparison.OrdinalIgnoreCase))
        {
            return ExecuteLoadDataset(args, setDataset);
        }

        DaylioData? daylioData = getCurrentData();
        if (daylioData is null || daylioData.DataRepo?.CSVData is null)
        {
            return new ToolCallResult(
                new List<ToolCallContent>
                {
                    new(
                        "text",
                        "No Daylio dataset is currently loaded. Use the 'load_dataset' tool with a valid file path, or pass --file at startup.")
                },
                IsError: true);
        }

        return name.ToLowerInvariant() switch
        {
            "get_summary" => ExecuteGetSummary(daylioData),
            "query_entries" => ExecuteQueryEntries(daylioData, args),
            "get_mood_distribution" => ExecuteGetMoodDistribution(daylioData),
            "get_top_activities" => ExecuteGetTopActivities(daylioData, args),
            "get_activity_mood_impact" => ExecuteGetActivityMoodImpact(daylioData, args),
            "get_time_of_day_trends" => ExecuteGetTimeOfDayTrends(daylioData),
            "get_streaks" => ExecuteGetStreaks(daylioData),
            "generate_report" => ExecuteGenerateReport(daylioData, args),
            "get_activity_synergies" => ExecuteGetActivitySynergies(daylioData, args),
            _ => new ToolCallResult(
                new List<ToolCallContent> { new("text", $"Unknown tool: '{name}'") },
                IsError: true)
        };
    }

    private static ToolCallResult ExecuteLoadDataset(JsonElement? args, Action<DaylioData, string> setDataset)
    {
        if (args is null || !args.Value.TryGetProperty("filePath", out JsonElement pathElement))
        {
            return new ToolCallResult(
                new List<ToolCallContent> { new("text", "Missing required argument 'filePath'.") },
                IsError: true);
        }

        string? filePath = pathElement.GetString();
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
        {
            return new ToolCallResult(
                new List<ToolCallContent> { new("text", $"File does not exist at path: '{filePath}'.") },
                IsError: true);
        }

        try
        {
            DaylioData loaded = new(filePath);
            if (loaded.DataRepo?.CSVData is null)
            {
                return new ToolCallResult(
                    new List<ToolCallContent> { new("text", $"Failed to parse CSV data from: '{filePath}'.") },
                    IsError: true);
            }

            setDataset(loaded, filePath);
            int total = loaded.DataSummary?.TotalEntries ?? 0;
            int days = loaded.DataSummary?.TotalDays ?? 0;
            string earliest = loaded.DataSummary?.EarliestEntry?.FullDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "N/A";
            string latest = loaded.DataSummary?.LatestEntry?.FullDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "N/A";

            string msg = $"Successfully loaded Daylio dataset from '{filePath}'.\n" +
                         $"- Entries: {total}\n" +
                         $"- Days Tracked: {days}\n" +
                         $"- Date Range: {earliest} to {latest}";

            return new ToolCallResult(new List<ToolCallContent> { new("text", msg) });
        }
        catch (Exception ex)
        {
            return new ToolCallResult(
                new List<ToolCallContent> { new("text", $"Error loading CSV: {ex.Message}") },
                IsError: true);
        }
    }

    private static ToolCallResult ExecuteGetSummary(DaylioData daylioData)
    {
        DaylioDataSummary? s = daylioData.DataSummary;
        StreakDetails streaks = daylioData.GetStreakDetails();

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

        return new ToolCallResult(
            new List<ToolCallContent> { new("text", JsonSerializer.Serialize(summaryObj, JsonOptions)) });
    }

    private static ToolCallResult ExecuteQueryEntries(DaylioData daylioData, JsonElement? args)
    {
        IEnumerable<DaylioCSVDataModel>? results = daylioData.DataRepo?.CSVData;
        if (results is null)
        {
            return new ToolCallResult(new List<ToolCallContent> { new("text", "[]") });
        }

        if (args is not null)
        {
            if (args.Value.TryGetProperty("startDate", out JsonElement startElement) &&
                DateOnly.TryParse(startElement.GetString(), CultureInfo.InvariantCulture, out DateOnly startDate))
            {
                results = results.Where(e => e.FullDate >= startDate);
            }

            if (args.Value.TryGetProperty("endDate", out JsonElement endElement) &&
                DateOnly.TryParse(endElement.GetString(), CultureInfo.InvariantCulture, out DateOnly endDate))
            {
                results = results.Where(e => e.FullDate <= endDate);
            }

            if (args.Value.TryGetProperty("mood", out JsonElement moodElement))
            {
                string? targetMood = moodElement.GetString();
                if (!string.IsNullOrWhiteSpace(targetMood))
                {
                    results = results.Where(e => e.Mood.Equals(targetMood, StringComparison.OrdinalIgnoreCase));
                }
            }

            if (args.Value.TryGetProperty("activity", out JsonElement actElement))
            {
                string? targetAct = actElement.GetString();
                if (!string.IsNullOrWhiteSpace(targetAct))
                {
                    results = results.Where(e => e.ActivitiesCollection.Any(a =>
                        a.Equals(targetAct, StringComparison.OrdinalIgnoreCase)));
                }
            }

            if (args.Value.TryGetProperty("keyword", out JsonElement keyElement))
            {
                string? keyword = keyElement.GetString();
                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    results = results.Where(e =>
                        (!string.IsNullOrWhiteSpace(e.Note) &&
                         e.Note.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrWhiteSpace(e.NoteTitle) &&
                         e.NoteTitle.Contains(keyword, StringComparison.OrdinalIgnoreCase)));
                }
            }
        }

        int limit = 20;
        if (args is not null && args.Value.TryGetProperty("limit", out JsonElement limitElement) &&
            limitElement.TryGetInt32(out int customLimit) && customLimit > 0)
        {
            limit = customLimit;
        }

        List<object> formatted = results
            .OrderByDescending(e => e.Timestamp)
            .Take(limit)
            .Select(e => (object)new
            {
                date = e.FullDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                time = e.Time.ToString("HH:mm", CultureInfo.InvariantCulture),
                weekday = e.Weekday,
                mood = e.Mood,
                activities = e.ActivitiesCollection,
                noteTitle = e.NoteTitle,
                note = e.Note
            })
            .ToList();

        return new ToolCallResult(
            new List<ToolCallContent> { new("text", JsonSerializer.Serialize(formatted, JsonOptions)) });
    }

    private static ToolCallResult ExecuteGetMoodDistribution(DaylioData daylioData)
    {
        IReadOnlyDictionary<string, int> distribution = daylioData.GetMoodDistribution();
        int total = daylioData.DataSummary?.TotalEntries ?? 0;

        List<object> list = distribution
            .OrderByDescending(x => x.Value)
            .Select(x => (object)new
            {
                mood = x.Key,
                count = x.Value,
                percentage = total > 0 ? Math.Round((x.Value / (double)total) * 100.0, 1) : 0.0
            })
            .ToList();

        return new ToolCallResult(
            new List<ToolCallContent> { new("text", JsonSerializer.Serialize(list, JsonOptions)) });
    }

    private static ToolCallResult ExecuteGetTopActivities(DaylioData daylioData, JsonElement? args)
    {
        int count = 10;
        if (args is not null && args.Value.TryGetProperty("count", out JsonElement countElement) &&
            countElement.TryGetInt32(out int customCount) && customCount > 0)
        {
            count = customCount;
        }

        IReadOnlyList<KeyValuePair<string, int>> top = daylioData.GetTopActivities(count);
        List<object> list = top
            .Select(x => (object)new { activity = x.Key, count = x.Value })
            .ToList();

        return new ToolCallResult(
            new List<ToolCallContent> { new("text", JsonSerializer.Serialize(list, JsonOptions)) });
    }

    private static ToolCallResult ExecuteGetActivityMoodImpact(DaylioData daylioData, JsonElement? args)
    {
        string? activity = null;
        int minOccurrences = 1;

        if (args is not null)
        {
            if (args.Value.TryGetProperty("activity", out JsonElement actElement))
            {
                activity = actElement.GetString();
            }

            if (args.Value.TryGetProperty("minOccurrences", out JsonElement minElement) &&
                minElement.TryGetInt32(out int customMin) && customMin > 0)
            {
                minOccurrences = customMin;
            }
        }

        if (!string.IsNullOrWhiteSpace(activity))
        {
            ActivityMoodImpact? impact = daylioData.GetActivityMoodImpact(activity);
            if (impact is null)
            {
                return new ToolCallResult(
                    new List<ToolCallContent> { new("text", $"No entries found for activity: '{activity}'.") });
            }

            return new ToolCallResult(
                new List<ToolCallContent> { new("text", JsonSerializer.Serialize(impact, JsonOptions)) });
        }

        IReadOnlyList<ActivityMoodImpact> impacts = daylioData.GetAllActivityMoodImpacts(minOccurrences);
        return new ToolCallResult(
            new List<ToolCallContent> { new("text", JsonSerializer.Serialize(impacts, JsonOptions)) });
    }

    private static ToolCallResult ExecuteGetTimeOfDayTrends(DaylioData daylioData)
    {
        IReadOnlyDictionary<TimeOfDayPeriod, TimeOfDayMood> trends = daylioData.GetMoodByTimeOfDay();
        List<object> list = trends
            .Select(x => (object)new
            {
                period = x.Key.ToString(),
                averageMood = Math.Round(x.Value.AverageMood, 2),
                entryCount = x.Value.EntryCount
            })
            .ToList();

        return new ToolCallResult(
            new List<ToolCallContent> { new("text", JsonSerializer.Serialize(list, JsonOptions)) });
    }

    private static ToolCallResult ExecuteGetStreaks(DaylioData daylioData)
    {
        StreakDetails streaks = daylioData.GetStreakDetails();
        object streakObj = new
        {
            longestStreak = streaks.LongestStreak,
            currentStreak = streaks.CurrentStreak,
            streakStartDate = streaks.StreakStartDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            streakEndDate = streaks.StreakEndDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
        };

        return new ToolCallResult(
            new List<ToolCallContent> { new("text", JsonSerializer.Serialize(streakObj, JsonOptions)) });
    }

    private static ToolCallResult ExecuteGenerateReport(DaylioData daylioData, JsonElement? args)
    {
        string? title = null;
        if (args is not null && args.Value.TryGetProperty("title", out JsonElement titleElement))
        {
            title = titleElement.GetString();
        }

        string report = daylioData.GenerateMarkdownReport(title);
        return new ToolCallResult(
            new List<ToolCallContent> { new("text", report) });
    }

    private static ToolCallResult ExecuteGetActivitySynergies(DaylioData daylioData, JsonElement? args)
    {
        int minOccurrences = 2;
        int count = 15;

        if (args is not null)
        {
            if (args.Value.TryGetProperty("minOccurrences", out JsonElement minElement) && minElement.TryGetInt32(out int parsedMin))
            {
                minOccurrences = Math.Max(1, parsedMin);
            }

            if (args.Value.TryGetProperty("count", out JsonElement countElement) && countElement.TryGetInt32(out int parsedCount))
            {
                count = Math.Max(1, parsedCount);
            }
        }

        IReadOnlyList<ActivityPairImpact> synergies = daylioData.GetAllActivityPairImpacts(minOccurrences)
            .Take(count)
            .ToList();

        string json = JsonSerializer.Serialize(synergies, JsonOptions);
        return new ToolCallResult(new List<ToolCallContent> { new("text", json) });
    }
}
