# DaylioData Architecture Overview

DaylioData is a .NET library and Model Context Protocol (MCP) server designed to parse, index, query, analyze, and expose exported Daylio CSV mood tracking and activity data.

---

## 4-Layer Architectural Design

The solution is organized into four distinct architectural layers:

```mermaid
flowchart TB
    subgraph Layer0["0. Model Context Protocol (MCP) Server Layer (DaylioData.Mcp)"]
        Server["McpServer<br/>(Stdio JSON-RPC 2.0 Loop & State Coordinator)"]
        Tools["ToolHandler<br/>(9 Analytical & Query MCP Tools)"]
        Resources["ResourceHandler<br/>(daylio:// Summary, Activities, Moods)"]
        Prompts["PromptHandler<br/>(Weekly Reflection & Habit Analysis Templates)"]
        Server --> Tools
        Server --> Resources
        Server --> Prompts
    end

    subgraph Layer1["1. Public Façade, Query API, Analytics & Export Layer (DaylioData)"]
        Facade["DaylioData<br/>(Primary Entry Point & Lifecycle Coordinator)"]
        Methods["Methods<br/>(Thread-Safe Query, Filter & Search Extensions)"]
        Analytics["DaylioAnalytics<br/>(Habit Analytics, Correlations, Streaks, Time-of-Day)"]
        Export["DaylioExport<br/>(Markdown Reports & JSON Serialization)"]
        Facade --> Methods
        Facade --> Analytics
        Facade --> Export
    end

    subgraph Layer2["2. Repository & Aggregation Layer"]
        Repo["DaylioDataRepo<br/>(In-Memory Dataset & Unique Indices)"]
        Summary["DaylioDataSummary<br/>(Aggregated Metrics & Property Reflection)"]
        Attr["SummaryPropertyAttribute<br/>(Metadata Decorator for Metric Export)"]
        Repo --> Summary
        Summary --> Attr
    end

    subgraph Layer3["3. Data Models & Low-Level I/O Layer"]
        Model["DaylioCSVDataModel<br/>(Record Schema, Timestamp & CsvHelper Index Mapping)"]
        FileAccess["DaylioFileAccess<br/>(CsvHelper Configuration, Stream & TextReader Processing)"]
        Repo --> Model
        Repo --> FileAccess
    end

    Layer0 --> Layer1
    Layer1 --> Layer2
    Layer2 --> Layer3
```

---

## Architectural Layers Explained

### 0. Model Context Protocol (MCP) Server Layer (`DaylioData.Mcp`)
- **Components**: `McpServer`, `ToolHandler`, `ResourceHandler`, `PromptHandler`, `Program`.
- **Responsibilities**: Implements the Model Context Protocol over stdio using JSON-RPC 2.0. Serves AI agents (such as Claude Desktop, Cursor, and Antigravity) with 9 analytical tools, 3 resource endpoints (`daylio://summary`, `daylio://activities`, `daylio://moods`), and 2 guided prompt workflows. Packaged as a standalone .NET Global Tool (`daylio-mcp`).

### 1. Public Façade, Query API, Analytics & Export Layer (`DaylioData`)
- **Components**: `DaylioData`, `DaylioData.Methods`, `DaylioData.DaylioAnalytics`, `DaylioData.DaylioExport`.
- **Responsibilities**: Serves as the consumer-facing interface. Coordinates initialization from file paths, `TextReader`, or `Stream` sources. Exposes thread-safe fluent query functions (`GetEntriesInRange`, `GetEntriesWithActivity`, `GetEntriesWithMood`, `GetActivityCount`, `GetEntriesWithString`), advanced habit analytics (`GetActivityMoodImpact`, `GetAllActivityMoodImpacts`, `GetMoodByTimeOfDay`, `GetStreakDetails`, `GetMoodDistribution`, `GetTopActivities`), and export formats (`GenerateMarkdownReport`, `ToJson`).

### 2. Repository & Aggregation Layer
- **Components**: `DaylioDataRepo`, `DaylioDataSummary`, `SummaryPropertyAttribute`.
- **Responsibilities**: Manages the parsed in-memory collection of Daylio entries. Computes and indexes unique activities and moods case-insensitively. Produces summary statistics (total entries, total days, distinct activity counts, average entries per day, earliest/latest entries by timestamp) via reflection on decorated summary properties.

### 3. Data Models & Low-Level I/O Layer
- **Components**: `DaylioCSVDataModel`, `DaylioFileAccess`, CsvHelper.
- **Responsibilities**: Defines the CSV schema mapping with positional indices (`[Index(n)]`), DateOnly and TimeOnly deserialization, combined `Timestamp` (`DateTime`), header normalization (`snake_case` to PascalCase), and invariant culture stream/reader handling.

---

## Key Invariants & Design Principles

1. **Culture Invariance**: All CSV reading, date/time parsing, and metric number formatting must specify `CultureInfo.InvariantCulture` to prevent locale-specific date and delimiter anomalies.
2. **Deterministic Resource Management**: File streams and `CsvReader` instances must always be wrapped in `using` blocks to prevent file locks.
3. **Thread-Safe Facade & Extensions**: All query and analytics extension methods operate directly on the target instance without mutating static shared state.
4. **Clean Stdio Separation**: The MCP server strictly directs JSON-RPC protocol communication to standard output and diagnostic logging to standard error, ensuring protocol integrity.
5. **Defensive Null Handling**: Missing CSV values (notes, activities, custom moods) must be handled gracefully without throwing unhandled exceptions.
6. **Explicit Typing & Formatting**: All source files adhere to repository standards, including Allman bracing, explicit variable types (no `var`), and target-typed `new()`.
