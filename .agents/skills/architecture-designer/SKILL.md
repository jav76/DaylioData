---
name: architecture-designer
description: Guides system design, codebase modularization, tech stack selection, and generates ADRs interactively with developer sign-offs. Use when starting a new project, refactoring a module, or designing system architecture.
---

# Interactive Architecture & System Design Skill

## Goal
Collaboratively architect software systems by gathering constraints, proposing structural options with trade-offs, recording architectural decisions, and producing phased scaffolding.

## Workflow Rules

### Phase 1: Clarification & Constraint Mapping
- Analyze user requirements, existing code, and operational goals.
- Ask 2 to 3 targeted questions regarding non-functional requirements (e.g., scale, latency, persistence, state management, deployment target).

### Phase 2: Option Matrix & Trade-Off Evaluation
- Propose 2 to 3 distinct architectural patterns (e.g., Repository vs. Query Service, In-Memory vs. Stream Processing).
- Present a comparative markdown table evaluating:
  - **Complexity & Velocity**
  - **Maintainability & Testability**
  - **Operational Overhead**
- **STOP:** Prompt the user to select an approach, customize elements, or challenge trade-offs. Do not proceed until the user responds.

### Phase 3: Architecture Decision Record (ADR)
- Once the approach is confirmed, generate an ADR file under `docs/architecture/ADR-001-<decision-slug>.md` using the standard ADR format (Context, Decision, Consequences, Status).

### Phase 4: Blueprint & Directory Scaffolding
- Propose the complete directory tree structure and interface boundaries.
- Produce an implementation plan artifact detailing module dependency flow.
- Await user sign-off before generating concrete implementation files.
