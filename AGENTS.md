# DaylioData Agent Guidelines & Standards

This document defines core conventions and operational rules for AI coding assistants working within the **DaylioData** repository.

---

## 1. Quick Reference & Commands

- **Build**: `dotnet build src/DaylioData/DaylioData.sln`
- **Format Verification**: `dotnet format src/DaylioData/DaylioData.sln --verify-no-changes --severity warn`
- **Auto-Format**: `dotnet format src/DaylioData/DaylioData.sln`
- **Release Build**: `dotnet build src/DaylioData/DaylioData.sln --configuration Release`

---

## 2. Agent Rule Files

Detailed rules and operational standards are located in `.agents/`:
- **Code Style & C# Standards**: [.agents/rules/code-style.md](file:///home/jaret/Documents/GitHub/DaylioData/.agents/rules/code-style.md)
- **Logging Standards**: [.agents/rules/logging.md](file:///home/jaret/Documents/GitHub/DaylioData/.agents/rules/logging.md)
- **Knowledge Graph & Architecture Context**: [.agents/rules/understand-context.md](file:///home/jaret/Documents/GitHub/DaylioData/.agents/rules/understand-context.md)
- **Architectural Code Reviewer Agent**: [.agents/agents/code-reviewer.md](file:///home/jaret/Documents/GitHub/DaylioData/.agents/agents/code-reviewer.md)
- **Code Reviewer Skill**: [.agents/skills/code-reviewer/SKILL.md](file:///home/jaret/Documents/GitHub/DaylioData/.agents/skills/code-reviewer/SKILL.md)
- **Architecture Designer Skill**: [.agents/skills/architecture-designer/SKILL.md](file:///home/jaret/Documents/GitHub/DaylioData/.agents/skills/architecture-designer/SKILL.md)

---

## 3. Core Architectural Boundaries

Refer to [docs/architecture/overview.md](file:///home/jaret/Documents/GitHub/DaylioData/docs/architecture/overview.md) for full architectural documentation:
1. **Public Façade & Query APIs (`DaylioData`, `Methods`)**: Entry point for consumers, high-level querying, and date-range filtering.
2. **Repository & In-Memory Indexes (`DaylioDataRepo`, `DaylioDataSummary`, `SummaryPropertyAttribute`)**: In-memory dataset management, distinct activity/mood indexers, and reflective summary generation.
3. **Data Models & Low-Level I/O (`DaylioCSVDataModel`, `DaylioFileAccess`, CsvHelper)**: CSV schema definitions, positional index attributes, culture invariant stream processing, and safe resource disposal.

---

## 4. Key Invariants & Non-Negotiables

1. **No `var`**: Always use explicit types (`string path = "..."`, `int count = 0`).
2. **Target-typed `new()`**: Use `List<DaylioCSVDataModel> list = new();` when the type is declared on the left.
3. **File-scoped Namespaces**: Always use file-scoped namespaces (`namespace DaylioData;`).
4. **Allman Bracing**: Opening brace `{` on its own line at the parent indentation level.
5. **Culture Invariance**: Always specify `CultureInfo.InvariantCulture` for CSV reader configurations and date/time formatting.
6. **Deterministic Disposal**: Always wrap `StreamReader` and `CsvReader` instances in `using` blocks.
7. **No Direct Console Output**: Never use `Console.WriteLine` in library code; use structured logging or handled exceptions.
