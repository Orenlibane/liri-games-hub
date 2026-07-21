---
name: unity-test
description: >
  Generates Unity Test Framework tests (NUnit) for Unity game systems and scripts —
  both EditMode tests (fast, no scene needed) and PlayMode tests (full runtime
  simulation). Takes a MonoBehaviour, ScriptableObject, system description, or SDD
  task and produces a complete test file that can be dropped into a Tests folder
  and run immediately from the Unity Test Runner. Use this skill when the user
  wants to verify a game system works correctly, write tests before or after
  implementing a feature, debug unexpected behavior via test, or asks "how do I
  test X in Unity", "write tests for my Y system", "my Z isn't working, can you
  write a test to check it", or "generate unit tests for...". Always trigger
  for any Unity testing request.
---

# Unity Test Generation Skill

You generate complete Unity Test Framework test files using NUnit. Tests you produce should be runnable immediately from Window → General → Test Runner — no additional setup beyond placing the file.

## EditMode vs PlayMode — choose first

**EditMode tests** (faster, preferred when possible):
- Test pure C# logic that doesn't need a running scene
- ScriptableObject data validation
- Utility/helper class logic
- State machine transitions (if logic is pure C#)
- Run instantly without entering play mode

**PlayMode tests** (when you need the full runtime):
- MonoBehaviour lifecycle (Awake, Start, Update, coroutines)
- Physics interactions
- Input handling
- Animation system
- Anything that requires `yield return` timing

When generating tests, **default to EditMode** and only reach for PlayMode when the thing being tested genuinely requires it.

## Project setup reminder

Before tests can run, the project needs a test assembly. Remind the user if this hasn't been done:
```
Assets/Tests/EditMode/
  └── MyGame.EditMode.Tests.asmdef   ← Create via right-click → Create → Testing → Assembly Definition
Assets/Tests/PlayMode/
  └── MyGame.PlayMode.Tests.asmdef   ← Same, but check "Include platforms: Editor + PlayMode"
```
Both assembly definitions need to reference `nunit.framework.dll` (auto-included) and the main game assembly.

---

## EditMode test template

```csharp
using NUnit.Framework;
using UnityEngine;

namespace MyGame.Tests.EditMode
{
    /// <summary>Tests for the HealthSystem's damage and healing logic.</summary>
    public class HealthSystemTests
    {
        // ── Setup / Teardown ────────────────────────────────────────────
        private HealthSystem _sut;  // system under test

        [SetUp]
        public void SetUp()
        {
            // Create a fresh instance before each test
            var go = new GameObject("TestPlayer");
            _sut = go.AddComponent<HealthSystem>();
            // Initialize with known state
            _sut.Initialize(maxHealth: 100f);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_sut.gameObject);
        }

        // ── Happy path tests ─────────────────────────────────────────────
        [Test]
        public void TakeDamage_ReducesCurrentHealth()
        {
            _sut.TakeDamage(30f);
            Assert.AreEqual(70f, _sut.CurrentHealth, delta: 0.001f);
        }

        [Test]
        public void Heal_IncreasesCurrentHealth()
        {
            _sut.TakeDamage(50f);
            _sut.Heal(20f);
            Assert.AreEqual(70f, _sut.CurrentHealth, delta: 0.001f);
        }

        // ── Boundary / edge case tests ────────────────────────────────────
        [Test]
        public void TakeDamage_DoesNotGoBelowZero()
        {
            _sut.TakeDamage(999f);
            Assert.AreEqual(0f, _sut.CurrentHealth, delta: 0.001f);
        }

        [Test]
        public void Heal_DoesNotExceedMaxHealth()
        {
            _sut.Heal(999f);
            Assert.AreEqual(100f, _sut.CurrentHealth, delta: 0.001f);
        }

        // ── Event tests ──────────────────────────────────────────────────
        [Test]
        public void TakeDamage_FiresOnHealthChangedEvent()
        {
            float receivedHealth = -1f;
            _sut.OnHealthChanged += (current, max) => receivedHealth = current;

            _sut.TakeDamage(25f);

            Assert.AreEqual(75f, receivedHealth, delta: 0.001f);
        }

        [Test]
        public void TakeDamage_FiresOnDeathWhenHealthReachesZero()
        {
            bool deathFired = false;
            _sut.OnDeath += () => deathFired = true;

            _sut.TakeDamage(100f);

            Assert.IsTrue(deathFired, "OnDeath should fire when health reaches 0");
        }

        // ── Parameterized tests ──────────────────────────────────────────
        [TestCase(0f,   100f)]
        [TestCase(50f,   50f)]
        [TestCase(100f,   0f)]
        [TestCase(150f,   0f)]  // over-damage clamped to 0
        public void TakeDamage_ParameterizedBoundaries(float damage, float expectedHealth)
        {
            _sut.TakeDamage(damage);
            Assert.AreEqual(expectedHealth, _sut.CurrentHealth, delta: 0.001f);
        }
    }
}
```

## PlayMode test template

```csharp
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace MyGame.Tests.PlayMode
{
    /// <summary>PlayMode tests for PlayerController physics movement.</summary>
    public class PlayerControllerTests
    {
        private GameObject _playerGO;
        private PlayerController _player;
        private Rigidbody2D _rb;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            _playerGO = new GameObject("TestPlayer");
            _playerGO.AddComponent<Rigidbody2D>();
            _playerGO.AddComponent<BoxCollider2D>();
            _player = _playerGO.AddComponent<PlayerController>();
            _rb = _playerGO.GetComponent<Rigidbody2D>();

            yield return null;  // wait one frame for Awake/Start
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Object.Destroy(_playerGO);
            yield return null;
        }

        [UnityTest]
        public IEnumerator MoveRight_IncreasesXPosition()
        {
            float startX = _playerGO.transform.position.x;

            _player.SetMoveInput(new Vector2(1f, 0f));
            yield return new WaitForSeconds(0.2f);  // let physics run

            Assert.Greater(_playerGO.transform.position.x, startX,
                "Player should move right when given rightward input");
        }

        [UnityTest]
        public IEnumerator Jump_IncreasesYVelocity()
        {
            // Place on ground first (simplified — use a proper ground layer in real tests)
            _playerGO.transform.position = Vector3.zero;
            yield return null;

            _player.TryJump();
            yield return null;  // one physics frame

            Assert.Greater(_rb.velocity.y, 0f, "Jump should give upward velocity");
        }
    }
}
```

---

## Testing ScriptableObjects

SOs can be tested in EditMode without any scene — just instantiate them directly:

```csharp
[Test]
public void EnemyConfig_MeleeDamageCalculation()
{
    var config = ScriptableObject.CreateInstance<EnemyConfig>();
    config.SetTestValues(baseDamage: 10f, damageMultiplier: 1.5f);

    float result = config.CalculateDamage();

    Assert.AreEqual(15f, result, delta: 0.001f);
    Object.DestroyImmediate(config);
}
```

---

## Testing patterns to know

### Testing events with callbacks
```csharp
bool eventFired = false;
system.OnSomething += () => eventFired = true;
system.DoThing();
Assert.IsTrue(eventFired);
```

### Testing coroutines (PlayMode)
```csharp
[UnityTest]
public IEnumerator Coroutine_CompletesAfterDelay()
{
    _sut.StartCooldown(1f);
    Assert.IsTrue(_sut.IsOnCooldown);
    yield return new WaitForSeconds(1.1f);
    Assert.IsFalse(_sut.IsOnCooldown);
}
```

### Using LogAssert for expected errors
```csharp
[Test]
public void Initialize_WithNullConfig_LogsError()
{
    LogAssert.Expect(LogType.Error, "Config cannot be null");
    _sut.Initialize(config: null);
}
```

### Mocking dependencies
Unity doesn't have a built-in mock framework. Use one of:
- **Interface + test double**: Extract an interface from the dependency, create a fake implementation in the test
- **NSubstitute** or **Moq**: Import via UPM if the project uses them

---

## Output format

After generating tests, tell the developer:

1. **File path** — `Assets/Tests/EditMode/HealthSystemTests.cs`
2. **Assembly reference** — which `.asmdef` this file needs to be inside
3. **How to run** — Window → General → Test Runner → Run All
4. **What's covered** — list the behaviors being verified
5. **What's NOT covered** — honest gaps, things that need PlayMode or are hard to test
6. **Next tests to write** — based on SDD task list

Example:
```
📁 Save to: Assets/Tests/EditMode/HealthSystemTests.cs
📦 Place inside: MyGame.EditMode.Tests assembly

▶️ Run: Window → General → Test Runner → EditMode tab → Run All

✅ Covers:
   - Damage reduces health correctly
   - Healing is clamped to MaxHealth
   - Damage is clamped to 0
   - OnHealthChanged fires with correct value
   - OnDeath fires when health reaches 0

⚠️ Not covered (add PlayMode tests for these):
   - Death animation trigger timing
   - Invincibility frames after taking damage

➡️ Next: Write tests for EnemyAI state transitions (SDD Task 2.4)
```
