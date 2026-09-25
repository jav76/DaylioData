# DaylioData v1.0.0 Migration Guide

This guide details breaking changes and migration steps for upgrading from **DaylioData 0.1.x** to **DaylioData 1.0.0-beta.1** (and the upcoming 1.0.0 release candidate).

---

## Overview of Key Architectural Modernizations

The `1.0.0-beta.1` release eliminates legacy technical debt, provides thread-safe invariants, and modernizes the library for .NET 8 and .NET 10:

1. **Elimination of Deprecated Static Methods & Ambient State**: Removed all `[Obsolete]` static methods and static shared fields from the legacy `Methods` class.
2. **Direct Instance Queries on [`DaylioData`](file:///home/jaret/Documents/GitHub/DaylioData/src/DaylioData/DaylioData.cs)**: Query and filtering methods are now first-class instance methods on the [`DaylioData`](file:///home/jaret/Documents/GitHub/DaylioData/src/DaylioData/DaylioData.cs) façade.
3. **[`DaylioQueryExtensions`](file:///home/jaret/Documents/GitHub/DaylioData/src/DaylioData/DaylioQueryExtensions.cs)**: Replaces the legacy `Methods` class name for fluent query extensions.
4. **Non-Null Collection Returns**: Filtering methods now return non-null `IReadOnlyList<DaylioCSVDataModel>` (yielding `Array.Empty<DaylioCSVDataModel>()` when unmatched) instead of `null`.
5. **Immutable Records & Pre-Cached Collections**: [`DaylioCSVDataModel`](file:///home/jaret/Documents/GitHub/DaylioData/src/DaylioData/Models/DaylioCSVDataModel.cs) is now an immutable C# `record` with `{ get; init; }` properties and pre-cached `ActivitiesCollection`.
6. **Guaranteed Non-Null Invariants & Fail-Fast Constructors**: [`DataRepo`](file:///home/jaret/Documents/GitHub/DaylioData/src/DaylioData/DaylioData.cs) and [`DataSummary`](file:///home/jaret/Documents/GitHub/DaylioData/src/DaylioData/DaylioData.cs) are guaranteed non-null on all initialized [`DaylioData`](file:///home/jaret/Documents/GitHub/DaylioData/src/DaylioData/DaylioData.cs) instances. Constructors validate file existence and throw standard I/O exceptions on failure. Non-throwing alternatives (`TryLoad`, `TryLoadAsync`) are provided.
7. **Single-Pass Cached Metrics**: [`DaylioDataSummary`](file:///home/jaret/Documents/GitHub/DaylioData/src/DaylioData/Models/DaylioDataSummary.cs) now computes summary metrics in a single O(N) pass and caches min/max timestamps rather than executing repeated O(N log N) sorts on property access.

---

## Detailed Breaking Changes & Migration Steps

### 1. Query Method Invocations

#### Before (0.1.x)
```csharp
// Deprecated static ambient methods:
Methods.InitData(daylioData);
IEnumerable<DaylioCSVDataModel>? inRange = Methods.GetEntriesInRange(startDate, endDate);
IEnumerable<DaylioCSVDataModel>? codingEntries = Methods.GetEntriesWithActivity("coding");

// Extension method on legacy class name:
IEnumerable<DaylioCSVDataModel>? matches = Methods.GetEntriesInRange(daylioData, startDate, endDate);
```

#### After (1.0.0-beta.1)
```csharp
// Direct instance methods on DaylioData:
IReadOnlyList<DaylioCSVDataModel> inRange = daylioData.GetEntriesInRange(startDate, endDate);
IReadOnlyList<DaylioCSVDataModel> codingEntries = daylioData.GetEntriesWithActivity("coding");

// Or using DaylioQueryExtensions:
IReadOnlyList<DaylioCSVDataModel> matches = DaylioQueryExtensions.GetEntriesInRange(daylioData, startDate, endDate);
```

---

### 2. Query Return Types & Null Handling

In `0.1.x`, querying for an activity that was not logged returned `null`, forcing callers to use null checks or the null-forgiving operator `!`. In `1.0.0-beta.1`, query methods return empty `IReadOnlyList<DaylioCSVDataModel>` instances (`Array.Empty<T>()`), following .NET Framework Design Guidelines.

#### Before (0.1.x)
```csharp
IEnumerable<DaylioCSVDataModel>? entries = daylioData.GetEntriesWithActivity("skydiving");
if (entries is not null)
{
    foreach (DaylioCSVDataModel entry in entries)
    {
        // ...
    }
}
```

#### After (1.0.0-beta.1)
```csharp
IReadOnlyList<DaylioCSVDataModel> entries = daylioData.GetEntriesWithActivity("skydiving");

// Safe to iterate immediately without null checks or null-forgiving operators:
foreach (DaylioCSVDataModel entry in entries)
{
    // ...
}

// Check count directly with O(1) performance:
if (entries.Count > 0)
{
    // ...
}
```

---

### 3. File Loading & Fail-Fast Constructors

In `0.1.x`, calling `new DaylioData("missing_file.csv")` silently swallowed errors and left `DataRepo.CSVData` as `null`. In `1.0.0-beta.1`, constructors validate inputs and throw `FileNotFoundException` or `IOException`. Callers desiring safe, non-throwing loading should use `DaylioData.TryLoad` or `DaylioData.TryLoadAsync`.

#### Before (0.1.x)
```csharp
DaylioData data = new("data.csv");
if (data.DataRepo?.CSVData is null)
{
    // Silent failure
}
```

#### After (1.0.0-beta.1)
```csharp
// Option A: Fail-fast constructors with standard exception handling
try
{
    DaylioData data = new("data.csv");
}
catch (FileNotFoundException ex)
{
    // Handle missing file
}

// Option B: Non-throwing TryLoad pattern
if (DaylioData.TryLoad("data.csv", out DaylioData? data))
{
    // File loaded successfully, data is guaranteed non-null
    Console.WriteLine(data.DataSummary.TotalEntries);
}

// Option C: Non-throwing async loading
(bool success, DaylioData? asyncData) = await DaylioData.TryLoadAsync("data.csv");
if (success && asyncData is not null)
{
    // Process asyncData
}
```

---

### 4. [`DaylioCSVDataModel`](file:///home/jaret/Documents/GitHub/DaylioData/src/DaylioData/Models/DaylioCSVDataModel.cs) Immutability

[`DaylioCSVDataModel`](file:///home/jaret/Documents/GitHub/DaylioData/src/DaylioData/Models/DaylioCSVDataModel.cs) is now declared as `public sealed record DaylioCSVDataModel : IEquatable<DaylioCSVDataModel>`. Property setters `{ get; set; }` have been replaced with `{ get; init; }`. Modifying records after parsing must use record `with` expressions.

#### Before (0.1.x)
```csharp
DaylioCSVDataModel model = new() { ... };
model.Mood = "good"; // In-place mutation
```

#### After (1.0.0-beta.1)
```csharp
DaylioCSVDataModel model = new() { ... };
DaylioCSVDataModel updated = model with { Mood = "good" }; // Pure record copy
```

---

### 5. Non-Null Façade Invariants

`DataRepo` and `DataSummary` on [`DaylioData`](file:///home/jaret/Documents/GitHub/DaylioData/src/DaylioData/DaylioData.cs) are now typed as non-nullable (`public DaylioDataRepo DataRepo` and `public DaylioDataSummary DataSummary`). The public parameterless constructor `DaylioData()` has been removed.

#### Before (0.1.x)
```csharp
int count = daylioData.DataSummary?.TotalEntries ?? 0;
IReadOnlyList<string> activities = daylioData.DataRepo?.Activities.ToList() ?? new();
```

#### After (1.0.0-beta.1)
```csharp
int count = daylioData.DataSummary.TotalEntries;
HashSet<string> activities = daylioData.DataRepo.Activities;
```
