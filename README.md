# DaylioData

A modern .NET library and Model Context Protocol (MCP) server for reading, querying, and analyzing exported Daylio CSV mood tracking and activity data.

---

## Features

- **Flexible Ingestion**: Parse Daylio CSV data directly from file paths, `TextReader`, or `Stream` instances with fail-fast validation and exception-safe `TryLoad` patterns.
- **Direct & Fluent Query API**: Filter and search entries by date-time range, mood, activity, or note text with non-null `IReadOnlyList<T>` collection contracts and zero ambient static state.
- **Immutable Data Models**: High-performance immutable `DaylioCSVDataModel` records with pre-cached activity collections and culture-invariant formatting.
- **Single-Pass Cached Metrics**: Instant O(1) summary metric access with automatic cache refreshes on dataset updates.
- **Rich Habit & Mood Analytics**: Calculate mood distributions, top activities, tracking streaks, day-of-week averages, time-of-day trends, habit synergies, and rolling mood trends.
- **Comprehensive Reporting & Export**: Generate publication-ready Markdown reports or structured JSON payloads with one line of code.
- **Model Context Protocol (MCP) Server**: Expose your personal Daylio dataset to AI assistants (such as Claude Desktop, Cursor, and Antigravity) with 11 analytical tools, 3 resource endpoints, and guided prompt templates.

---

## Upgrading to 1.0.0

For breaking changes, deprecated method removals, and step-by-step upgrade instructions from `0.1.x`, see the [v1.0.0 Migration Guide](docs/MIGRATION_v1.0.md).

---

## Library Usage

### Ingesting Data

You can initialize `DaylioData` using fail-fast constructors or safe non-throwing factory methods:

```csharp
using DaylioData;

// Fail-fast initialization (throws FileNotFoundException or IOException on error)
DaylioData daylioData = new("path_to_your_file.csv");

// Exception-safe loading pattern
if (DaylioData.TryLoad("path_to_your_file.csv", out DaylioData? safeData))
{
    // safeData is guaranteed non-null
}

// Or asynchronously
DaylioData asyncData = await DaylioData.LoadAsync("path_to_your_file.csv");

// From a Stream or TextReader (e.g. web upload or memory stream)
using StreamReader reader = new(stream);
DaylioData daylioDataFromStream = new(reader);
```

### Accessing Summary Metrics

Summary metrics are calculated in a single O(N) pass and cached for instant O(1) access:

```csharp
// Guaranteed non-null summary properties
int totalEntries = daylioData.DataSummary.TotalEntries;
int totalDays = daylioData.DataSummary.TotalDays;
double avgPerDay = daylioData.DataSummary.AverageEntriesPerDay;

// Formatted summary text
string summaryText = daylioData.DataSummary.GetSummary();
```

### Direct & Fluent Querying

Query entries using direct instance methods on `DaylioData` or fluent extension methods:

```csharp
using DaylioData;
using DaylioData.Models;

// Query by date range
DateTime start = new(2023, 1, 1, 0, 0, 0);
DateTime end = new(2023, 1, 31, 23, 59, 59);
IReadOnlyList<DaylioCSVDataModel> januaryEntries = daylioData.GetEntriesInRange(start, end);

// Query by activity or mood (returns empty list if not found, never null)
IReadOnlyList<DaylioCSVDataModel> codingEntries = daylioData.GetEntriesWithActivity("coding");
IReadOnlyList<DaylioCSVDataModel> radEntries = daylioData.GetEntriesWithMood("rad");

// Search note text or titles
IReadOnlyList<DaylioCSVDataModel> projectNotes = daylioData.GetEntriesWithString("project");

// Activity occurrence count
int codingCount = daylioData.GetActivityCount("coding");
```

### Habit & Mood Analytics

Compute insights, correlations, and trends using `DaylioAnalytics`:

```csharp
using DaylioData;

// Mood frequency breakdown
IReadOnlyDictionary<string, int> distribution = daylioData.GetMoodDistribution();

// Top 5 most frequent activities
IReadOnlyList<KeyValuePair<string, int>> topActivities = daylioData.GetTopActivities(5);

// Longest and current daily tracking streaks
StreakDetails streakDetails = daylioData.GetStreakDetails();
int longest = streakDetails.LongestStreak;
int current = streakDetails.CurrentStreak;

// Activity-to-mood impact correlation (average mood with vs. without activity)
ActivityMoodImpact? impact = daylioData.GetActivityMoodImpact("running");

// Rank all activities by net mood impact
IReadOnlyList<ActivityMoodImpact> allImpacts = daylioData.GetAllActivityMoodImpacts(minOccurrences: 2);

// Habit synergies: co-occurring activity pairs and net mood boosts
IReadOnlyList<ActivityPairImpact> synergies = daylioData.GetAllActivityPairImpacts(minOccurrences: 2);

// Rolling 7-day smoothed mood trends
IReadOnlyList<DailyRollingMood> trends = daylioData.GetRollingMoodTrends(windowDays: 7);

// Time-of-day trends (Morning, Afternoon, Evening, Night)
IReadOnlyDictionary<TimeOfDayPeriod, TimeOfDayMood> timeOfDayMoods = daylioData.GetMoodByTimeOfDay();

// Average mood rating by day of the week
daylioData.DataRepo.SetDefaultMoodLevels();
IReadOnlyDictionary<DayOfWeek, decimal> weekdayMoods = daylioData.GetAverageMoodByDayOfWeek();
```

### Reports & JSON Export

Generate formatted Markdown reports or JSON data:

```csharp
using DaylioData;

// Generate a Markdown report
string markdownReport = daylioData.GenerateMarkdownReport("Monthly Habit Overview");

// Export structured JSON
string jsonOutput = daylioData.ToJson(indented: true);
```

---

## Daylio MCP Server (`daylio-mcp`)

The solution includes `DaylioData.Mcp`, a standard Model Context Protocol (MCP) server packaged as a .NET Tool. It enables AI assistants to query, analyze, and reason about your Daylio tracking history.

### Running Locally

```bash
# Run directly from source
dotnet run --project src/DaylioData.Mcp -- --file /path/to/daylio_export.csv

# Or install globally as a .NET Tool
dotnet tool install --global DaylioData.Mcp
daylio-mcp --file /path/to/daylio_export.csv
```

You can also specify the dataset file path using the `DAYLIO_CSV_PATH` environment variable.

### Claude Desktop / Cursor Configuration

Add `daylio-mcp` to your `claude_desktop_config.json`:

```json
{
  "mcpServers": {
    "daylio": {
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "/absolute/path/to/DaylioData/src/DaylioData.Mcp",
        "--",
        "--file",
        "/absolute/path/to/daylio_export.csv"
      ]
    }
  }
}
```

If installed globally as a .NET tool:

```json
{
  "mcpServers": {
    "daylio": {
      "command": "daylio-mcp",
      "args": ["--file", "/absolute/path/to/daylio_export.csv"]
    }
  }
}
```

### Supported MCP Capabilities

- **Tools**:
  - `load_dataset`: Dynamically load or switch Daylio CSV files at runtime.
  - `get_summary`: Retrieve high-level statistics and tracking date ranges.
  - `query_entries`: Filter journal entries by date range, mood, activity, or keyword.
  - `get_mood_distribution`: Get counts and percentages for each mood.
  - `get_top_activities`: List the most frequent activities.
  - `get_activity_mood_impact`: Measure whether specific habits raise or lower mood ratings.
  - `get_activity_synergies`: Identify co-occurring habits and evaluate positive/negative compounding mood effects.
  - `get_time_of_day_trends`: Analyze mood patterns across Morning, Afternoon, Evening, and Night.
  - `get_streaks`: Retrieve current and longest tracking streaks.
  - `get_rolling_mood_trends`: Track smoothed daily and multi-day rolling mood averages over time.
  - `generate_report`: Generate a complete Markdown summary report.
- **Resources**:
  - `daylio://summary`: Dataset summary statistics in JSON.
  - `daylio://activities`: Complete alphabetical list of logged activities.
  - `daylio://moods`: Configured moods and numeric ratings.
- **Prompts**:
  - `weekly_reflection`: Guided template for reviewing weekly trends and accomplishments.
  - `habit_correlation_review`: Guided template for deep-dive habit correlation analysis.

---

## Testing & Verification

Run the test suite across all targets:

```bash
dotnet test src/DaylioData/DaylioData.sln
```

Verify formatting:

```bash
dotnet format src/DaylioData/DaylioData.sln --verify-no-changes --severity warn
```

---

## NuGet Package

NuGet: https://www.nuget.org/packages/DaylioData/
