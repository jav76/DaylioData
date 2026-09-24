---
trigger: always_on
---

# DaylioData Modern Logging & Diagnostic Standards

Adhere to the following conventions when authoring, modifying, or refactoring diagnostics and logging across the DaylioData codebase.

---

## 1. Core Framework & Facade
- **Microsoft.Extensions.Logging / Structured Logging**: Standardize on structured logging via `ILogger` or explicit diagnostic callbacks.
- **No Direct Console or Debug Output**: Never use `Console.WriteLine` or `Trace.WriteLine` in library code. All diagnostic output must flow through structured logging abstractions or handled exceptions.

---

## 2. Structured Message Templates (Never Use `$""` Interpolation)
- **Named Placeholders**: Always use message templates with descriptive tokens. This preserves structured parameters for telemetry and log sinks:
  ```csharp
  // Correct
  _logger.LogInformation(
      "Parsed {RecordCount} Daylio records from {FilePath} across {DayCount} days",
      records.Count,
      filePath,
      summary.TotalDays);

  // Incorrect (AVOID)
  _logger.LogInformation($"Parsed {records.Count} records from {filePath}");
  ```
- **Performance**: String interpolation causes immediate string allocations and heap allocations even when the target log level is disabled. Message templates are only formatted if the log level is active.

---

## 3. Log Levels & Usage Guidelines
| Level | Semantic Meaning & When to Use | Default in Release | Default in Debug |
|---|---|---|---|
| **Trace** | Granular per-row CSV parsing events, column-by-column header mapping. | Disabled | Disabled |
| **Debug** | Internal repository state changes, activity deduplication, mood indexing. | Disabled | **Active** |
| **Information** | High-level milestones (e.g. CSV file loaded, dataset initialized, summary generated). | Disabled | Active |
| **Warning** | Recoverable anomalies, unexpected headers, skipped blank records, unmapped fields. | **Active** | Active |
| **Error** | File access failures, corrupted CSV structures, unhandled parser errors. | Active | Active |
| **Critical** | Fatal system failures or catastrophic memory exhaustion during large file parsing. | Active | Active |

---

## 4. Zero-Overhead Level Guards
- When computing expensive parameters or aggregations for a log message, always wrap the computation in an `IsEnabled` guard:
  ```csharp
  if (_logger.IsEnabled(LogLevel.Debug))
  {
      string detailedSummary = summary.GetSummary();
      _logger.LogDebug("Daylio dataset summary details:\n{Summary}", detailedSummary);
  }
  ```

---

## 5. Exception Handling & Logging
- **Pass the Exception Object**: Always pass the `Exception` object as the first parameter so the full stack trace and inner exceptions are preserved:
  ```csharp
  try
  {
      // ...
  }
  catch (IOException ex)
  {
      _logger.LogError(ex, "Failed to read Daylio CSV file at {FilePath}", filePath);
      throw;
  }
  ```
- Do not log only `ex.Message` without passing the exception instance.
