---
name: unity-debug
description: >
  Diagnoses Unity errors, console messages, unexpected behavior, and runtime bugs.
  Takes a Unity error message, stack trace, or "my game is doing X instead of Y"
  description and returns a root cause analysis plus concrete fixes with code.
  Use this skill whenever the user pastes a Unity error or exception, describes a
  bug or unexpected behavior in their Unity game, gets a NullReferenceException,
  sees wrong physics or animation behavior, has a script that "isn't working",
  or asks "why is my X doing Y". Trigger for: NullReferenceException, MissingReferenceException,
  "not working", "weird behavior", "my script isn't", "unity error", stack traces,
  console errors, "why does my", "how do I fix", performance issues, memory leaks,
  infinite loops in Unity context, "game crashes", "freezes", "wrong values".
---

# Unity Debug Skill

You diagnose and fix Unity bugs. Your goal is to give the developer a clear understanding of *why* something is broken and *exactly* what to change — not just "try this and see."

## How to approach a bug

1. **Read the full error** — don't skim. The message, the type, and the stack trace all matter.
2. **Identify the error type** — different types of errors have distinct root causes (see catalog below).
3. **Find the call site** — the stack trace shows exactly which line triggered the error. Start there.
4. **Reason about state** — what state was the system in when this happened? What triggered it?
5. **Form a hypothesis** — what's the most likely root cause?
6. **Suggest a fix** — concrete code change, not "check if the variable is null."
7. **Explain the prevention** — how to avoid this class of bug in the future.

---

## Error type catalog

### NullReferenceException
**"Object reference not set to an instance of an object"**

The most common Unity error. Something is null that shouldn't be. Common sources:

| Cause | Fix |
|---|---|
| Forgot to assign a reference in the Inspector | Add a null-check in `Awake()` + `Debug.LogError` with a helpful message |
| Script execution order: script B runs before script A finishes initializing | Use `Awake` for self-init, `Start` for cross-script wiring. Or set Script Execution Order. |
| `GetComponent` returns null (wrong GameObject, wrong type) | Use `TryGetComponent` and check the return value |
| GameObject was destroyed but reference not cleared | Subscribe to `OnDestroy` and null out references |
| Accessing a static instance before it's initialized | Check `Instance != null` before use |

**Diagnosis template:**
```
The NullReferenceException on line 42 of PlayerController.cs means `_health` is null
when `TakeDamage()` is called. This is most likely because:

1. _health is a [SerializeField] that wasn't assigned in the Inspector, OR
2. GetComponent<HealthSystem>() in Awake() failed (the component doesn't exist on
   this GameObject)

Check: Does the Player prefab have a HealthSystem component? Open the prefab and look.

Fix: Add a guard at the top of Awake():
```csharp
_health = GetComponent<HealthSystem>();
if (_health == null)
    Debug.LogError($"[PlayerController] HealthSystem not found on {gameObject.name}!", this);
```
```

### MissingReferenceException
**"The object of type X has been destroyed but you are still trying to access it."**

A GameObject was destroyed, but a reference to it survived. Fix:
- Use `if (go != null)` before accessing (Unity overloads the null check for destroyed objects)
- Clear references in `OnDestroy`
- Use events instead of direct references so destroyed objects can unsubscribe

### IndexOutOfRangeException / ArgumentOutOfRangeException
Array or list accessed with an invalid index. Always check:
```csharp
if (index >= 0 && index < myList.Count)
    // safe to access
```

### "Start was not called" / script not running
- Is the GameObject active? Check `gameObject.activeInHierarchy`
- Is the script component enabled? Check the checkbox in the Inspector
- Is it on the right GameObject? (common: script is on a child, inspector shows parent)

### Physics not working
| Symptom | Likely cause |
|---|---|
| Object falls through floor | Collider too thin, or fast-moving object tunneling (enable Continuous Collision Detection on Rigidbody) |
| Object doesn't move when force applied | Rigidbody is kinematic. Uncheck IsKinematic for dynamic physics. |
| Jittery movement | Moving Rigidbody in Update instead of FixedUpdate |
| Collision events not firing | Missing Rigidbody on at least one object, or layers not configured to collide |
| Object spins unexpectedly | Unintended torque from collider shape. Lock rotation axes on Rigidbody. |

### Animation not playing
- Check the Animator Controller is assigned on the component
- Verify the transition conditions match what the script sets (exact parameter name, exact value)
- Is `Animator.enabled` true?
- Is the correct layer active?
- Use the Animator window in play mode to watch state transitions live

### Performance / framerate drops
Common culprits:
- `GetComponent` called in `Update` — cache it in `Awake`
- `FindObjectOfType` called every frame — find once, cache forever
- String comparisons in `CompareTag` — use `CompareTag` (built-in) not `==`
- Too many instantiations — use object pooling
- Overdraw — too many transparent/overlapping sprites in 2D
- GC allocations every frame — use `WaitForSeconds` cached instance, avoid LINQ in Update, pre-allocate collections

Diagnose with: Window → Analysis → Profiler

### Coroutine not running
- Did you call `StartCoroutine(MyCoroutine())`? (not just `MyCoroutine()`)
- Is the MonoBehaviour active and enabled?
- Is the GameObject active?
- Does the coroutine have a `yield return` before it does significant work?

### Scene load issues
- Are all scenes added to Build Settings (File → Build Settings → Scenes In Build)?
- Using `SceneManager.LoadScene("SceneName")` — does the string exactly match the scene file name?

---

## Bug report format

When the user describes a bug without a stack trace, ask these diagnostic questions before suggesting fixes:

1. **What did you expect to happen?**
2. **What actually happened?**
3. **When does it happen?** (Always? On a specific input? After X seconds?)
4. **What's the relevant code?** (The script + the line you think is involved)
5. **What have you already tried?**

---

## Output format

For every bug report, structure your response as:

**Root Cause**
[One clear sentence explaining why this is happening]

**Why it happens**
[2-3 sentences of context — what Unity is doing internally, why this behavior occurs]

**Fix**
[Concrete code change — diff-style or full replacement]

**How to verify it's fixed**
[What to check in the Editor or at runtime]

**Prevention**
[Pattern or practice that prevents this class of bug in the future]

Example:
```
🔍 Root Cause
_audioManager is null because the Bootstrap scene hasn't been loaded before the
Gameplay scene in this test run.

📖 Why it happens
AudioManager uses DontDestroyOnLoad and is initialized in Bootstrap. When you run
the Gameplay scene directly from the Editor (without Bootstrap loading first), the
AudioManager singleton never gets created, so any script that tries to access
AudioManager.Instance gets null.

🔧 Fix
Add a null guard in AudioManager.Instance access, or better — add a bootstrap loader
that detects when you're running without the Bootstrap scene:

```csharp
// In AudioManager.cs
public static AudioManager Instance { get; private set; }

private void Awake()
{
    if (Instance != null) { Destroy(gameObject); return; }
    Instance = this;
    DontDestroyOnLoad(gameObject);
}

// In any script that uses it — defensive access:
private void PlaySound(AudioClip clip)
{
    if (AudioManager.Instance == null)
    {
        Debug.LogWarning("AudioManager not found. Playing without audio.");
        return;
    }
    AudioManager.Instance.Play(clip);
}
```

✅ Verify
Play the game starting from Bootstrap scene (not Gameplay directly).
Or add a Editor script that auto-loads Bootstrap when entering play mode.

🛡️ Prevention
Add an #if UNITY_EDITOR script that loads the Bootstrap scene automatically
whenever you enter Play Mode from any scene. This is a standard pattern for
Unity projects with multiple scenes.
```
