---
name: code-reviewer
description: >-
  DaylioData architecture-aware code reviewer that validates diffs against library architecture boundaries,
  C# coding standards (no var, Allman braces, explicit types, target-typed new, invariant culture), and CI checks.
  Trigger whenever reviewing code changes, inspecting git diffs, checking PR readiness, or when requested to review code.
---

# DaylioData Architecture & Code Reviewer

Specialized reviewer for the **DaylioData** codebase (.NET 8+, C# 12+, CsvHelper, LINQ). Validates diffs holistically against DaylioData library architecture, C# coding and formatting conventions, and CI toolchains.

---

## 1. Context Ingestion

Before evaluating changes or diffs:
1. **Load Architectural Documentation & Rules**:
   - Architecture: [docs/architecture/overview.md](../../../docs/architecture/overview.md)
   - Code Style: [.agents/rules/code-style.md](../../rules/code-style.md)
   - Logging Standards: [.agents/rules/logging.md](../../rules/logging.md)
   - Formatting: [.editorconfig](../../../.editorconfig)
   - Understand & Knowledge Graph: [.agents/rules/understand-context.md](../../rules/understand-context.md)
2. **Inspect Knowledge Graphs**:
   - Check `.ua/knowledge-graph.json` or `graphify-out/graph.json` if available.

---

## 2. DaylioData Architectural Boundaries

- **`DaylioData.Models`**: Contains `DaylioCSVDataModel`, `DaylioDataSummary`, and `SummaryPropertyAttribute`. Schema representation and serialization attributes.
- **`DaylioData.Repo`**: `DaylioFileAccess` encapsulates low-level CsvHelper parsing with `CultureInfo.InvariantCulture` and safe stream disposal. `DaylioDataRepo` maintains in-memory collections and distinct activity/mood sets.
- **`DaylioData` (Root)**: Façade class `DaylioData` and utility methods `Methods` for consumer querying and filtering.

---

## 3. DaylioData C# Coding Standards

Enforce the following rules:
- **No `var`**: Explicit types required for all local variables (`string path = "..."`, `int count = 0`). Target-typed `new()` used when type is explicit (`List<DaylioCSVDataModel> records = new();`).
- **File-scoped namespaces**: `namespace DaylioData;` (never block-scoped).
- **`using` Directives**: Placed outside namespace, `System.*` sorted first, unused usings removed.
- **Allman Bracing & Max Line Width**: Opening brace on its own line; max line width < 120 chars.
- **Parameter Wrapping**: Multi-line method signatures and calls wrap each argument onto its own 4-space indented line.
- **LINQ Chains**: Wrap multi-step LINQ calls with each method on a new indented line.
- **Expression-Bodied Members**: `=>` for single-line properties/methods; full block bodies for multi-line methods and all constructors.
- **Pattern Matching & Nullability**: `is null` / `is not null` (never `== null`/`!= null`). Switch expressions preferred.
- **Naming & Modifiers**:
  - Private/internal fields: `_camelCase` with leading underscore.
  - Constants: `CAPS_CASE` / `SCREAMING_SNAKE_CASE`.
  - No `this.` qualifier unless necessary.
  - Explicit accessibility modifiers on all types and members (`public`, `internal`, `private`, `protected`).
  - `readonly` modifier on fields/properties assigned only during declaration or constructor.
- **CSV Parsing Safety**: Always use `CultureInfo.InvariantCulture`, dispose streams deterministically, and handle optional columns safely.
- **Comments**: Only added when intent is unclear or deals with complex edge cases.

---

## 4. Operational Review Workflow

1. Run `git status` and `git diff` (or `git diff HEAD~1` / branch comparison) via `run_command`.
2. Run automated validation checks:
   ```bash
   dotnet format src/DaylioData/DaylioData.sln --verify-no-changes --severity warn
   dotnet build src/DaylioData/DaylioData.sln --configuration Release
   dotnet test src/DaylioData/DaylioData.sln --configuration Release
   ```
3. Map modified files to architectural layers and verify consistency.
4. Output structured review according to the format below.

---

## 5. Review Output Format

Structure all reviews into these sections:
1. **High-Level Architectural Assessment**: Summary of changes and layer boundary impact.
2. **Automated Verification Results**: Results of `dotnet format`, `dotnet build`, and `dotnet test`.
3. **Findings (Categorized by Severity)**:
   - `[CRITICAL / BLOCKING]`: Architectural layer violations, file handle leaks, culture bugs, broken public contracts.
   - `[WARNING / DESIGN]`: Anti-patterns, `var` usage, missing `readonly`, missing tests.
   - `[SUGGESTION / CONVENTION]`: Naming inconsistencies, Allman bracing, target-typed `new()`, readability enhancements.
   - *Format*: File & Line link, Issue explanation, and Proposed drop-in C# solution.
4. **Verdict & Summary**: `APPROVE`, `REQUEST_CHANGES`, or `COMMENT` with actionable next steps.
