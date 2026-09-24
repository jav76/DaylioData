# DaylioData

A .NET library for reading, querying, and analyzing exported Daylio CSV mood tracking and activity data.

## Features

- **Flexible Ingestion**: Parse Daylio CSV data directly from file paths, `TextReader`, or `Stream` instances.
- **Fluent Query API**: Filter and search entries by date-time range, mood, activity, or note text.
- **Rich Analytics**: Calculate mood distribution, top activities, tracking streaks, and day-of-week trends.
- **In-Memory Summary**: Automatic calculation of total entries, days tracked, distinct activities, and earliest/latest entries.

## Usage

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

Query entries fluently using extension methods:

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

// Search note text
IEnumerable<DaylioCSVDataModel>? projectNotes = daylioData.GetEntriesWithString("project");
```

### Habit & Mood Analytics

Compute insights and trends using `DaylioAnalytics`:

```csharp
using DaylioData;

// Mood frequency breakdown
IReadOnlyDictionary<string, int> distribution = daylioData.GetMoodDistribution();

// Top 5 most frequent activities
IReadOnlyList<KeyValuePair<string, int>> topActivities = daylioData.GetTopActivities(5);

// Longest daily tracking streak
int longestStreak = daylioData.GetLongestStreak();

// Average mood rating by day of the week
daylioData.DataRepo?.SetDefaultMoodLevels();
IReadOnlyDictionary<DayOfWeek, decimal> weekdayMoods = daylioData.GetAverageMoodByDayOfWeek();
```

## Testing & Verification

Run the test suite across all targets:

```bash
dotnet test src/DaylioData/DaylioData.sln
```

## NuGet Package

NuGet: https://www.nuget.org/packages/DaylioData/
