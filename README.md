# DaylioData

A .NET library and Model Context Protocol (MCP) server for reading, querying, and analyzing exported Daylio CSV mood tracking and activity data.

## Features

- **Flexible Ingestion**: Parse Daylio CSV data directly from file paths, `TextReader`, or `Stream` instances.
- **Fluent Query API**: Filter and search entries by date-time range, mood, activity, or note text without shared static state.
- **Rich Habit & Mood Analytics**: Calculate mood distribution, top activities, tracking streaks, day-of-week averages, time-of-day trends, and activity-to-mood impact correlations.
- **Comprehensive Reporting & Export**: Generate publication-ready Markdown reports or structured JSON payloads with one line of code.
- **Model Context Protocol (MCP) Server**: Expose your personal Daylio dataset to AI assistants (such as Claude Desktop, Cursor, and Antigravity) with 9 analytical tools, 3 resource endpoints, and guided prompt templates.

---

## Library Usage

### Ingesting Data

You can initialize `DaylioData` using a file path, `TextReader`, or `Stream`:

```csharp
using DaylioData;

// From a local CSV file
DaylioData daylioData = new("path_to_your_file.csv");

// Or from a Stream / TextReader (e.g. web upload or memory stream)
using StreamReader reader = new(stream);
DaylioData daylioDataFromStream = new(reader);
```

### Accessing Summary Metrics

```csharp
string summary = daylioData.DataSummary?.GetSummary() ?? string.Empty;
```

### Fluent Querying

Query entries fluently using thread-safe extension methods:

```csharp
using DaylioData;
using DaylioData.Models;

// Query by date range
DateTime start = new(2023, 1, 1, 0, 0, 0);
DateTime end = new(2023, 1, 31, 23, 59, 59);
IEnumerable<DaylioCSVDataModel>? januaryEntries = daylioData.GetEntriesInRange(start, end);

// Query by activity or mood
IEnumerable<DaylioCSVDataModel>? codingEntries = daylioData.GetEntriesWithActivity("coding");
IEnumerable<DaylioCSVDataModel>? radEntries = daylioData.GetEntriesWithMood("rad");

// Search note text or titles
IEnumerable<DaylioCSVDataModel>? projectNotes = daylioData.GetEntriesWithString("project");
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
// Or rank all activities by positive/negative mood delta:
IReadOnlyList<ActivityMoodImpact> allImpacts = daylioData.GetAllActivityMoodImpacts(minOccurrences: 2);

// Time-of-day trends (Morning, Afternoon, Evening, Night)
IReadOnlyDictionary<TimeOfDayPeriod, TimeOfDayMood> timeOfDayMoods = daylioData.GetMoodByTimeOfDay();

// Average mood rating by day of the week
daylioData.DataRepo?.SetDefaultMoodLevels();
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
  - `get_time_of_day_trends`: Analyze mood patterns across Morning, Afternoon, Evening, and Night.
  - `get_streaks`: Retrieve current and longest tracking streaks.
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
