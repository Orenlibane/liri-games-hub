---
name: unity-prd
description: >
  Creates a comprehensive Product Requirements Document (PRD) for Unity game projects.
  The PRD includes game vision, core mechanics, full Unity technical architecture
  (MonoBehaviours, ScriptableObjects, scene hierarchy, input systems, shader notes,
  performance budgets), milestones, and a structured SDD handoff section with feature
  breakdowns and system contracts. Output is a single markdown file designed for solo/hobby
  developers. Use this skill whenever the user wants to plan a game, write a game design
  document, create a PRD for a Unity project, or spec out a game idea before building it.
  Also trigger when the user mentions "game plan", "game spec", "game requirements",
  "game architecture", or wants to turn a game concept into a structured development plan,
  even if they don't say "PRD" explicitly.
---

# Unity Game PRD Skill

You are creating a Product Requirements Document for a Unity game project. The PRD serves two purposes: it's a clear reference for the developer during build, and it's the structured input for the SDD skill that will break it into actionable development tasks.

## Choosing a mode: Full vs Quick

Before generating, decide which mode fits the user's situation:

**Full PRD** (default): All 5 sections including complete Unity architecture and SDD handoff with C# contracts. Best when the user wants to hand off to the SDD skill and start building. Produces a 60–100KB document. Takes longer but saves time in every sprint that follows.

**Quick PRD**: Sections 1–2 only (Overview + Core Mechanics) plus a lightweight Section 5 (Feature list, no system contracts). Best when the user is still exploring an idea and not ready to build. Produces a ~10KB document in a fraction of the time. Does not feed the SDD skill well — tell the user to run Full PRD when they're ready to build.

**When to use Quick mode:**
- User says "I just want to plan out my idea" or "quick overview" or "explore this concept"
- User hasn't committed to Unity yet
- User wants to iterate on the game concept before locking in architecture
- You can also ask if unsure: "Do you want the full technical PRD (ready to build from) or a quick concept doc?"

When in Quick mode, generate only Sections 1 and 2 from the template below, plus a compact Section 5.1 with feature names and one-line descriptions (no system contracts). Clearly label the document `<!-- PRD Mode: Quick -->` at the top and include a note: *"When ready to build, run the Full PRD to generate Unity architecture and SDD handoff sections."*

## How to use this skill

1. Read the user's game concept carefully
2. Decide Full vs Quick mode (see above)
3. Ask clarifying questions if critical details are missing (genre, core mechanic, target platform)
4. Generate the PRD following the template below
5. Save as a markdown file in the workspace

## PRD Template

The PRD always follows this structure. Every section is required — if the user hasn't specified something, make reasonable assumptions for a solo indie Unity project and note them as assumptions.

```markdown
# [Game Title] — Product Requirements Document

## 1. Game Overview & Vision

### 1.1 Elevator Pitch
A single paragraph (2-3 sentences) capturing what makes this game compelling. Think "what would you say to get someone excited about this in 30 seconds?"

### 1.2 Genre & References
- Primary genre and any sub-genres
- 2-3 reference games and what specifically to draw from each (not just "like X" — say what aspect)

### 1.3 Target Platform
- Primary platform (PC, mobile, WebGL, etc.)
- Minimum spec / target device if relevant
- Target resolution and aspect ratio

### 1.4 Art Style & Tone
- Visual direction (2D pixel art, 3D low-poly, stylized, realistic, etc.)
- Mood and atmosphere
- Color palette direction
- UI style notes

### 1.5 Target Audience
- Who is this for?
- Session length expectations
- Difficulty approach

---

## 2. Core Mechanics & Systems

### 2.1 Core Gameplay Loop
Describe the moment-to-moment experience. What does the player DO most of the time? Map it as a loop:
Player action → Feedback → Reward/consequence → Next decision

### 2.2 Player Mechanics
For each player ability or action:
- **Name**: What the player does
- **Input**: How the player triggers it
- **Behavior**: What happens in-game
- **Feedback**: Visual/audio/haptic response
- **Edge cases**: What happens at boundaries (e.g., wall collision during dash)

### 2.3 Game Systems
For each major system (progression, inventory, combat, crafting, etc.):
- **Purpose**: Why this system exists in the game
- **Rules**: How it works mechanically
- **Player-facing data**: What the player sees
- **Internal state**: What the system tracks behind the scenes
- **Interactions**: How this system connects to other systems

### 2.4 AI & NPC Behavior
If applicable:
- AI decision-making approach (state machine, behavior tree, utility AI)
- NPC types and their behavioral patterns
- Difficulty scaling approach

### 2.5 Level / World Design
- Level structure (linear, hub-and-spoke, open world, procedural)
- Progression through levels/areas
- Key landmarks or set pieces

---

## 3. Unity Technical Architecture

This section describes how the game maps to Unity's systems. The goal is to provide enough detail that a developer (you, future you, or a collaborator) can start building without re-making these decisions.

### 3.1 Project Structure
```
Assets/
├── Scripts/
│   ├── Core/           # Singletons, managers, game state
│   ├── Player/         # Player controller, abilities, stats
│   ├── Enemies/        # AI, spawning, behavior
│   ├── Systems/        # Inventory, progression, save/load
│   ├── UI/             # HUD, menus, dialogs
│   └── Utilities/      # Helpers, extensions, constants
├── ScriptableObjects/  # Data definitions
├── Prefabs/            # Reusable game objects
├── Scenes/             # All scenes
├── Art/                # Sprites, models, materials
├── Audio/              # SFX, music
└── Resources/          # Runtime-loaded assets (use sparingly)
```
Adjust this structure to fit the specific game.

### 3.2 Scene Hierarchy
List each scene and its purpose:
- **Bootstrap**: Persistent managers, don't-destroy-on-load setup
- **MainMenu**: Title screen, settings, save slot selection
- **Gameplay_[Level]**: Actual gameplay scenes
- **Loading**: Transition screen if needed

Describe the scene flow (which scenes load into which, additive loading if used).

### 3.3 MonoBehaviour Architecture
For each key MonoBehaviour:
- **Class name**: `PlayerController`, `EnemyAI`, etc.
- **Responsibility**: Single clear purpose
- **Key serialized fields**: What's configured in the Inspector
- **Unity callbacks used**: `Awake`, `Update`, `FixedUpdate`, `OnCollisionEnter`, etc.
- **Public API**: Methods other scripts call
- **Dependencies**: What it requires (Rigidbody, Collider, etc.)

Prefer composition over deep inheritance. Use `[RequireComponent]` where appropriate.

### 3.4 ScriptableObject Data Design
For each ScriptableObject type:
- **Name**: `WeaponData`, `EnemyConfig`, `LevelDefinition`, etc.
- **Fields**: What data it holds
- **Purpose**: Why this is a SO and not hardcoded (balancing, variety, designer-friendly tuning)
- **How many instances**: Roughly how many of these will exist

ScriptableObjects are the backbone of data-driven design. Use them for anything that should be tunable without touching code: enemy stats, item definitions, level parameters, audio settings, difficulty curves.

### 3.5 Input System
- Which input system: old `Input.GetKey` vs. new Input System package
- Input action map layout
- Control schemes (keyboard+mouse, gamepad, touch)
- Rebinding support if needed

### 3.6 Physics & Collision
- 2D vs 3D physics
- Layer setup and collision matrix
- Raycasting usage
- Rigidbody configuration approach (dynamic, kinematic, static)

### 3.7 Rendering & Visual Effects
- Render pipeline: Built-in, URP, or HDRP (and why)
- Camera setup (Cinemachine, custom follow, fixed)
- Lighting approach
- Particle systems needed
- Shader notes (custom shaders, shader graph nodes)
- Post-processing effects

### 3.8 Audio Architecture
- Audio system approach (AudioManager singleton, FMOD, Wwise, or simple AudioSource)
- Music: layers, transitions, adaptive
- SFX: pooling, spatial audio, mixer groups
- UI sounds

### 3.9 Save / Load System
- What gets saved (player progress, settings, world state)
- Storage approach (PlayerPrefs, JSON file, binary serialization)
- Save slot support
- Cloud save if applicable

### 3.10 Performance Budget
- Target framerate
- Target draw calls / batching strategy
- Memory considerations
- Object pooling needs (bullets, particles, enemies)
- Level-of-detail or culling strategy if applicable

---

## 4. Milestones & Scope

### 4.1 MVP Definition
The absolute minimum playable version. List 5-10 features that make the game "work" at a basic level. Everything else is nice-to-have.

### 4.2 Development Phases
Break into phases:
- **Prototype** (get the core loop feeling good)
- **Alpha** (all core systems in, placeholder art OK)
- **Beta** (content complete, polish and bug fixing)
- **Release** (final polish, optimization, platform-specific)

For each phase, list what's in and what's explicitly out.

### 4.3 Risk Assessment
Identify the 3-5 biggest risks:
- **Risk**: What could go wrong
- **Impact**: How bad is it
- **Mitigation**: What to do about it

Common risks for solo devs: scope creep, art pipeline bottleneck, "fun factor" uncertainty, platform-specific issues.

---

## 5. SDD Handoff

This section is structured specifically so that the SDD (Software Design Document) skill can consume it and generate actionable development tasks. It summarizes everything above into two formats: feature breakdowns and system contracts.

### 5.1 Feature Breakdown
For each discrete feature in the game:

#### Feature: [Feature Name]
- **Description**: What this feature does from the player's perspective
- **Inputs**: What data/events trigger this feature
- **Outputs**: What this feature produces (state changes, visuals, audio)
- **Dependencies**: Other features or systems this requires
- **Acceptance criteria**: How to know this feature is "done"
  - [ ] Criterion 1
  - [ ] Criterion 2
- **Priority**: MVP / Post-MVP / Nice-to-have
- **Estimated complexity**: Low / Medium / High

### 5.2 System Contracts
For each system (a system is a group of related features that share state):

#### System: [System Name]
**Public Interface:**
```csharp
// The methods and events other systems can call/subscribe to
public void MethodName(ParamType param);
public event Action<EventDataType> OnSomethingHappened;
public PropertyType PropertyName { get; }
```

**Depends on:**
- [Other System] — for what purpose

**Depended on by:**
- [Other System] — for what purpose

**Internal state:**
- List of state variables this system owns

**Invariants:**
- Rules that must always be true (e.g., "Health never exceeds MaxHealth")
```

---

## Writing guidelines

When filling in this template, keep these principles in mind:

**Be specific, not aspirational.** A PRD for a solo dev should describe what you're actually going to build, not a dream feature list. If a section doesn't apply to this game, write "N/A — [brief reason]" rather than filling it with vague plans.

**Assumptions are fine, but label them.** When you're making a design choice the user didn't specify, prefix it with ⚠️ **Assumption:** so they can easily spot and override these.

**The SDD handoff section is the most important part.** Everything else is context and rationale. Section 5 is what actually drives development. Make sure every feature in sections 2 and 3 appears as a feature breakdown in 5.1, and every significant MonoBehaviour/system in section 3 has a system contract in 5.2.

**Write C# interface signatures, not pseudocode.** The system contracts in 5.2 should use valid C# syntax. They don't need to be final implementations, but they should be close enough that a developer can start coding from them.

**Scale to the game.** A simple mobile puzzle game doesn't need a 3-page physics section. A complex action RPG does. Use judgment about depth per section based on the game's actual complexity.
