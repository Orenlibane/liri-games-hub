---
name: unity-ui
description: >
  Generates Unity UI code — both the layout definition and the C# controller script —
  for any in-game screen: HUD, main menu, pause menu, inventory, dialog boxes, health
  bars, skill trees, or any other UI element. Supports both UI Toolkit (UIDocument +
  UXML + USS) and the legacy Canvas/UGUI system. Output is always complete and
  copy-paste ready. Use this skill whenever the user wants to build a game UI, create
  a menu, add a HUD element, display player stats on screen, make a popup or dialog,
  or wire up any visual overlay in Unity — even if they don't say "UI Toolkit" or
  "Canvas" explicitly. Trigger for: "add a health bar", "create the main menu",
  "make an inventory screen", "show player stats", "add a pause menu", "dialog box",
  "game over screen", "HUD", "heads-up display".
---

# Unity UI Generation Skill

You generate complete Unity UI implementations: the layout structure and the C# code that drives it. Before writing anything, decide whether to use **UI Toolkit** or **Canvas/UGUI** — this affects everything.

## Choosing the UI system

**Use UI Toolkit (UIDocument + UXML + USS) when:**
- Unity 2021.2 or newer
- PC/Console target platforms
- Complex UI with many dynamic elements
- User wants web-style layout control (flexbox-like)

**Use Canvas/UGUI when:**
- Unity 2020 or older
- Mobile target (better touch support)
- Simple UI (a few elements)
- Project already uses Canvas for other UI
- User is more familiar with the Inspector-based workflow

If unsure, **ask the user** before generating. Getting this wrong means throwing away the output.

---

## UI Toolkit output

For UI Toolkit, always generate three files:

### 1. UXML (layout)
```xml
<ui:UXML xmlns:ui="UnityEngine.UIElements" xmlns:uie="UnityEditor.UIElements">
    <ui:VisualElement name="hud-root" class="hud-container">
        <ui:VisualElement name="health-section" class="stat-section">
            <ui:Label name="health-label" class="stat-label" text="HP" />
            <ui:VisualElement name="health-bar-bg" class="bar-bg">
                <ui:VisualElement name="health-bar-fill" class="bar-fill health-fill" />
            </ui:VisualElement>
            <ui:Label name="health-value" class="stat-value" text="100/100" />
        </ui:VisualElement>
    </ui:VisualElement>
</ui:UXML>
```

Key rules for UXML:
- Every element that needs to be accessed from C# gets a unique `name`
- Visual structure uses `class` for styling, `name` for code references
- Avoid hardcoded inline styles — put all styling in USS

### 2. USS (styling)
```css
.hud-container {
    position: absolute;
    top: 20px;
    left: 20px;
    width: 250px;
    background-color: rgba(0, 0, 0, 0.5);
    border-radius: 8px;
    padding: 10px;
}

.bar-bg {
    width: 100%;
    height: 12px;
    background-color: rgba(255, 255, 255, 0.2);
    border-radius: 6px;
    overflow: hidden;
}

.bar-fill {
    height: 100%;
    border-radius: 6px;
    transition-property: width;
    transition-duration: 0.2s;
}

.health-fill {
    background-color: rgb(76, 200, 100);
    width: 100%;
}
```

### 3. C# UIDocument controller
```csharp
using UnityEngine;
using UnityEngine.UIElements;

namespace [ProjectName].UI
{
    /// <summary>Controls the in-game HUD display.</summary>
    [RequireComponent(typeof(UIDocument))]
    public class HUDController : MonoBehaviour
    {
        // ── Inspector Fields ─────────────────────────────────────────────
        [Header("References")]
        [SerializeField] private PlayerHealth _playerHealth;

        // ── Private State ─────────────────────────────────────────────
        private VisualElement _healthBarFill;
        private Label _healthValue;

        // ── Unity Callbacks ──────────────────────────────────────────────
        private void Awake()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            _healthBarFill = root.Q<VisualElement>("health-bar-fill");
            _healthValue   = root.Q<Label>("health-value");
        }

        private void OnEnable()
        {
            if (_playerHealth != null)
                _playerHealth.OnHealthChanged += UpdateHealthDisplay;
        }

        private void OnDisable()
        {
            if (_playerHealth != null)
                _playerHealth.OnHealthChanged -= UpdateHealthDisplay;
        }

        // ── Private Methods ──────────────────────────────────────────────
        private void UpdateHealthDisplay(float current, float max)
        {
            float pct = Mathf.Clamp01(current / max);
            _healthBarFill.style.width = Length.Percent(pct * 100f);
            _healthValue.text = $"{Mathf.CeilToInt(current)}/{Mathf.CeilToInt(max)}";
        }
    }
}
```

---

## Canvas/UGUI output

For Canvas/UGUI, generate a scene setup specification + C# controller:

### Scene hierarchy specification
```
Canvas (Screen Space - Overlay)
  └── HUD_Root [CanvasGroup]
        ├── HealthSection
        │     ├── HealthLabel [TextMeshProUGUI] "HP"
        │     ├── HealthBarBG [Image] (color: dark gray, sprite: rounded rect)
        │     │     └── HealthBarFill [Image] (image type: Filled, fill method: Horizontal)
        │     └── HealthValueText [TextMeshProUGUI] "100/100"
        └── GoldSection
              ├── GoldIcon [Image]
              └── GoldText [TextMeshProUGUI] "0"
```

### Canvas/UGUI C# controller
```csharp
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace [ProjectName].UI
{
    public class HUDController : MonoBehaviour
    {
        [Header("Health Bar")]
        [SerializeField] private Image _healthBarFill;
        [SerializeField] private TextMeshProUGUI _healthValueText;

        [Header("References")]
        [SerializeField] private PlayerHealth _playerHealth;

        private void OnEnable()
        {
            if (_playerHealth != null)
                _playerHealth.OnHealthChanged += UpdateHealthDisplay;
        }

        private void OnDisable()
        {
            if (_playerHealth != null)
                _playerHealth.OnHealthChanged -= UpdateHealthDisplay;
        }

        private void UpdateHealthDisplay(float current, float max)
        {
            _healthBarFill.fillAmount = Mathf.Clamp01(current / max);
            _healthValueText.text = $"{Mathf.CeilToInt(current)}/{Mathf.CeilToInt(max)}";
        }
    }
}
```

---

## Common UI patterns

### Animated transitions
For menus that slide/fade in:
```csharp
// UI Toolkit approach
private IEnumerator FadeIn(VisualElement panel, float duration)
{
    panel.style.opacity = 0;
    panel.style.display = DisplayStyle.Flex;
    float elapsed = 0f;
    while (elapsed < duration)
    {
        elapsed += Time.unscaledDeltaTime;
        panel.style.opacity = Mathf.Clamp01(elapsed / duration);
        yield return null;
    }
}
```

### Button wiring
```csharp
// UI Toolkit
root.Q<Button>("start-button").clicked += OnStartClicked;

// Canvas/UGUI (assign in Inspector or):
_startButton.onClick.AddListener(OnStartClicked);
```

### Modal dialogs
Modals should block input to elements behind them. Use a full-screen transparent overlay VisualElement (or Canvas panel) with a high sort order, and disable interaction on the panels beneath.

### Inventory grids
Use `ListView` (UI Toolkit) or a `GridLayoutGroup` with a pool of item cell prefabs (UGUI). Never instantiate one prefab per inventory slot without pooling — it's too slow for large inventories.

---

## Output format

After generating UI code, always tell the developer:

1. **Files to create** — UXML, USS, and .cs file paths
2. **Scene setup** — where to place the UIDocument or Canvas in the hierarchy
3. **Inspector assignments** — what references to drag in
4. **Events to connect** — which game system events this UI listens to
5. **Art assets needed** — sprites, fonts, icons required
6. **Next step** — what UI screen to build next (based on SDD)

Example:
```
📁 Files:
   Assets/UI/HUD.uxml
   Assets/UI/HUD.uss
   Assets/Scripts/UI/HUDController.cs

🎬 Scene setup: Add UIDocument component to a GameObject in your Gameplay scene.
   Assign HUD.uxml to the Source Asset field.

🎛️ Inspector: Drag the Player GameObject (which has PlayerHealth) into _playerHealth.

🔗 Listens to: PlayerHealth.OnHealthChanged event — no extra wiring needed.

🎨 Art needed: No sprites required (pure CSS styling). Add a font asset if you
   want custom typography.

➡️ Next: Build the PauseMenu UI (SDD Task 3.2)
```
