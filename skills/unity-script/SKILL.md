---
name: unity-script
description: >
  Generates complete, production-ready Unity C# scripts from SDD tasks, feature
  descriptions, or natural language requests. Produces MonoBehaviours, ScriptableObjects,
  interfaces, managers, utility classes, and editor scripts that follow the architecture
  defined in the project's PRD and SDD. Output is always a full script file ready to
  paste into Unity — never pseudocode or partial snippets. Use this skill whenever the
  user wants to write, generate, or implement Unity C# code for a specific system or
  feature, even if they phrase it as "write me a script for X", "how do I code Y in
  Unity", "implement the Z system", or "generate the MonoBehaviour for...".
  Always trigger for any Unity C# coding request.
---

# Unity C# Script Generation Skill

You are generating complete, copy-paste-ready Unity C# scripts. Every script you produce should be something the developer can drop straight into their Assets/Scripts folder and have it work — no placeholders, no "fill in your logic here", no partial implementations.

## Before you write anything

Read the PRD and SDD if they exist in the workspace. Specifically, look for:
- **PRD Section 3.3** — MonoBehaviour architecture (class names, responsibilities, dependencies)
- **PRD Section 3.4** — ScriptableObject design
- **PRD Section 3.1** — Project folder structure (where the file should live)
- **SDD System contracts** — The public interfaces this script must implement

If no PRD/SDD exists, ask the user for:
1. The project's render pipeline (Built-in, URP, HDRP)
2. Whether they're using the new Input System or old Input.GetKey
3. 2D or 3D physics
4. Unity version (this affects available APIs)

## Script quality standards

Every script must:

**Have exactly one clear responsibility.** If a script is doing two unrelated things, split it. A `PlayerController` handles movement. A `PlayerHealth` handles damage and death. A `PlayerAnimator` drives the Animator. They communicate via events or direct references, not by doing each other's jobs.

**Use `[SerializeField]` for Inspector exposure, never `public` fields.** Public fields bypass encapsulation. Use properties for read access from other scripts:
```csharp
[SerializeField] private float _moveSpeed = 5f;
public float MoveSpeed => _moveSpeed;
```

**Declare Unity callbacks only when actually used.** Don't include empty `Update()`, `Start()`, or `Awake()` methods. Every Unity callback in the script should do real work.

**Prefer `FixedUpdate` for physics, `Update` for input and visuals.** Never move a Rigidbody in `Update`. Never read time-sensitive input in `FixedUpdate`.

**Cache component references in `Awake`, not in `Update`.** GetComponent is slow. Always cache:
```csharp
private Rigidbody2D _rb;
private void Awake() => _rb = GetComponent<Rigidbody2D>();
```

**Use `[RequireComponent]` to declare hard dependencies:**
```csharp
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerController : MonoBehaviour
```

**Use events for loose coupling between systems.** When one system needs to react to something in another, use C# events or UnityEvents — not direct method calls if the relationship isn't guaranteed:
```csharp
public event Action<int> OnHealthChanged;
public event Action OnDeath;
```

**Null-check external references** when not using RequireComponent:
```csharp
if (_audioManager == null)
{
    Debug.LogWarning($"[{nameof(PlayerController)}] AudioManager not assigned!");
    return;
}
```

**Include XML doc comments on public API:**
```csharp
/// <summary>Apply damage to the player. Triggers OnDeath if health reaches zero.</summary>
/// <param name="amount">Damage amount (positive values reduce health).</param>
public void TakeDamage(float amount) { ... }
```

## Script structure template

Follow this ordering inside every class:

```csharp
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// Any other Unity package namespaces
// Any project namespaces

namespace [ProjectName].[Domain]  // e.g., MyGame.Player
{
    /// <summary>One-sentence description of what this class does.</summary>
    [RequireComponent(typeof(...))]  // if applicable
    public class ClassName : MonoBehaviour  // or ScriptableObject, or plain class
    {
        // ── Inspector Fields ─────────────────────────────────────────────
        [Header("Movement")]
        [SerializeField] private float _moveSpeed = 5f;

        // ── Events ───────────────────────────────────────────────────────
        public event Action<float> OnHealthChanged;

        // ── Private State ─────────────────────────────────────────────
        private Rigidbody2D _rb;
        private float _currentHealth;

        // ── Properties ───────────────────────────────────────────────────
        public float CurrentHealth => _currentHealth;

        // ── Unity Callbacks ──────────────────────────────────────────────
        private void Awake() { ... }
        private void Start() { ... }
        private void Update() { ... }
        private void FixedUpdate() { ... }
        private void OnDestroy() { ... }

        // ── Public API ───────────────────────────────────────────────────
        public void TakeDamage(float amount) { ... }
        public void Heal(float amount) { ... }

        // ── Private Methods ──────────────────────────────────────────────
        private void HandleDeath() { ... }
        private void UpdateHealthBar() { ... }
    }
}
```

## ScriptableObject guidelines

When generating a ScriptableObject:
- Always include the `[CreateAssetMenu]` attribute with a sensible menu path
- Keep SO data pure — no Unity callbacks, no references to scene objects
- Use `[Tooltip]` on every field so designers understand it in the Inspector

```csharp
[CreateAssetMenu(fileName = "New EnemyConfig", menuName = "MyGame/Enemy Config")]
public class EnemyConfig : ScriptableObject
{
    [Header("Stats")]
    [Tooltip("Base health at difficulty level 1")]
    [SerializeField] private float _baseHealth = 100f;

    [Tooltip("Movement speed in units/second")]
    [SerializeField] private float _moveSpeed = 3f;

    public float BaseHealth => _baseHealth;
    public float MoveSpeed => _moveSpeed;
}
```

## Manager / Singleton pattern

When generating a manager that should persist and be globally accessible:

```csharp
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
```

Only use singletons for truly global, persistent systems (GameManager, AudioManager, SaveManager). Everything else should use dependency injection or ScriptableObject events.

## Output format

After saving the script file(s), your response to the user MUST end with a developer note — this is as important as the code itself, because it tells the developer exactly what to do next in the Unity Editor. Without it, they're left guessing where to put the file and how to wire it up.

The note goes OUTSIDE and AFTER any code blocks, as plain text in your response. Use exactly this format:

```
📁 Save to: Assets/Scripts/Player/PlayerController.cs
🔧 Requires: Rigidbody2D, BoxCollider2D on the same GameObject
🎛️ Inspector: Assign the PlayerConfig ScriptableObject in the field
🔗 Referenced by: PlayerAnimator (drag this GameObject into its _controller field)
➡️ Next: Implement PlayerAnimator.cs (SDD Task 1.3)
```

Every field is required. If a field doesn't apply (e.g. no required components), write "None" rather than omitting the line. If multiple files were generated, include one note block per file. This note is the bridge between the code you wrote and the developer actually using it — never skip it.
