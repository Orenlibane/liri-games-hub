# DungeonDelve — Setup Instructions

## You only need to do 3 things:

---

### Step 1 — Create a new Unity project

1. Open **Unity Hub**
2. Click **New Project**
3. Select template: **2D (URP)**
4. Name it: `DungeonDelve`
5. Click **Create Project**

> Wait for Unity to finish setting up (takes ~1 minute).

---

### Step 2 — Copy these files in

1. Open your new project's folder in Finder/Explorer
   - In Unity: right-click the **Assets** folder → **Show in Explorer/Finder**
2. Copy the **`Assets`** folder from this download into your project, merging with the existing one
3. Switch back to Unity — it will recompile (~30 seconds)
4. Watch the bottom progress bar — wait until it stops

> If you see any red compile errors, paste them here and I'll fix them.

---

### Step 3 — Run the setup script

1. In Unity's top menu bar, click **DungeonDelve**
2. Click **🚀 Setup Project**
3. Click **"Yes, set it up!"**

Unity will automatically:
- Create the Bootstrap, MainMenu, and Gameplay scenes
- Set up all GameObjects and UI
- Create Warrior, Rogue, Mage class ScriptableObjects
- Create Goblin and Skeleton enemy ScriptableObjects
- Configure Build Settings

4. When the "Setup Complete! 🎉" popup appears, click **"Let's go!"**
5. Press the **▶ Play button** in Unity

You should see the DungeonDelve main menu appear with a PLAY button!

---

## What's in here (Sprint 1)

| File | What it does |
|------|-------------|
| `Scripts/Core/GameManager.cs` | Persists across scenes, owns run state (floor, class, alive) |
| `Scripts/Core/TurnManager.cs` | Drives turn order: player acts, then enemies in speed order |
| `Scripts/Core/IActable.cs` | Interface every combatant implements |
| `Scripts/Core/ClassDefinition.cs` | ScriptableObject for class data (Warrior/Rogue/Mage) |
| `Scripts/Core/ItemData.cs` | ScriptableObject for items (weapons, potions, armor) |
| `Scripts/Core/EnemyConfig.cs` | ScriptableObject for enemy stats and AI type |
| `Scripts/Core/MetaUnlock.cs` | ScriptableObject for meta-progression unlocks |
| `Scripts/UI/MainMenuController.cs` | Drives the main menu Play/Quit buttons |
| `Editor/DungeonDelveSetup.cs` | The script that built all your scenes automatically |

---

## Sprint 1 is done when:
- [ ] Unity opens without compile errors
- [ ] DungeonDelve → 🚀 Setup Project runs without errors
- [ ] Pressing Play shows the main menu
- [ ] PLAY button loads the (empty) Gameplay scene
- [ ] QUIT button exits play mode

## Stuck? Tell me:
- Any red error messages from the Unity console
- Which step you got to before something went wrong

Sprint 2 is ready whenever you are: dungeon generation, player movement, and the first enemy.
