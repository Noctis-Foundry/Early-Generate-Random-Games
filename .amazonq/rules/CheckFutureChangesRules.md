# AI Review Rules

## Purpose

This document defines strict rules for AI when reviewing the project.
The AI must **only analyze, document issues, and propose future changes**.
Direct modification of the project source code is **strictly prohibited**.

---

# Critical Restrictions

## File Modification Policy

During bug search, architecture analysis, or review:

AI **must NOT modify any project files**.

Allowed actions:

* Read source code
* Analyze architecture
* Detect bugs
* Detect potential improvements
* Document findings

The **only file AI is allowed to modify or create** is:

```
future_changes.md
```

No exceptions.

AI must never:

* Refactor code
* Auto-fix code
* Rewrite files
* Reorganize folders
* Change configuration files
* Modify database logic
* Update UI code

All suggestions must be documented instead.

---

# Documentation Output Rules

All findings must be written to:

```
future_changes.md
```

The structure must follow **exactly the same format** as the project task document.

---

# Required Markdown Structure

The AI must follow this strict structure.

```
#priority Приоритетные задачи
---

#fix Fix UX experience
---

#bug Fix Bug
---

#refactor Refactoring plan
---

#refactor View Models
---

#refactor Window Base
---

#refactor View classes
---

#dowork Task Runner
---
```

---

# Writing Rules

Each task must follow the format:

```
- [ ] Task description
```

Subtasks:

```
     - Subtask description
```

Example:

```
#bug Fix Bug
---

- [ ] Fix incorrect listener initialization after Admin Panel refresh
     - Review listener lifecycle
     - Ensure proper disposal
     - Prevent duplicate subscriptions
```

Completed tasks must **never be marked as completed by AI**.

AI must always use:

```
[ ]
```

Never:

```
[x]
```

Completion is handled only by the developer.

---

# Obsidian Navigation Tags

To improve navigation inside Obsidian, the AI must include tags.

At the end of each section add tags:

```
#ai #review #todo
```

Example:

```
#bug Fix Bug
---

- [ ] Investigate potential memory leak in processing window
     - Check DispatcherTimer disposal
     - Check event listeners cleanup
     - Verify window removal from visual tree

#ai #review #todo
```

---

# Analysis Strategy

When analyzing the project, AI must focus on:

### Architecture

* Separation of concerns
* ViewModel responsibilities
* Service abstraction
* Dependency injection
* Lifecycle management

### Concurrency

* Task execution
* Exception handling
* Semaphore usage
* Async patterns

### UI Lifecycle

* Window lifecycle
* ViewModel binding
* Dispatcher usage
* Memory leaks

### Performance

* Database queries
* Event listeners
* Data loading strategies

---

# Prohibited Behavior

AI must never:

* Perform automated refactoring
* Generate patches
* Rewrite architecture automatically
* Modify repository structure
* Change database schema
* Apply fixes directly

The AI acts **only as an analysis and documentation assistant**.

---

# Expected Workflow

1. AI analyzes the project.
2. AI identifies issues or improvements.
3. AI writes structured tasks to `future_changes.md`.
4. Developer reviews and decides what to implement.

AI does not implement changes.

---

# Goal

The goal of this system is to:

* Keep full developer control
* Prevent unsafe automated changes
* Maintain architectural clarity
* Use AI as an **engineering review assistant**
* Keep future improvements documented in a structured format
