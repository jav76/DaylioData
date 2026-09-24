# Contributing to DaylioData

Thank you for your interest in contributing to DaylioData!

---

## Quickstart for Contributors

### 1. Prerequisites
- **[.NET 8+ SDK](https://dotnet.microsoft.com/)** (`v8.0.0+`)

### 2. Fork & Clone
```bash
git clone https://github.com/jav76/DaylioData.git
cd DaylioData
git checkout -b feature/my-feature
```

### 3. Build & Format
```bash
# Restore and build solution
dotnet build src/DaylioData/DaylioData.sln

# Format code
dotnet format src/DaylioData/DaylioData.sln

# Verify formatting without changes
dotnet format src/DaylioData/DaylioData.sln --verify-no-changes --severity warn
```

---

## Coding Standards

DaylioData adheres to strict C# conventions defined in `.editorconfig` and `.agents/rules/code-style.md`:
- **No `var`**: Always use explicit types (`string filePath = "..."`, `int total = 0`).
- **Target-typed `new()`**: Use target-typed `new()` when the type is declared on the left (`List<DaylioCSVDataModel> records = new();`).
- **File-scoped namespaces**: Always use file-scoped namespaces (`namespace DaylioData;`).
- **Allman bracing**: Opening brace `{` on its own line at the parent indentation level.
- **Culture Invariance**: Always specify `CultureInfo.InvariantCulture` for CSV reader configurations and date/time formatting.
- **Structured logging**: Never interpolate strings into logger calls (use `_logger.LogInformation("Loaded {Count} records", count)`).

---

## Submitting a Pull Request

1. Verify that your code builds cleanly (`dotnet build src/DaylioData/DaylioData.sln --configuration Release`).
2. Run code formatting (`dotnet format src/DaylioData/DaylioData.sln`).
3. Verify that `dotnet format --verify-no-changes --severity warn` reports zero issues.
4. Submit your PR against the `main` branch with a clear title and description of your changes.
