---
trigger: always_on
---

# Understand-Anything & Knowledge Graph Rules

- **Prioritize Knowledge Graph & Domain Context**: Before performing manual codebase research, multiple file scans, or answering natural language questions about system architecture, components, or business processes, always check if `.ua/knowledge-graph.json` or `.ua/domain-graph.json` exists in the workspace root.
- **Architectural Layers & Dependencies**: Use `.ua/knowledge-graph.json` or `graphify-out/graph.json` to inspect layers (`models`, `repo`, `façade-api`), node types, complexity ratings, and relationships (`calls`, `imports`, `implements`, `contains`, `triggers`).
- **Business Domain & Process Flow Tracing**: Use domain graph artifacts to map end-to-end domain logic and step sequences (CSV file reading, header matching, record deserialization, activity/mood indexing, metric aggregation, date-range filtering) directly to their implementing source files and line ranges.
- **Maintain Graph Freshness**: When modifying architectural boundaries, domain contracts, or adding new subsystems, suggest or execute incremental graph updates to keep knowledge graph artifacts synchronized.
