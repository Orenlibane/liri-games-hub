# DungeonDelve — Sprint 1 Scene Setup Guide

Follow these steps in the Unity Editor after importing all the .cs files.

---

## Before you start: Folder Structure (Task 1.1)

In the **Project window**, create these folders under `Assets/`:

```
Assets/
├── Scripts/
│   ├── Core/       ← paste Core .cs files here
│   └── UI/         ← paste UI .cs files here
├── ScriptableObjects/
│   ├── Classes/
│   ├── Items/
│   ├── Enemies/
│   └── MetaUnlocks/
├── Prefabs/
├── Scenes/
├── Art/
└── Audio/
```

Right-click in the Project window → Create → Folder to make each one.

---

## Scene 1: Bootstrap (Task 1.2)

**File → New Scene → Basic (Built-in) → save as `Assets/Scenes/Bootstrap.unity`**

### Hierarchy setup:
```
Bootstrap (scene root)
└── GameManager          [Empty GameObject]
      └── [Add Component: GameManager]
```

**Step by step:**
1. Delete the default "Main Camera" GameObject (Bootstrap has no visuals)
2. Right-click Hierarchy → Create Empty → rename to `GameManager`
3. With `GameManager` selected: **Add Component** → search `GameManager` → add it
4. In the Inspector, verify these fields:
   - Main Menu Scene Name: `MainMenu`
   - Gameplay Scene Name: `Gameplay`
   - Debug Logging: ✅ (on, for now)

---

## Scene 2: MainMenu (Task 1.5)

**File → New Scene → Basic (Built-in) → save as `Assets/Scenes/MainMenu.unity`**

### Hierarchy setup:
```
MainMenu (scene root)
├── Main Camera          [Camera component, default settings]
└── Canvas               [Canvas + CanvasScaler + GraphicRaycaster]
      ├── Title           [TextMeshProUGUI]
      ├── PlayButton      [Button + TextMeshProUGUI child]
      ├── QuitButton      [Button + TextMeshProUGUI child]
      └── MenuController  [MainMenuController script]
```

**Step by step:**

1. Keep the default `Main Camera`.

2. **Create Canvas:**
   - GameObject → UI → Canvas → rename to `Canvas`
   - Canvas component settings:
     - Render Mode: `Screen Space - Overlay`
   - CanvasScaler settings:
     - UI Scale Mode: `Scale With Screen Size`
     - Reference Resolution: `1920 × 1080`
     - Match: `0.5`

3. **Add Title text:**
   - Right-click Canvas → UI → TextMeshPro - Text → rename `Title`
   - Text: `DungeonDelve`
   - Font Size: `72`
   - Alignment: Center/Center
   - Position: roughly top-center of the screen (Rect Transform: Pos Y = 200)

4. **Add Play button:**
   - Right-click Canvas → UI → Button - TextMeshPro → rename `PlayButton`
   - Button child text: `PLAY`
   - Rect Transform: Width=200, Height=60, Pos Y=0

5. **Add Quit button:**
   - Right-click Canvas → UI → Button - TextMeshPro → rename `QuitButton`
   - Button child text: `QUIT`
   - Rect Transform: Width=200, Height=60, Pos Y=-80

6. **Add MainMenuController:**
   - Right-click Canvas → Create Empty → rename `MenuController`
   - Add Component → `MainMenuController`
   - In the Inspector, assign:
     - Play Button: drag `PlayButton` from hierarchy
     - Quit Button: drag `QuitButton` from hierarchy
     - Default Class: *(leave empty for now — create the Warrior SO first, then come back)*

---

## Create the Warrior ClassDefinition ScriptableObject (Task 1.4)

1. Right-click `Assets/ScriptableObjects/Classes/` → Create → DungeonDelve → Class Definition
2. Name it `Warrior`
3. Fill in the Inspector:
   - Class Name: `Warrior`
   - Class Description: `A sturdy fighter who excels in close combat and outlasting enemies.`
   - Starting Max HP: `30`
   - Starting Attack: `5`
   - Starting Defense: `2`
   - Starting Speed: `5`
   - Starting Max Mana: `0`
   - Ability Name: `Block`
   - Ability Description: `Once per turn, reduce incoming damage by 50%.`
   - Ability Cooldown Turns: `3`

4. Go back to `MainMenu` scene → select `MenuController` → drag `Warrior` SO into **Default Class** field.

---

## Build Settings (Task 1.6)

**File → Build Settings → Add Open Scenes** (repeat for each scene in order):

| Index | Scene |
|-------|-------|
| 0 | Assets/Scenes/Bootstrap.unity |
| 1 | Assets/Scenes/MainMenu.unity |
| 2 | Assets/Scenes/Gameplay.unity *(create an empty scene and save it — just needs to exist)* |

---

## Sprint 1 Acceptance Criteria Checklist

- [ ] Project opens without console errors
- [ ] All folders exist under Assets/
- [ ] Bootstrap scene is scene index 0
- [ ] Pressing Play in Bootstrap → MainMenu loads automatically
- [ ] Clicking PLAY in MainMenu → Gameplay scene loads (empty scene is fine for now)
- [ ] Clicking QUIT exits play mode (or closes the build)
- [ ] GameManager.Instance is accessible from the console: type `DungeonDelve.Core.GameManager.Instance` in the console (or add a debug log)
- [ ] Warrior ClassDefinition SO is in Assets/ScriptableObjects/Classes/
- [ ] Right-clicking in Assets/ScriptableObjects shows: DungeonDelve → Class Definition, Item Data, Enemy Config, Meta Unlock

---

## What's next: Sprint 2

Sprint 2 builds the dungeon floor:
- Procedural room generation on a tile grid
- Player prefab with EntityStats and movement
- First enemy (Goblin) that chases and attacks
- When all 3 are in the scene, TurnManager becomes testable end-to-end

Use the `unity-script` skill to generate `DungeonGenerator.cs`, `EntityStats.cs`, and `PlayerController.cs` when you're ready.
