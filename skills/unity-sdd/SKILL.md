---
name: unity-sdd
description: >
  Takes a Unity game PRD (Product Requirements Document) and generates a structured
  Software Design Document (SDD) with actionable development tasks organized into
  sprints. Each task includes implementation details, file paths, dependencies, and
  acceptance criteria — ready to be followed step-by-step to build the game in Unity.
  Use this skill whenever the user has a PRD or game spec and wants to turn it into
  a development plan, task list, implementation roadmap, or "what do I build first"
  breakdown. Also trigger when the user says "create tasks from this PRD", "break this
  down into dev tasks", "make an SDD", "plan the implementation", or "what should I
  code first" in the context of a Unity game project.
---

# Unity Game SDD Skill

You are generating a Software Design Document — a concrete, ordered development plan that a solo Unity developer can follow task-by-task to build their game. The input is a PRD (Product Requirements Document) that includes a Feature Breakdown and System Contracts section.

## How to use this skill

1. Read the user's PRD carefully — especially Section 5 (SDD Handoff)
2. If no PRD exists yet, tell the user they should create one first (suggest the unity-prd skill)
3. Generate the SDD following the template below
4. Save as a markdown file in the workspace

## Reading the PRD

The PRD's Section 5 is your primary input:
- **5.1 Feature Breakdown** tells you WHAT to build, with acceptance criteria
- **5.2 System Contracts** tells you HOW systems connect, with C# interfaces

But don't ignore the rest of the PRD — sections 1-4 provide critical context:
- Section 3 (Unity Architecture) tells you about project structure, scene hierarchy, and technical decisions already made
- Section 4 (Milestones) tells you what's MVP vs. post-MVP, which directly affects task ordering
- Section 2 (Core Mechanics) helps you understand the player experience, which matters when you're deciding how to test things

## SDD Template

```markdown
# [Game Title] — Software Design Document

## Overview
- **Source PRD**: [filename or link]
- **Total tasks**: [count]
- **Estimated sprints**: [count]
- **MVP tasks**: [count of tasks needed for minimum playable version]

---

## Sprint Plan

Tasks are organized into sprints. Each sprint should result in something testable — you should be able to run the game and see/verify something new after each sprint.

### Sprint 1: [Sprint Name — e.g., "Project Skeleton & Core Movement"]
**Goal**: What's playable/testable after this sprint
**Estimated effort**: [X tasks, roughly Y hours]

---

#### Task 1.1: [Task Name]
- **Feature**: [Which feature from PRD 5.1 this implements, or "Infrastructure"]
- **System**: [Which system from PRD 5.2 this belongs to, if any]
- **Priority**: MVP / Post-MVP / Polish
- **Depends on**: [Task IDs this requires, or "None"]

**What to build:**
[2-5 sentences describing what this task produces. Be specific — name the classes, the scene, the prefab.]

**Files to create/modify:**
- `Assets/Scripts/Core/GameManager.cs` — [what this file does]
- `Assets/Scenes/Bootstrap.unity` — [what to set up in this scene]

**Implementation notes:**
[Key decisions, patterns to use, gotchas to watch for. This is where you translate the PRD's architecture decisions into practical guidance.]

**Acceptance criteria:**
- [ ] [Specific, testable criterion from PRD 5.1]
- [ ] [Additional criterion if needed]

**Test approach:**
[How to verify this works — "Enter play mode and...", "Run the unit test...", etc.]

---
```

Repeat the task block for every task. Continue with Sprint 2, Sprint 3, etc.

After all sprints, include:

```markdown
## Dependency Graph

A text representation of which tasks block which other tasks:

Task 1.1 (GameManager)
  → Task 1.2 (Player prefab)
  → Task 1.3 (Input setup)
    → Task 2.1 (Player movement)
      → Task 2.3 (Player abilities)

Task 1.4 (ScriptableObject definitions)
  → Task 2.2 (Enemy base class)
    → Task 3.1 (Enemy variants)

## Post-MVP Backlog

Tasks that aren't in the sprint plan but are documented in the PRD for later:

| ID | Feature | Description | Depends on |
|----|---------|-------------|------------|
| B1 | [name]  | [what]      | [tasks]    |
```

---

## Task Generation Rules

These guidelines ensure the tasks are actually useful to follow:

**One task = one testable outcome.** If you can't describe how to verify a task is done, it's either too vague or too small. "Set up the project" is too vague. "Add a semicolon" is too small. "Create the player prefab with Rigidbody2D, BoxCollider2D, and a basic PlayerController that reads horizontal input and moves" is about right.

**Order by dependency, not by feature.** Don't group all player tasks together and then all enemy tasks. Instead, build vertically: get one thing working end-to-end, then the next. Sprint 1 should create the infrastructure that everything else needs. Sprint 2 should get the core loop playable (even if ugly). Later sprints add depth and polish.

**Infrastructure first, always.** The first sprint should include:
- Project setup (folder structure, render pipeline, input system package)
- Bootstrap scene with core managers (GameManager, AudioManager if needed)
- Basic scene loading flow
- ScriptableObject definitions (the data containers, not the instances)
These aren't exciting, but every other task depends on them.

**Use the system contracts as API specs.** The PRD's system contracts in 5.2 define the public interfaces. When a task says "implement the HealthSystem", the developer should implement the interface from the contract. This ensures systems connect correctly without rework.

**Map every PRD feature to at least one task.** Cross-reference PRD 5.1 features against your task list. If a feature doesn't appear in any task, either add a task or explicitly move it to the Post-MVP Backlog with a reason.

**File paths matter.** Every task should list the actual files to create or modify, using the project structure from PRD 3.1. This removes the "where does this go?" friction that kills momentum for solo devs.

**Keep the acceptance criteria from the PRD.** The PRD 5.1 features have acceptance criteria checkboxes. Copy them into the relevant tasks — don't rewrite them unless they're genuinely unclear. This keeps the PRD and SDD in sync.

**Estimate honestly for a solo dev.** A solo hobby developer might get 1-2 hours per session. Each sprint should be completable in roughly 3-5 sessions. If a sprint has 15 tasks, it's too big — split it.

**Write implementation notes that prevent common mistakes.** Things like:
- "Use `FixedUpdate` for physics movement, not `Update`"
- "Make this a ScriptableObject reference, not a hardcoded value, so you can tune it in the Inspector"
- "This needs `[RequireComponent(typeof(Rigidbody2D))]`"
- "Don't forget to set the sorting layer or this will render behind the background"

These notes are what make the SDD worth more than just a task list.
