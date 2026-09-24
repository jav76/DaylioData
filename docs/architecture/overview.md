# DaylioData Architecture Overview

DaylioData is a lightweight .NET library designed to parse, index, query, and analyze exported Daylio CSV mood tracking and activity data.

---

## 3-Layer Architectural Design

The library is organized into three distinct layers:

```mermaid
flowchart TB
    subgraph Layer1["1. Public Façade, Query API & Analytics Layer"]
        Facade["DaylioData<br/>(Primary Entry Point & Lifecycle Coordinator)"]
        Methods["Methods<br/>(Query, Filter & Search Extensions)"]
        Analytics["DaylioAnalytics<br/>(Habit Analytics, Trends & Streaks)"]
    end

    subgraph Layer2["2. Repository & Aggregation Layer"]
        Repo["DaylioDataRepo<br/>(In-Memory Dataset & Unique Indices)"]
        Summary["DaylioDataSummary<br/>(Aggregated Metrics & Property Reflection)"]
        Attr["SummaryPropertyAttribute<br/>(Metadata Decorator for Metric Export)"]
    end

    subgraph Layer3["3. Data Models & Low-Level I/O Layer"]
        Model["DaylioCSVDataModel<br/>(Record Schema, Timestamp & CsvHelper Index Mapping)"]
        FileAccess["DaylioFileAccess<br/>(CsvHelper Configuration, Stream & TextReader Processing)"]
    end

    Layer1 --> Layer2
    Layer2 --> Layer3
```

---

## Architectural Layers Explained

### 1. Public Façade, Query API & Analytics Layer
- **Components**: `DaylioData`, `DaylioData.Methods`, `DaylioData.DaylioAnalytics`.
- **Responsibilities**: Serves as the consumer-facing interface. Coordinates initialization from file paths, `TextReader`, or `Stream` sources. Exposes fluent query functions (`GetEntriesInRange`, `GetEntriesWithActivity`, `GetEntriesWithMood`, `GetActivityCount`, `GetEntriesWithString`, `GetAverageActivityMood`) and analytics (`GetMoodDistribution`, `GetTopActivities`, `GetLongestStreak`, `GetAverageMoodByDayOfWeek`).

### 2. Repository & Aggregation Layer
- **Components**: `DaylioDataRepo`, `DaylioDataSummary`, `SummaryPropertyAttribute`.
- **Responsibilities**: Manages the parsed in-memory collection of Daylio entries. Computes and indexes unique activities and moods case-insensitively. Produces summary statistics (total entries, total days, distinct activity counts, average entries per day, earliest/latest entries by timestamp) via reflection on decorated summary properties.

### 3. Data Models & Low-Level I/O Layer
- **Components**: `DaylioCSVDataModel`, `DaylioFileAccess`, CsvHelper.
- **Responsibilities**: Defines the CSV schema mapping with positional indices (`[Index(n)]`), DateOnly and TimeOnly deserialization, combined `Timestamp` (`DateTime`), header normalization (`snake_case` to PascalCase), and invariant culture stream/reader handling.

---

## Key Invariants & Design Principles

1. **Culture Invariance**: All CSV reading and date/time parsing must specify `CultureInfo.InvariantCulture` to prevent locale-specific date and delimiter anomalies.
2. **Deterministic Resource Management**: File streams and `CsvReader` instances must always be wrapped in `using` blocks to prevent file locks.
3. **Defensive Null Handling**: Missing CSV values (notes, activities, custom moods) must be handled gracefully without throwing unhandled exceptions.
4. **Explicit Typing & Formatting**: All source files adhere to the `.editorconfig` rules, including Allman bracing, explicit variable types, and target-typed `new()`.
