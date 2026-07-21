---
name: unity-scene
description: >
  Generates a complete Unity scene hierarchy specification — what GameObjects to create,
  how to structure them, which components to attach, how to configure each component,
  and step-by-step instructions for building the scene in the Unity Editor. Output is
  a structured scene blueprint that a developer can follow from a blank scene to a
  fully wired scene ready for scripting. Use this skill when the user wants to set up
  a Unity scene, build a level, create a gameplay scene, set up the Bootstrap or main
  menu scene, wire up cameras, lights, and managers in a scene, or asks "how do I
  structure my X scene", "what GameObjects do I need for Y", "set up my dungeon scene",
  "scene hierarchy for my game". Trigger for: "set up scene", "scene structure",
  "scene hierarchy", "build the level", "create a gameplay scene", "scene setup".
---

# Unity Scene Setup Skill

You generate a complete, step-by-step specification for building a Unity scene. The output tells the developer exactly what to create, where, and how to configure it — so they can go from File → New Scene to a fully structured, ready-to-script scene.

## Before generating

Read the PRD if it exists, specifically:
- **Section 3.1** — Project structure (naming conventions)
- **Section 3.2** — Scene hierarchy overview (list of scenes and their purpose)
- **Section 3.5** — Input system
- **Section 3.6** — Physics (2D vs 3D)
- **Section 3.7** — Render pipeline and camera setup

Ask the user which scene they want to set up (Bootstrap, MainMenu, Gameplay, etc.) if not specified.

---

## Scene specification format

### Section 1: Scene overview
State the scene's purpose and what it should contain at a high level.

### Section 2: Hierarchy tree
Use an indented tree to show the complete GameObject structure:

```
[Scene: Gameplay_DungeonFloor1]
│
├── _Managers                          [Empty GameObject, top-level]
│     ├── GameManager                  [GameManager.cs]
│     ├── AudioManager                 [AudioManager.cs, AudioSource (x2)]
│     ├── UIManager                    [UIManager.cs]
│     └── InputManager                 [PlayerInput, InputActionAsset: GameInputs]
│
├── _Camera                            [Empty GameObject]
│     ├── MainCamera                   [Camera, AudioListener, CinemachineBrain]
│     └── VirtualCamera_Follow         [CinemachineVirtualCamera]
│
├── _Lighting                          [Empty GameObject]
│     ├── GlobalLight                  [Light2D, type: Global, intensity: 0.1]
│     └── PlayerLight                  [Light2D, type: Point, follows player via script]
│
├── _World                             [Empty GameObject]
│     ├── DungeonTilemap               [Tilemap, TilemapRenderer, TilemapCollider2D, CompositeCollider2D]
│     ├── WallTilemap                  [Tilemap, TilemapRenderer, TilemapCollider2D]
│     └── PropContainer                [Empty — instantiated props go here at runtime]
│
├── _Entities                          [Empty GameObject]
│     ├── Player                       [Prefab instance → Player.prefab]
│     └── EnemyContainer               [Empty — spawned enemies go here at runtime]
│
└── _UI                                [Canvas, Screen Space Overlay, CanvasScaler]
      ├── HUD                          [UIDocument, Source: HUD.uxml]
      └── PauseMenu                    [UIDocument, Source: PauseMenu.uxml, disabled by default]
```

### Section 3: Component configuration
For every non-trivial component, specify the settings:

```
## Component Configuration

### MainCamera
- Clear Flags: Solid Color, Background: #000000
- Projection: Orthographic
- Size: 5 (adjust to match pixel art scale)
- Near: -1, Far: 100
- Culling Mask: Everything except "UI" (UI handled by Canvas)

### CinemachineVirtualCamera (VirtualCamera_Follow)
- Follow: [drag Player transform here]
- Look At: none
- Body: Framing Transposer
  - Dead Zone Width: 0.1, Dead Zone Height: 0.1
  - Soft Zone Width: 0.8, Soft Zone Height: 0.8
  - Lookahead Time: 0 (disable for top-down dungeon feel)
- Aim: Do Nothing

### DungeonTilemap
- TilemapRenderer: Sorting Layer "World", Order in Layer 0
- TilemapCollider2D: Used by Composite = true
- Rigidbody2D (on same GO): Body Type = Static, Used by Composite = true
- CompositeCollider2D: Geometry Type = Polygons

### GlobalLight (Light2D)
- Light Type: Global
- Intensity: 0.1  ← dark dungeon base ambient
- Color: #1A1A3A  ← cool dark blue tint

### Canvas
- Render Mode: Screen Space - Overlay
- UI Scale Mode: Scale with Screen Size
- Reference Resolution: 1920 × 1080
- Match: 0.5 (width/height blend)
```

### Section 4: Step-by-step build instructions
Write numbered instructions a developer can follow in the Unity Editor:

```
## Build Instructions

1. File → New Scene → Basic (Built-in)
   Delete the default "Main Camera" and "Directional Light" GameObjects.

2. Create the manager hierarchy:
   - Right-click Hierarchy → Create Empty, name it "_Managers"
   - Right-click _Managers → Create Empty three times:
     GameManager, AudioManager, UIManager
   - Add components to each (see Component Configuration above)

3. Set up the camera:
   - Create Empty → name "_Camera"
   - GameObject → Camera → rename "MainCamera", child of _Camera
   - Window → Package Manager → search "Cinemachine" → Install
   - GameObject → Cinemachine → Cinemachine Virtual Camera → name "VirtualCamera_Follow"
   - Set MainCamera config (see above)

4. Set up 2D Lighting (URP only):
   - GameObject → Light → 2D → Global Light 2D → name "GlobalLight", child of _Lighting
   - Add Point Light 2D for player light later (after Player prefab is created)

5. Create the Tilemap world:
   - GameObject → 2D Object → Tilemap → Rectangular → rename parent "DungeonTilemap"
   - Add TilemapCollider2D, check "Used by Composite"
   - Add Rigidbody2D: Body Type = Static, check "Used by Composite"
   - Add CompositeCollider2D: Geometry Type = Polygons
   - Repeat for "WallTilemap" (separate layer for walls with different collision)

6. Drag in the Player prefab:
   - Drag Assets/Prefabs/Player.prefab into _Entities in the hierarchy
   - (If Player prefab doesn't exist yet, create an empty placeholder: GameObject → 2D Object → Sprites → Square, rename "Player", add placeholder scripts)

7. Set up the UI:
   - GameObject → UI → Canvas → rename to "_UI"
   - Configure Canvas (see Component Configuration)
   - Add UIDocument child for HUD (assign HUD.uxml)
   - Add UIDocument child for PauseMenu (assign PauseMenu.uxml, disable the component)

8. Wire up Cinemachine:
   - Select VirtualCamera_Follow
   - Drag the Player transform into the Follow field

9. Save the scene:
   - File → Save As → Assets/Scenes/Gameplay_DungeonFloor1.unity
   - Add to Build Settings (File → Build Settings → Add Open Scenes)
```

### Section 5: Prefab references
List all prefabs this scene expects to instantiate (either at edit time or runtime):

```
## Expected Prefabs
- Assets/Prefabs/Player.prefab — drag into scene at edit time
- Assets/Prefabs/Enemies/Goblin.prefab — spawned at runtime by EnemySpawner
- Assets/Prefabs/Enemies/SkeletonWarrior.prefab — spawned at runtime
- Assets/Prefabs/Items/HealthPotion.prefab — spawned at runtime by loot system
- Assets/Prefabs/VFX/DamageNumber.prefab — spawned at runtime by damage system
```

### Section 6: Sorting layers
If not already set up in the project:

```
## Required Sorting Layers (Edit → Project Settings → Tags and Layers)
Background   (order 0)
World        (order 1)
Items        (order 2)
Entities     (order 3)
Player       (order 4)
Effects      (order 5)
UI           (order 10)
```

---

## Bootstrap scene specification

The Bootstrap scene is special — it loads first, sets up persistent managers, then loads the actual first scene. Always generate this if it doesn't exist yet:

```
[Scene: Bootstrap]
│
└── Bootstrap                [Bootstrap.cs — loads MainMenu after setup]
      ├── GameManager        [GameManager.cs, DontDestroyOnLoad]
      ├── AudioManager       [AudioManager.cs + AudioMixer, DontDestroyOnLoad]
      ├── SaveManager        [SaveManager.cs, DontDestroyOnLoad]
      └── SceneLoader        [SceneLoader.cs, DontDestroyOnLoad]
```

Bootstrap.cs loads next scene after a single frame:
```csharp
private IEnumerator Start()
{
    // Give all Awake() calls a frame to complete
    yield return null;
    SceneManager.LoadScene("MainMenu");
}
```

---

## Output format

After generating the scene spec, tell the developer:

1. **Scene file path** — where to save
2. **Dependencies** — packages that must be installed (Cinemachine, etc.)
3. **Prefabs needed** — what prefabs to create before this scene is functional
4. **Estimated setup time** — realistic estimate for a solo dev
5. **Next scene to build** — based on SDD

Example:
```
📁 Save to: Assets/Scenes/Gameplay_DungeonFloor1.unity

📦 Required packages: Cinemachine (2.9+), 2D Lights (included in URP)

🧩 Prefabs needed before this scene is fully functional:
   - Player.prefab (SDD Task 1.2)
   - At least one enemy prefab (SDD Task 2.1)

⏱️ Estimated setup time: ~45 minutes for a solo dev following these steps

➡️ Next scene: MainMenu (SDD Task 3.1)
```
