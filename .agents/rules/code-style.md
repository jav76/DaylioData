---
trigger: always_on
---

# DaylioData Code Style & Modern C# Standards

Adhere to the following conventions when authoring, modifying, or refactoring C# code across the DaylioData workspace (.NET 8+, C# 12+, CsvHelper, LINQ).

---

## 1. Type Declarations & `var`
- **Explicit Types Over `var`**: Do not use `var` for local variable declarations. Always use the explicit type (e.g. `string filePath = "..."`, `int totalEntries = 0`, `DaylioCSVDataModel? entry = null`).
- **Target-Typed `new()`**: When the explicit type is already declared on the left-hand side, target-typed `new()` is preferred for constructor invocations to eliminate redundancy:
  ```csharp
  // Correct
  List<DaylioCSVDataModel> records = new();
  HashSet<string> activities = new();
  DaylioDataRepo repo = new(fileAccess);

  // Avoid
  var records = new List<DaylioCSVDataModel>();
  List<DaylioCSVDataModel> records = new List<DaylioCSVDataModel>();
  ```

---

## 2. Namespaces & `using` Directives
- **File-Scoped Namespaces**: Always use file-scoped namespace declarations (`namespace DaylioData;`, `namespace DaylioData.Models;`, `namespace DaylioData.Repo;`) to save horizontal indentation.
- **`using` Directive Placement & Ordering**: Place `using` directives outside the namespace at the top of the file. Sort `System` and `System.*` directives first, followed alphabetically by other namespaces. Remove unused directives.
- **Global Usings**: Confine global usings to dedicated files (e.g., `GlobalUsings.cs`) for ubiquitous namespaces only.

---

## 3. Braces, Line Width & Layout (Allman Style)
- **Allman Bracing**: Opening braces `{` must always be placed on their own line at the same indentation level as the parent declaration (classes, methods, properties, control flow statements).
- **Line Length Target**: Target line widths under 120 characters. Sensible exceptions apply to long URL strings, regex patterns, or attribute annotations.
- **Whitespace & Formatting**: Clean whitespace and blank lines to separate logical blocks or enhance clarity are encouraged.

---

## 4. Parameter & Invocation Wrapping
- **Multi-Line Signatures & Calls**: When a method signature, constructor definition, or method invocation wraps across lines, place each parameter/argument on its own line indented by 4 spaces:
  ```csharp
  public static IEnumerable<DaylioCSVDataModel>? GetEntriesInRange(
      DaylioData daylioData,
      DateTime startDate,
      DateTime endDate)
  {
      // ...
  }
  ```
- **Fluent / LINQ Chains**: Wrap multi-step LINQ or builder invocations so that each method call starts on a new indented line:
  ```csharp
  List<DaylioCSVDataModel> filtered = entries
      .Where(e => e.Mood == targetMood)
      .OrderBy(e => e.FullDate)
      .ToList();
  ```
- **Initializers**: Multi-line object and collection initializers should place each property/element on its own line with a trailing comma.

---

## 5. Expression-Bodied Members vs Block Bodies
- **Single-Line Members**: Use expression bodies (`=>`) for single-line properties, getters, indexers, and short single-line helper methods:
  ```csharp
  public DaylioDataRepo? DataRepo => _dataRepo;
  public double AverageEntriesPerDay => TotalEntries / (double)TotalDays;
  ```
- **Multi-Line Members & Constructors**: Use full block bodies with braces for multi-line methods and all constructors.

---

## 6. Pattern Matching, Switch Expressions & Null Checking
- **Null Checking**: Use `is null` and `is not null` instead of `== null` / `!= null`.
- **Pattern Matching**: Prefer type pattern matching (`if (item is DaylioCSVDataModel entry)`) over `as` casting followed by null checks.
- **Switch Expressions**: Prefer switch expressions (`state switch { ... }`) when mapping or returning values over traditional `switch` statements:
  ```csharp
  string label = mood switch
  {
      "rad" => "Great",
      "good" => "Good",
      "meh" => "Average",
      "bad" => "Bad",
      "awful" => "Terrible",
      _ => mood
  };
  ```

---

## 7. Naming, Modifiers & Qualification
- **Private Fields**: Prefix private and internal instance fields with an underscore and use `_camelCase` (e.g. `private DaylioDataRepo? _dataRepo;`, `private readonly DaylioFileAccess _fileAccess;`).
- **Constants & Magic Numbers**: Constants (`const`) must use `CAPS_CASE` / `SCREAMING_SNAKE_CASE` (e.g. `private const string FULL_DATE_HEADER = "full_date";`). Avoid magic numbers throughout the codebase unless the meaning is immediately obvious from its context (such as standard math or index boundaries like `0` or `1`). Any non-obvious numeric values (including buffer sizes, default limits, timeouts, and scaling factors) must be declared as descriptive, all-caps `const` fields.
- **No `this.` Qualifier**: Avoid `this.` qualification unless strictly necessary to disambiguate shadowed identifiers.
- **Explicit Accessibility**: Always explicitly declare accessibility modifiers (`private`, `public`, `protected`, `internal`) on all types and members.
- **`readonly` Modifier**: Apply `readonly` to all fields and properties that are assigned only during declaration or in the constructor.

---

## 8. Modern Types & Primary Constructors
- **Records**: Use `record` or `record struct` for immutable data models, DTOs, and event payloads where value semantics and immutability are desired.
- **Primary Constructors**: Use primary constructors for records/DTOs and concise service classes. Use traditional constructor blocks when initialization requires input validation, defensive copying, or complex setup logic.

---

## 9. CSV Parsing, Data Safety & Culture Standards
- **Culture Invariance**: Always specify `CultureInfo.InvariantCulture` for CSV reader configurations, date/time parsing, and number formatting to avoid locale-specific delimiters or date formats corrupting data.
- **DateOnly & TimeOnly Handling**: Utilize modern .NET `DateOnly` and `TimeOnly` types for Daylio's date and time fields rather than legacy `DateTime` when time or date components are absent or independent.
- **Resource Disposal & Stream Safety**: Ensure all `StreamReader` and `CsvReader` instances are strictly managed with `using` declarations or statements to guarantee immediate deterministic disposal of file handles.
- **Defensiveness & Null Safety**: Handle null or empty CSV cells gracefully (such as empty notes, omitted activities, or missing tags) without throwing unhandled null reference exceptions.
