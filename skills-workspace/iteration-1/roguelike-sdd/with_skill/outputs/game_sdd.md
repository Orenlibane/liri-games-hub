# DungeonDelve — Software Design Document

## Overview
- **Source PRD**: DungeonDelve PRD (Roguelike Dungeon Crawler)
- **Total tasks**: 32
- **Estimated sprints**: 6
- **MVP tasks**: 20 (Sprints 1–4)

---

## Sprint Plan

Tasks are organized into sprints. Each sprint results in something testable — you should be able to run the game and see/verify something new after each sprint.

### Sprint 1: Project Skeleton & Core Infrastructure
**Goal**: Bootstrap scene with GameManager, TurnManager, and basic scene loading working. No gameplay yet, but infrastructure is ready.
**Estimated effort**: 6 tasks, roughly 4–6 hours

---

#### Task 1.1: Project Setup & Folder Structure
- **Feature**: Infrastructure
- **System**: None
- **Priority**: MVP
- **Depends on**: None

**What to build:**
Create the project directory structure following the PRD architecture. Set up folders for Scripts (Core, Player, Enemies, Systems, UI, Utilities), ScriptableObjects, Prefabs, Scenes, Art, and Audio. Configure project settings: set resolution to 1920x1080, configure the 2D renderer, and import the input system package (or use legacy Input if preferred for simplicity in MVP).

**Files to create/modify:**
- Project root: create folders `Assets/Scripts/Core`, `Assets/Scripts/Player`, `Assets/Scripts/Enemies`, `Assets/Scripts/Systems`, `Assets/Scripts/UI`, `Assets/Scripts/Utilities`, `Assets/ScriptableObjects`, `Assets/Prefabs`, `Assets/Scenes`, `Assets/Art`, `Assets/Audio`
- `ProjectSettings/ProjectSettings.asset` — set resolution to 1920x1080, aspect ratio 16:9

**Implementation notes:**
Use a clean folder structure from the start to avoid refactoring later. Create empty .gitkeep files in empty folders if using version control. For MVP, the legacy Input system is sufficient; defer migrating to the new Input System if it adds complexity.

**Acceptance criteria:**
- [ ] Folder structure matches PRD 3.1 layout
- [ ] Project loads without errors
- [ ] Console shows no missing folder warnings

**Test approach:**
Open the project in Unity. Verify all folders exist in the Assets folder. Check ProjectSettings that resolution is 1920x1080.

---

#### Task 1.2: Create Bootstrap Scene & GameManager
- **Feature**: Infrastructure
- **System**: GameManager
- **Priority**: MVP
- **Depends on**: Task 1.1

**What to build:**
Create the Bootstrap scene and the GameManager MonoBehaviour. GameManager will persist across scenes using DontDestroyOnLoad, track the current run state (floor number, player alive status), and manage scene transitions. Implement the public properties and event hooks defined in the System Contracts (PRD 5.2). Bootstrap scene should load MainMenu after initialization.

**Files to create/modify:**
- `Assets/Scenes/Bootstrap.unity` — empty scene with a single GameObject "GameManager" containing the GameManager script
- `Assets/Scripts/Core/GameManager.cs` — MonoBehaviour implementing run state tracking, scene loading, and DontDestroyOnLoad setup

**Implementation notes:**
Use `DontDestroyOnLoad(gameObject)` in Awake to ensure GameManager persists across scenes. Keep GameManager initialization simple in MVP — focus only on scene transitions and run state. Defer complex meta-progression state until Sprint 5. Use a simple enum for GameState (e.g., MainMenu, InDungeon, Dead). Make GameManager a singleton (optional but helpful: use a static `Instance` property).

**Acceptance criteria:**
- [ ] Bootstrap scene created and set as first scene in build settings
- [ ] GameManager persists across scene loads
- [ ] GameManager successfully loads MainMenu scene on startup
- [ ] Console shows no errors related to GameManager

**Test approach:**
Set Bootstrap as the startup scene. Enter play mode. Verify the console does not show errors. After MainMenu is loaded (Task 1.4), verify that transitions work.

---

#### Task 1.3: Create TurnManager System
- **Feature**: Turn Resolution
- **System**: TurnManager
- **Priority**: MVP
- **Depends on**: Task 1.2

**What to build:**
Implement the TurnManager MonoBehaviour as the central turn resolution system. It maintains a queue of entities that implement an `IActable` interface, tracks whose turn it is, and resolves all actions each turn cycle. Implement all public methods and events from the System Contracts (PRD 5.2): `RegisterEntity`, `UnregisterEntity`, `SubmitPlayerAction`, `OnTurnResolutionStart`, `OnTurnResolutionEnd`, and the `IsPlayerTurn` property. TurnManager will live in the Gameplay scene (created in Sprint 2).

**Files to create/modify:**
- `Assets/Scripts/Core/TurnManager.cs` — MonoBehaviour implementing the TurnManager contract
- `Assets/Scripts/Core/IActable.cs` — interface for entities that can act in turns

**Implementation notes:**
Use a Queue<IActable> to track action order. On each turn cycle, dequeue the current actor, request their action (if player, wait for input; if enemy, request from EnemyAI), resolve the action, and re-queue them at the back. Use Speed stat to determine initial action order (faster actors go first each turn). Emit `OnTurnResolutionStart` before processing actions and `OnTurnResolutionEnd` after all actions resolve. Use `IsPlayerTurn` to know when to request player input vs. run enemy AI. For MVP, keep this synchronous (no coroutines for now).

**Acceptance criteria:**
- [ ] TurnManager registers and unregisters entities correctly
- [ ] Player action submitted via SubmitPlayerAction triggers turn resolution
- [ ] All entities act exactly once per turn cycle
- [ ] Faster entities (higher Speed stat) act before slower ones
- [ ] OnTurnResolutionStart and OnTurnResolutionEnd fire correctly

**Test approach:**
This task is tested in Sprint 2 when entities are added. For now, verify the TurnManager script compiles and has no syntax errors. Create a simple unit test (optional for MVP): instantiate TurnManager, mock an IActable entity, register it, verify it's in the queue, and call SubmitPlayerAction to ensure events fire.

---

#### Task 1.4: Create ScriptableObject Definitions
- **Feature**: Infrastructure
- **System**: None
- **Priority**: MVP
- **Depends on**: Task 1.1

**What to build:**
Create four ScriptableObject definitions as classes (not instances yet): `ClassDefinition`, `ItemData`, `EnemyConfig`, and `MetaUnlock`. These are the data containers referenced in PRD 3.4. Each should include a `[CreateAssetMenu]` attribute so they can be created in the Inspector. Include all fields mentioned in the PRD (e.g., ClassDefinition has starting stats, starting items, class ability; ItemData has name, stats, type, sprite).

**Files to create/modify:**
- `Assets/Scripts/Core/ClassDefinition.cs` — ScriptableObject for character class data
- `Assets/Scripts/Core/ItemData.cs` — ScriptableObject for item definitions
- `Assets/Scripts/Core/EnemyConfig.cs` — ScriptableObject for enemy definitions
- `Assets/Scripts/Core/MetaUnlock.cs` — ScriptableObject for meta-progression unlocks

**Implementation notes:**
Each ScriptableObject should be lightweight data containers with no logic. Use `[System.Serializable]` for nested data structures (e.g., a struct for "item slot" containing item reference and quantity). Include a `GetHashCode()` or unique ID field for easy reference. Defer creating actual instances (e.g., Warrior class, Iron Sword item) until later sprints when they're needed.

**Acceptance criteria:**
- [ ] All four ScriptableObject definitions compile without errors
- [ ] Each definition appears in the "Create Asset" menu with the correct category
- [ ] All fields from PRD 3.4 are represented

**Test approach:**
In the Project window, right-click and verify each ScriptableObject type appears in the Create Asset menu. Create one instance of each (e.g., "Warrior" ClassDefinition) and verify all fields can be edited in the Inspector without errors.

---

#### Task 1.5: Create MainMenu Scene & Navigation
- **Feature**: Infrastructure
- **System**: None
- **Priority**: MVP
- **Depends on**: Task 1.2

**What to build:**
Create the MainMenu scene with basic UI. Include a Title text, a "Play" button that starts a new run, a "Meta-Progression Display" placeholder (for now, just an empty panel or text saying "Meta-Progression Coming Soon"), and a "Quit" button. The Play button should save the selected class choice (hardcoded to Warrior for MVP) and load the Gameplay scene.

**Files to create/modify:**
- `Assets/Scenes/MainMenu.unity` — UI scene with Canvas, Title, Play, Quit buttons, and placeholder for meta-progression
- `Assets/Scripts/UI/MainMenuController.cs` — MonoBehaviour to handle Play/Quit button clicks and scene transitions

**Implementation notes:**
Use the Canvas from Unity's built-in UI system. For MVP, hardcode the class selection to Warrior and skip the class-select UI (that's a Sprint 3 task). Keep the meta-progression display as a non-functional placeholder — it will be filled in during Sprint 5. Use `SceneManager.LoadScene()` to load the Gameplay scene. Communicate the selected class to GameManager via a public static property or a ScriptableObject singleton (avoid singletons if possible, but for a small game they're acceptable).

**Acceptance criteria:**
- [ ] MainMenu scene loads from Bootstrap
- [ ] Play button loads Gameplay scene
- [ ] Quit button closes the application
- [ ] No console errors

**Test approach:**
Set Gameplay scene in build settings (create an empty scene if it doesn't exist yet). Enter play mode in Bootstrap. After MainMenu loads, click Play. Verify Gameplay scene loads (it will be empty for now). Click Quit and verify the application closes (or logs the quit in the console if in the Editor).

---

#### Task 1.6: Set Build Settings & Scene Order
- **Feature**: Infrastructure
- **System**: None
- **Priority**: MVP
- **Depends on**: Tasks 1.2, 1.5

**What to build:**
Configure the build settings to include Bootstrap (scene 0), MainMenu (scene 1), and Gameplay (scene 2) in the correct order. Verify the startup scene is Bootstrap. This ensures the game initializes correctly when built.

**Files to create/modify:**
- `ProjectSettings/EditorBuildSettings.asset` — add the three scenes in order

**Implementation notes:**
Open File > Build Settings. Add the three scenes in this order: Bootstrap, MainMenu, Gameplay. Ensure Bootstrap is at index 0 (the startup scene). This is not a task you "code" but rather a configuration task that must be done before testing the full flow.

**Acceptance criteria:**
- [ ] Bootstrap is scene 0
- [ ] MainMenu is scene 1
- [ ] Gameplay is scene 2
- [ ] Build settings show no missing scene errors

**Test approach:**
Open Build Settings and visually verify the three scenes are listed in order. Enter play mode in Bootstrap and verify the scene progression works (Bootstrap → MainMenu, MainMenu → Gameplay).

---

### Sprint 2: Playable Movement & Turn System
**Goal**: Player can move one tile per turn on a procedurally generated grid, and enemies (placeholder) exist. No combat yet, but the core loop is playable.
**Estimated effort**: 6 tasks, roughly 5–7 hours

---

#### Task 2.1: Create Gameplay Scene & DungeonGenerator
- **Feature**: Grid Movement
- **System**: DungeonGenerator
- **Priority**: MVP
- **Depends on**: Task 1.1

**What to build:**
Create the Gameplay scene with a DungeonGenerator MonoBehaviour. The generator creates a simple dungeon floor as a grid of walkable and non-walkable tiles using procedural generation (for MVP, a simple algorithm: start with all tiles walkable, carve out rooms and corridors using binary space partition or random walk). Implement the methods from the System Contracts (PRD 5.2): `GenerateFloor`, `IsTileWalkable`, `GetPlayerSpawnPosition`, `GetEnemySpawnPositions`, and `OnFloorGenerated` event. Store the generated floor as a 2D array or dictionary for O(1) walkability checks.

**Files to create/modify:**
- `Assets/Scenes/Gameplay.unity` — scene with a canvas for HUD and a "DungeonRoot" GameObject containing the generator
- `Assets/Scripts/Systems/DungeonGenerator.cs` — MonoBehaviour implementing floor generation
- `Assets/Scripts/Core/DungeonConfig.cs` — ScriptableObject with generation parameters (dungeon width, height, room count, etc.)

**Implementation notes:**
For MVP, use a simple algorithm: fill a grid with walls, then carve out rooms randomly and connect them with corridors. Don't try to make beautiful procedural dungeons yet — focus on generating something playable quickly. Store walkability in a 2D boolean array `walkable[x, y]`. Spawn the player in the center or top-left of the first room, and spawn enemies in other rooms (exact positions determined in Task 2.5). Use a fixed grid size (e.g., 50x50 tiles) for the first MVP version; make it configurable later.

**Acceptance criteria:**
- [ ] GenerateFloor creates a playable dungeon layout
- [ ] IsTileWalkable returns correct values
- [ ] GetPlayerSpawnPosition and GetEnemySpawnPositions return valid walkable tiles
- [ ] OnFloorGenerated fires after generation completes

**Test approach:**
Enter play mode in the Gameplay scene. Add a temporary debug visualizer (a simple script that draws a grid using Gizmos or UI Image placeholders) to verify the dungeon layout. Call `GenerateFloor(1)` and inspect the generated layout visually.

---

#### Task 2.2: Create Player Prefab & PlayerController Input
- **Feature**: Grid Movement
- **System**: PlayerController
- **Priority**: MVP
- **Depends on**: Tasks 1.1, 1.3

**What to build:**
Create the Player prefab with a sprite (use a 32x32 placeholder pixel art of a character), a BoxCollider2D (for detecting overlap with enemies and items), and a PlayerController MonoBehaviour. PlayerController reads WASD or arrow key input, converts it to a grid movement action, and submits it to the TurnManager (do not move the player directly — queue the action instead). Implement an `IActable` interface on PlayerController so it can be registered with the TurnManager.

**Files to create/modify:**
- `Assets/Prefabs/Player.prefab` — player GameObject with sprite, collider, and PlayerController
- `Assets/Scripts/Player/PlayerController.cs` — MonoBehaviour reading input and submitting actions to TurnManager
- `Assets/Art/player_32x32.png` — simple placeholder pixel art sprite (or use a colored square for MVP)

**Implementation notes:**
Attach the PlayerController to the player prefab root. Read input in `Update()` (W/A/S/D or arrow keys). Convert input to a direction vector (0, 1), then create an action object (e.g., `MoveAction(direction)`) and submit it to TurnManager via `SubmitPlayerAction()`. Do not move the player immediately — wait for `OnTurnResolutionEnd` event from TurnManager to update the player's position. Store the player's grid position as a `Vector2Int`. Implement `IActable.OnActable()` method (or similar) that receives the submitted action and returns the result.

**Acceptance criteria:**
- [ ] Player sprite appears in the Gameplay scene
- [ ] WASD/arrow keys queue movement actions
- [ ] PlayerController implements IActable correctly

**Test approach:**
Instantiate the Player prefab in the Gameplay scene at the dungeon spawn position. Enter play mode and press WASD keys. Verify input is detected in the console or via a debug log. (Movement won't happen yet until Task 2.3 because the action resolution isn't implemented.)

---

#### Task 2.3: Implement Movement Action & Resolution
- **Feature**: Grid Movement
- **System**: TurnManager, DungeonGenerator
- **Priority**: MVP
- **Depends on**: Tasks 2.1, 2.2, 1.3

**What to build:**
Create the `MoveAction` class (implements `IAction` interface) and implement the action resolution logic in TurnManager. When a MoveAction is resolved, check if the target tile is walkable using `DungeonGenerator.IsTileWalkable()`. If walkable, update the player's position. If blocked by a wall, the action fails but the turn is still consumed. Implement the `IAction` interface with properties for direction and actor.

**Files to create/modify:**
- `Assets/Scripts/Core/IAction.cs` — interface for actions
- `Assets/Scripts/Core/MoveAction.cs` — concrete action class
- `Assets/Scripts/Systems/TurnManager.cs` — add action resolution logic (modify from Task 1.3)
- `Assets/Scripts/Player/PlayerController.cs` — update to create MoveAction objects (modify from Task 2.2)

**Implementation notes:**
The TurnManager's turn resolution loop should look like: 1) Dequeue next actor, 2) If player, wait for action (already submitted by PlayerController), 3) If enemy, request action from EnemyAI, 4) Execute action (call `IAction.Execute(actor)`), 5) Emit `OnTurnResolutionEnd`, 6) Repeat. MoveAction.Execute() should: a) Calculate target position, b) Check walkability, c) If walkable, update actor position and return success, d) If blocked, return failure. The player's position should be updated in the action's Execute method, not in PlayerController.

**Acceptance criteria:**
- [ ] Player moves one tile per WASD input to adjacent walkable tiles
- [ ] Player cannot move into walls (blocked movement consumes turn)
- [ ] Moving triggers turn resolution (TurnManager events fire)

**Test approach:**
Enter play mode with Player in Gameplay scene. Press a direction key. Verify the player moves one tile in that direction. Try moving into a wall. Verify the player stays in place but the turn is consumed (next turn, enemies move if they exist).

---

#### Task 2.4: Create EntityStats System
- **Feature**: Combat (infrastructure)
- **System**: EntityStats
- **Priority**: MVP
- **Depends on**: Task 1.1

**What to build:**
Create the EntityStats MonoBehaviour as a data container and logic handler for HP, Attack, Defense, and Speed. Implement the public interface from the System Contracts (PRD 5.2): `TakeDamage()`, `Heal()`, `OnHealthChanged` event, `OnDeath` event, `CurrentHP`, `MaxHP`, `Attack`, `Defense`. EntityStats should not know about combat logic—it only tracks stats and fires events when they change.

**Files to create/modify:**
- `Assets/Scripts/Core/EntityStats.cs` — MonoBehaviour implementing HP/stat tracking

**Implementation notes:**
Store HP as a private field with public getter. On `TakeDamage()`, reduce CurrentHP by the amount (clamped to >= 0), emit `OnHealthChanged(currentHP, maxHP)`, and if CurrentHP reaches 0, emit `OnDeath` and disable the entity's GameObject. On `Heal()`, increase CurrentHP (clamped to <= MaxHP) and emit `OnHealthChanged`. All stat fields (Attack, Defense, Speed) should be public properties or Inspector-editable fields (use `[SerializeField]` for encapsulation). Keep this class simple—no dependencies on combat or enemy AI.

**Acceptance criteria:**
- [ ] TakeDamage reduces HP correctly
- [ ] OnHealthChanged fires with correct values
- [ ] OnDeath fires when HP <= 0
- [ ] Entity is disabled when dead

**Test approach:**
Attach EntityStats to the Player prefab and set starting values in the Inspector. In play mode, call `GetComponent<EntityStats>().TakeDamage(10)` in the console or via a test script. Verify the console logs show `OnHealthChanged` firing and HP decreasing. Call `TakeDamage` until HP reaches 0 and verify `OnDeath` fires.

---

#### Task 2.5: Create Enemy Prefab & EnemyAI Base Class
- **Feature**: Turn Resolution (enemies)
- **System**: EnemyAI
- **Priority**: MVP
- **Depends on**: Tasks 2.1, 2.4, 1.3

**What to build:**
Create an Enemy prefab with a sprite, BoxCollider2D, EntityStats, and an EnemyAI MonoBehaviour. For MVP, implement a simple state machine: Idle (wander), Chase (if player in vision), Attack (if adjacent). The enemy should implement the `IActable` interface and register with TurnManager on spawn. Implement `OnActable()` to queue the appropriate action based on AI state.

**Files to create/modify:**
- `Assets/Prefabs/Enemy.prefab` — enemy GameObject with sprite, collider, EntityStats, EnemyAI
- `Assets/Scripts/Enemies/EnemyAI.cs` — MonoBehaviour implementing simple state machine
- `Assets/Scripts/Enemies/EnemyActionFactory.cs` (optional) — helper to create enemy actions

**Implementation notes:**
For MVP, use a simple state enum (Idle, Chase, Attack). In `OnActable()`, check if the player is within vision range (e.g., 5 tiles). If so, move towards the player (using A* or simple pathfinding, or just move towards them in a straight line for MVP simplicity). If adjacent, attack. If player is not in sight, wander (move to a random adjacent walkable tile). Use the DungeonGenerator to check walkability. Don't implement sophisticated pathfinding yet—simple "move towards player" is sufficient for MVP.

**Acceptance criteria:**
- [ ] Enemy spawns in Gameplay scene
- [ ] Enemy registers with TurnManager
- [ ] Enemy acts once per turn cycle
- [ ] Enemy moves towards player if player is in vision range
- [ ] Enemy attacks if adjacent to player

**Test approach:**
Spawn the enemy near the player in the Gameplay scene. Enter play mode and press a direction to move the player towards the enemy. Verify the enemy moves towards the player. Move next to the enemy and verify it queues an attack action (attack won't deal damage yet, but the action should be queued).

---

#### Task 2.6: Wire Up DungeonGenerator Spawning
- **Feature**: Grid Movement, Turn Resolution
- **System**: DungeonGenerator, TurnManager
- **Priority**: MVP
- **Depends on**: Tasks 2.1, 2.2, 2.5, 1.3

**What to build:**
Create a DungeonSpawner MonoBehaviour (or add logic to DungeonGenerator) that listens to `OnFloorGenerated` event, then instantiates the Player prefab at `GetPlayerSpawnPosition()` and Enemy prefabs at `GetEnemySpawnPositions()`. Also instantiate and configure the TurnManager in the Gameplay scene. Register all spawned entities with TurnManager so they can act in turns.

**Files to create/modify:**
- `Assets/Scripts/Systems/DungeonSpawner.cs` — MonoBehaviour handling spawn logic
- `Assets/Scenes/Gameplay.unity` — add TurnManager GameObject and wire up DungeonSpawner

**Implementation notes:**
Subscribe to `DungeonGenerator.OnFloorGenerated` in `Start()`. When the event fires, instantiate the player and enemies at their spawn positions. Immediately register them with TurnManager. Use `Instantiate()` to spawn the prefabs. Store references to spawned enemies in a list so you can clean them up when transitioning floors. For MVP, spawn 3-5 basic enemies per floor.

**Acceptance criteria:**
- [ ] Player and enemies spawn at correct positions when floor generates
- [ ] All spawned entities register with TurnManager
- [ ] Turn resolution includes both player and enemies

**Test approach:**
Enter play mode in Gameplay. Verify the player and enemies appear at their spawn positions. Move the player with WASD and verify enemies also move each turn. Verify the turn order includes both player and enemies.

---

### Sprint 3: Combat System & First Win Condition
**Goal**: Combat works (damage, death), enemies drop loot, player can defeat enemies and clear floors, win condition is functional (reach floor 10).
**Estimated effort**: 6 tasks, roughly 5–7 hours

---

#### Task 3.1: Implement Attack Action & Combat Resolution
- **Feature**: Combat
- **System**: TurnManager, EntityStats
- **Priority**: MVP
- **Depends on**: Tasks 2.3, 2.4, 2.5

**What to build:**
Create the `AttackAction` class and implement combat resolution in the action's Execute method. When an entity attacks an adjacent target, calculate damage as `max(1, Attacker.Attack - Defender.Defense)`. Apply damage to the target's EntityStats via `TakeDamage()`. If the target dies, the kill is logged (and later used for loot drops). Both player and enemies should be able to initiate attacks.

**Files to create/modify:**
- `Assets/Scripts/Core/AttackAction.cs` — attack action class implementing IAction
- `Assets/Scripts/Systems/TurnManager.cs` — update to resolve AttackAction (or delegate to action's Execute method)

**Implementation notes:**
AttackAction should store the attacker and target references, and implement `Execute()` to perform damage calculation. The damage formula is: `damage = max(1, attacker.Attack - target.Defense)`. This ensures damage is never 0, even against high-defense targets. Place the damage calculation in EntityStats or as a static utility function. Have EnemyAI queue AttackAction when the player is adjacent instead of MoveAction. Update PlayerController to allow the player to queue AttackAction by pressing a key (e.g., spacebar to attack in a direction) or by moving into an enemy (if movement action targets an enemy, convert to attack).

**Acceptance criteria:**
- [ ] Damage is calculated as max(1, Attack - Defense)
- [ ] Target EntityStats.OnDeath fires when HP <= 0
- [ ] Both player and enemies can attack

**Test approach:**
Enter play mode. Move the player next to an enemy. Press the attack key (or auto-attack when adjacent if you implement it). Verify damage is logged and the enemy's health decreases. Keep attacking until the enemy dies and verify OnDeath fires (the enemy disappears).

---

#### Task 3.2: Create ItemPickup Prefab & InventorySystem
- **Feature**: Item Collection (basic)
- **System**: InventorySystem
- **Priority**: MVP
- **Depends on**: Task 1.1

**What to build:**
Create the ItemPickup prefab (a visual representation of an item on the floor) and the InventorySystem MonoBehaviour. The ItemPickup has a reference to ItemData (ScriptableObject) and is destroyed when touched by the player. The InventorySystem tracks the player's items in a list or dictionary, supports add/remove/equip methods, and exposes the current equipment (weapon, armor, rings). For MVP, keep it simple: inventory is a list of items, equipping means swapping the current equipment slot.

**Files to create/modify:**
- `Assets/Prefabs/ItemPickup.prefab` — item on the floor with sprite and ItemPickup script
- `Assets/Scripts/Systems/InventorySystem.cs` — MonoBehaviour managing items and equipment
- `Assets/Scripts/Core/ItemPickup.cs` — script attached to ItemPickup prefab

**Implementation notes:**
ItemPickup should have a trigger collider that detects the player and calls `OnTriggerEnter()` to add the item to the player's inventory. The InventorySystem should be attached to the player or GameManager. Use a struct to represent an equipped slot: `EquippedSlot { ItemData item; int quantity; }`. When an item is picked up, check if it's consumable (stackable) or equipment (unique). For equipment, add it to inventory; for consumables, increase quantity. Defer UI display of inventory until Sprint 4. For MVP, just log pickups to the console.

**Acceptance criteria:**
- [ ] ItemPickup detects player collision and triggers pickup
- [ ] Item is added to InventorySystem
- [ ] Equipment can be equipped/unequipped
- [ ] Consumables stack

**Test approach:**
Place an ItemPickup prefab with a test ItemData in the Gameplay scene. Enter play mode and move the player over the item. Verify the item is removed from the world and the console logs the pickup. In the Inspector, verify the InventorySystem shows the item in the player's inventory.

---

#### Task 3.3: Create Enemy Loot Table & Drop Logic
- **Feature**: Item Collection
- **System**: EnemyAI, InventorySystem
- **Priority**: MVP
- **Depends on**: Tasks 2.4, 3.2, 3.1

**What to build:**
Add a loot table to EnemyConfig (ScriptableObject) that defines what items an enemy drops and with what probability. When an enemy dies (OnDeath event fires), spawn ItemPickup prefabs at the enemy's position for each loot drop. Create a LootTable helper class to manage probability-based loot selection.

**Files to create/modify:**
- `Assets/Scripts/Core/EnemyConfig.cs` — add LootTableEntry list (modify from Task 1.4)
- `Assets/Scripts/Systems/LootTableHelper.cs` — utility to select loot based on probabilities
- `Assets/Scripts/Enemies/EnemyAI.cs` — subscribe to OnDeath and spawn loot (modify from Task 2.5)

**Implementation notes:**
A LootTableEntry should have: ItemData, drop probability (0–1), and quantity. When the enemy dies, iterate the loot table, roll a random number for each entry, and if it passes the probability check, instantiate an ItemPickup. Use `UnityEngine.Random.value` to roll probabilities. Spawn the loot at or near the enemy's position (within 1 tile). For MVP, keep loot tables simple: maybe 50% gold, 25% weapon, 25% consumable per enemy.

**Acceptance criteria:**
- [ ] Enemy death spawns items based on loot table
- [ ] Loot is placed at enemy's position
- [ ] Probabilities are respected (statistical verification)

**Test approach:**
Configure an enemy with a 100% drop rate for a test item. Kill the enemy and verify the item appears on the ground. Pick it up and verify it's in the inventory. Repeat a few times to test probability-based drops.

---

#### Task 3.4: Implement Floor Progression & Clear Condition
- **Feature**: Floor Progression
- **System**: GameManager, DungeonGenerator
- **Priority**: MVP
- **Depends on**: Tasks 1.2, 2.1

**What to build:**
Add logic to GameManager to track the current floor number (starting at 1). When all enemies are dead, trigger a "floor clear" condition. The player can then take stairs (a special tile or object) to descend to the next floor. When descending, GameManager increments the floor counter, DungeonGenerator generates a new floor, and all entities reset (new player position, new enemies). Repeat until the player reaches floor 10 (win condition for MVP).

**Files to create/modify:**
- `Assets/Scripts/Core/GameManager.cs` — add floor tracking and clear logic (modify from Task 1.2)
- `Assets/Scripts/Systems/DungeonGenerator.cs` — floor difficulty scales with floor number (modify from Task 2.1)
- `Assets/Scripts/Systems/DungeonSpawner.cs` — spawn stairs on floor clear (modify from Task 2.6)

**Implementation notes:**
GameManager should expose a `CurrentFloor` property and a `FloorClearedEvent` that fires when all enemies are dead. Subscribe to this event in DungeonSpawner to spawn the stairs. When the player interacts with stairs (collision or input), increment CurrentFloor, call `DungeonGenerator.GenerateFloor(CurrentFloor)`, and wait for `OnFloorGenerated` to respawn entities. Floor difficulty can scale simply: increase enemy count and stats (HP, Attack) slightly each floor (e.g., +2% per floor). For MVP, just spawn more enemies per floor; save AI difficulty scaling for post-MVP.

**Acceptance criteria:**
- [ ] Floor number increments after clearing all enemies
- [ ] New floor generates with correct difficulty scaling
- [ ] Stairs appear after enemies are cleared
- [ ] Player can descend to next floor

**Test approach:**
Enter play mode. Kill all enemies. Verify a stairs object appears. Click on stairs and verify the next floor generates with more enemies. Repeat until reaching floor 10. Verify a "You Win!" message appears (even if just console log).

---

#### Task 3.5: Implement Permadeath & Game Over
- **Feature**: Permadeath
- **System**: GameManager, EntityStats
- **Priority**: MVP
- **Depends on**: Tasks 1.2, 2.4

**What to build:**
Subscribe to the player's EntityStats.OnDeath event in GameManager. When it fires, trigger a game over state. Display a "Game Over" screen showing the final floor reached and allow the player to return to MainMenu. Implement permadeath: all progress is lost, and the player must start a new run from floor 1 with fresh stats.

**Files to create/modify:**
- `Assets/Scripts/Core/GameManager.cs` — listen to player death and transition to GameOver state (modify from Task 1.2)
- `Assets/Scripts/UI/GameOverScreen.cs` — new MonoBehaviour to display game over UI
- `Assets/Scenes/Gameplay.unity` — add GameOverScreen canvas

**Implementation notes:**
When EntityStats.OnDeath fires on the player, pause the game (set `Time.timeScale = 0` or disable input). Show a GameOverScreen that displays final floor number and has a "Return to MainMenu" button. When the button is clicked, reload MainMenu (reset TimeScale to 1). For MVP, keep the game over screen simple: just show floor number and a button. Defer detailed stats/meta-progression to Sprint 5.

**Acceptance criteria:**
- [ ] Player death triggers GameOverScreen
- [ ] Game over screen shows final floor
- [ ] Return to MainMenu button works
- [ ] Returning to MainMenu resets run state

**Test approach:**
In play mode, move the player next to an enemy and let the enemy attack until the player dies. Verify GameOverScreen appears and shows the floor number. Click "Return to MainMenu" and verify MainMenu loads.

---

#### Task 3.6: Create Warrior Class Definition
- **Feature**: Player Classes (Warrior MVP)
- **System**: None
- **Priority**: MVP
- **Depends on**: Task 1.4

**What to build:**
Create a Warrior ClassDefinition ScriptableObject with starting stats (e.g., HP: 30, Attack: 8, Defense: 4, Speed: 3) and a starting weapon/armor item (e.g., Iron Sword, Leather Armor). This is the only class available in MVP; Rogue and Mage come in Sprint 3+. Update the player spawn logic to use this class definition to set up EntityStats.

**Files to create/modify:**
- `Assets/ScriptableObjects/Classes/Warrior.asset` — Warrior ClassDefinition instance
- `Assets/Scripts/Systems/DungeonSpawner.cs` — apply class stats to player on spawn (modify from Task 2.6)

**Implementation notes:**
Create the Warrior asset in the Inspector by right-clicking in the project folder and selecting "Create > ClassDefinition". Set the starting stats. For MVP, you don't need starting items yet—just stats. Later, you can add a `startingItems[]` array to the ClassDefinition. When spawning the player, load the Warrior ClassDefinition from a hardcoded reference (or through GameManager) and apply its stats to the player's EntityStats.

**Acceptance criteria:**
- [ ] Warrior ClassDefinition created with correct stats
- [ ] Player spawns with Warrior stats
- [ ] Stats match PRD class definition (high HP, high Defense)

**Test approach:**
Enter play mode and verify the player starts with the Warrior's HP and Attack values (visible in Inspector or via console logs). Verify these stats affect combat (e.g., high Defense reduces damage taken).

---

### Sprint 4: Class Selection, Balanced Combat & Polish
**Goal**: Three classes are playable (Warrior, Rogue, Mage) with distinct feels, combat is balanced, UI shows player stats and inventory, and the game feels ready for Alpha.
**Estimated effort**: 8 tasks, roughly 6–8 hours

---

#### Task 4.1: Create Class Selection UI (MainMenu)
- **Feature**: Player Classes
- **System**: None
- **Priority**: MVP
- **Depends on**: Task 1.5

**What to build:**
Extend the MainMenu with a class selection screen. Display three class options (Warrior, Rogue, Mage) with their stat summaries and a description. Allow the player to select a class, which is saved to GameManager. The Play button should now load Gameplay with the selected class instead of hardcoded Warrior.

**Files to create/modify:**
- `Assets/Scenes/MainMenu.unity` — add class selection UI elements (modify from Task 1.5)
- `Assets/Scripts/UI/ClassSelectionPanel.cs` — new MonoBehaviour for class selection logic
- `Assets/Scripts/Core/GameManager.cs` — store selected class (modify from Task 1.2)

**Implementation notes:**
Create three buttons, one for each class. Each button has an image (class icon) and text (class name and brief description). When clicked, highlight the selected button and save the class choice to GameManager (e.g., `GameManager.SelectedClass = Rogue`). Display the selected class's starting stats (HP, Attack, Defense) in a text display. The Play button now reads the selected class and applies it during player spawn. For MVP, keep the UI simple: three buttons and a summary panel.

**Acceptance criteria:**
- [ ] Three class options displayed
- [ ] Class can be selected
- [ ] Selected class is applied to player at spawn
- [ ] Stats shown match ClassDefinition

**Test approach:**
Enter play mode in MainMenu. Click each class and verify the stats displayed change. Click Play and verify the player spawns with the selected class's stats. Start a new run with a different class and verify the stats change.

---

#### Task 4.2: Create Rogue Class Definition
- **Feature**: Player Classes (Rogue)
- **System**: None
- **Priority**: MVP
- **Depends on**: Task 1.4

**What to build:**
Create a Rogue ClassDefinition with starting stats emphasizing low HP, high Attack, and medium Speed (e.g., HP: 15, Attack: 12, Defense: 2, Speed: 5). Include a unique ability placeholder (e.g., "Dodge: 30% chance to evade attacks"). This is a post-MVP detail, but add the field to the ClassDefinition so it's ready for Sprint 5+.

**Files to create/modify:**
- `Assets/ScriptableObjects/Classes/Rogue.asset` — Rogue ClassDefinition instance
- `Assets/Scripts/Core/ClassDefinition.cs` — add classAbility field if not already present (modify from Task 1.4)

**Implementation notes:**
Rogue should feel fast and fragile: lower HP and Defense, but higher Attack and Speed. This means rogues act more often (higher Speed) and deal more damage, but can be killed quickly. For MVP, the class ability (Dodge) is just flavor text; the actual mechanic is implemented in Sprint 5+.

**Acceptance criteria:**
- [ ] Rogue ClassDefinition created with correct stats
- [ ] Stats emphasize speed and damage over durability
- [ ] Ability field is present (even if non-functional)

**Test approach:**
Select Rogue in MainMenu and start a run. Verify the player spawns with lower HP but higher Attack/Speed. Compare combat against an enemy with Warrior—Rogue should deal more damage but take more damage.

---

#### Task 4.3: Create Mage Class Definition
- **Feature**: Player Classes (Mage)
- **System**: None
- **Priority**: MVP
- **Depends on**: Task 1.4

**What to build:**
Create a Mage ClassDefinition with the lowest HP, medium Attack, and medium Speed, but add a mana system placeholder (e.g., HP: 12, Attack: 10, Defense: 1, Speed: 4, Mana: 20). Mage's unique mechanic is casting spells with mana costs; this is implemented in Sprint 5+. For MVP, Mages fight like other classes but with the mana field present.

**Files to create/modify:**
- `Assets/ScriptableObjects/Classes/Mage.asset` — Mage ClassDefinition instance
- `Assets/Scripts/Core/ClassDefinition.cs` — add mana field to ClassDefinition (modify from Task 1.4)
- `Assets/Scripts/Core/EntityStats.cs` — add Mana and MaxMana properties (modify from Task 2.4)

**Implementation notes:**
Mage is the glass cannon: very low HP and Defense, but the mana system hints at ranged/spellcasting. The mana system isn't implemented yet, so for MVP, Mages just use regular attacks. Add Mana and MaxMana properties to EntityStats so the player can see them in the UI.

**Acceptance criteria:**
- [ ] Mage ClassDefinition created with correct stats and mana
- [ ] Player spawns with mana field populated
- [ ] Mage stats emphasize frailty

**Test approach:**
Select Mage and start a run. Verify mana is displayed in the HUD (added in Sprint 4.4). Verify Mage has the lowest HP and can be killed quickly.

---

#### Task 4.4: Create In-Game HUD (Stats, Inventory Display)
- **Feature**: UI
- **System**: None
- **Priority**: MVP
- **Depends on**: Tasks 2.4, 3.2

**What to build:**
Create an on-screen HUD that displays the player's current stats (HP, current floor), inventory (currently equipped weapon, armor), and other important info. The HUD should update in real-time as stats change. Implement panels for health, floor number, and equipment slots.

**Files to create/modify:**
- `Assets/Scenes/Gameplay.unity` — add HUD canvas with stat/inventory displays (modify from Task 2.1)
- `Assets/Scripts/UI/HUDController.cs` — new MonoBehaviour updating HUD displays based on player/inventory events

**Implementation notes:**
Attach HUDController to the HUD canvas. Subscribe to EntityStats.OnHealthChanged to update the HP display. Subscribe to InventorySystem.OnItemPickup and OnItemEquipped (create these events if they don't exist) to update the equipment display. Use TextMeshPro Text components for labels. For MVP, keep the HUD simple: HP bar, floor number, equipped weapon name, equipped armor name. Add mana bar for Mage. Arrange elements in a corner (e.g., top-left or bottom-left).

**Acceptance criteria:**
- [ ] HUD displays current HP and max HP
- [ ] HUD displays current floor
- [ ] HUD displays equipped weapon and armor
- [ ] HUD updates in real-time
- [ ] Mage displays mana

**Test approach:**
Enter play mode in Gameplay. Verify HUD appears with correct values. Take damage and verify HP display updates. Pick up an item and verify equipped gear updates. Descend a floor and verify the floor number increments.

---

#### Task 4.5: Create Weapon & Armor Items
- **Feature**: Items
- **System**: InventorySystem
- **Priority**: MVP
- **Depends on**: Task 1.4

**What to build:**
Create 5–10 ItemData ScriptableObjects for weapons and armor: starting items (Iron Sword, Leather Armor) and loot items (Steel Sword +2 Attack, Steel Armor +2 Defense, etc.). Each item should have a name, type (weapon/armor), stat bonuses, and a sprite. These items will be placed in the world and dropped by enemies.

**Files to create/modify:**
- `Assets/ScriptableObjects/Items/Weapons/IronSword.asset`, `SteelSword.asset`, etc. — ItemData instances
- `Assets/ScriptableObjects/Items/Armor/LeatherArmor.asset`, `SteelArmor.asset`, etc. — ItemData instances
- `Assets/Art/item_sprites/` — placeholder sprites for each item (simple 32x32 pixel art or colored squares)

**Implementation notes:**
For each item, set: name, description, type (weapon or armor), stat bonuses (e.g., +2 Attack, +1 Defense), and a sprite. Create folders `Items/Weapons` and `Items/Armor` under ScriptableObjects. For MVP, keep item variety small: 2–3 weapon tiers, 2–3 armor tiers. Save more items for post-MVP. The stat bonuses should be additive—when the item is equipped, add its bonuses to the player's current stats.

**Acceptance criteria:**
- [ ] 10+ ItemData assets created with varied stat bonuses
- [ ] Items have sprites and descriptions
- [ ] Items can be placed in the world and picked up

**Test approach:**
Place ItemData instances as ItemPickups in the Gameplay scene. Enter play mode and move over them to verify they're picked up and added to inventory. Equip them and verify their stat bonuses are applied (display in HUD).

---

#### Task 4.6: Implement Equipment Stat Bonuses
- **Feature**: Items
- **System**: InventorySystem, EntityStats
- **Priority**: MVP
- **Depends on**: Tasks 3.2, 2.4, 4.5

**What to build:**
When an item is equipped, apply its stat bonuses to the player's EntityStats. When unequipped, remove the bonuses. Implement this via an event system: InventorySystem emits `OnItemEquipped` and `OnItemUnequipped` events with the item's bonus data. EntityStats listens and applies/removes the bonuses.

**Files to create/modify:**
- `Assets/Scripts/Systems/InventorySystem.cs` — emit equipment events (modify from Task 3.2)
- `Assets/Scripts/Core/EntityStats.cs` — apply equipment bonuses (modify from Task 2.4)

**Implementation notes:**
Add a method to EntityStats: `ApplyBonus(ItemData item)` that adds all of item's stat bonuses to the current stats. Add the reverse: `RemoveBonus(ItemData item)`. When InventorySystem equips an item, call `ApplyBonus()`. When unequipped, call `RemoveBonus()`. Store active bonuses in a list so they can be cleanly removed. For MVP, only support weapon and armor bonuses to Attack, Defense, and HP (if applicable). Defer more complex modifiers to later sprints.

**Acceptance criteria:**
- [ ] Equipped items increase relevant stats
- [ ] Stat increases are visible in HUD
- [ ] Unequipping items decreases stats back to base

**Test approach:**
Pick up a Steel Sword (if it has +2 Attack). Verify the HUD Attack value increases by 2. Pick up Steel Armor (+2 Defense). Verify Defense increases by 2. Unequip the sword and verify Attack decreases. Take damage and verify that increased Defense reduces damage taken.

---

#### Task 4.7: Implement Boss Encounters (Every 5 Floors)
- **Feature**: Boss Fights
- **System**: EnemyAI, DungeonGenerator
- **Priority**: MVP
- **Depends on**: Tasks 2.1, 2.5, 3.1

**What to build:**
Modify DungeonGenerator to spawn a boss enemy every 5 floors (floors 5, 10, 15, etc.). The boss is a special enemy with higher stats, unique appearance, and a loot table with guaranteed good items. Bosses are otherwise treated like normal enemies in combat. For MVP, use the existing EnemyAI system and just scale the stats.

**Files to create/modify:**
- `Assets/ScriptableObjects/Enemies/BossConfig.asset` — EnemyConfig for boss with high stats
- `Assets/Prefabs/Boss.prefab` — boss prefab with unique sprite and higher HP/Attack
- `Assets/Scripts/Systems/DungeonGenerator.cs` — check if floor % 5 == 0 and spawn boss instead of normal enemies (modify from Task 2.1)
- `Assets/Scripts/UI/HUDController.cs` — display boss name in HUD when present (modify from Task 4.4)

**Implementation notes:**
When generating a floor that's a multiple of 5, instantiate a single Boss prefab instead of multiple enemies. Scale boss stats: HP = floor * 5, Attack = base_attack + (floor / 5), Defense = base_defense + (floor / 10). Bosses should have guaranteed drops (100% chance) of a valuable item. For MVP, keep boss behavior the same as normal enemies (patrol/chase/attack). Unique boss abilities (e.g., special attacks) are post-MVP.

**Acceptance criteria:**
- [ ] Boss appears on floors 5, 10, 15, etc.
- [ ] Boss has higher stats than normal enemies
- [ ] Boss drops guaranteed loot
- [ ] Boss death clears the floor like normal enemies

**Test approach:**
Reach floor 5 and verify a boss spawns (visually distinct, higher stats in HUD). Fight the boss and verify it requires more hits to kill. Verify it drops loot. Descend to floor 6 and verify normal enemies return. Reach floor 10 (win condition) and verify the boss is the final challenge.

---

#### Task 4.8: Balance Combat & Difficulty Progression
- **Feature**: Combat, Difficulty
- **System**: EnemyAI, EntityStats
- **Priority**: MVP
- **Depends on**: All previous combat tasks

**What to build:**
Playtest the game multiple times and adjust enemy stats, player class stats, item bonuses, and enemy spawning to create a smooth difficulty curve. The goal is: early floors feel easy (player learns mechanics), mid floors are challenging (need good items and positioning), late floors are hard (bosses on 5/10 are real threats), and floor 10 feels like a climactic win.

**Files to create/modify:**
- All ClassDefinition assets — adjust starting stats based on playtest feedback
- All ItemData assets — adjust stat bonuses for balance
- EnemyConfig instances — adjust enemy stats for appropriate difficulty per floor
- `Assets/Scripts/Systems/DungeonGenerator.cs` — adjust enemy spawn count and difficulty scaling (modify from Task 2.1)

**Implementation notes:**
Play through the game 3–5 times with each class and take notes on where the difficulty feels off. Are early floors too easy? Buff enemies slightly or add more of them. Is the boss impossible? Reduce its stats or add a healing potion drop to the floor before the boss. The goal is that a skilled player can beat floor 10 consistently with good itemization and smart play. For MVP, aim for: floor 1–2 trivial, floor 3–4 easy, floor 5 moderate (first boss), floor 6–9 challenging, floor 10 climactic. This is the most playtesting-heavy task, and estimates are rough.

**Acceptance criteria:**
- [ ] Difficulty progresses smoothly across 10 floors
- [ ] Player can win with any class
- [ ] Boss fights feel appropriately challenging
- [ ] No obvious balance issues (e.g., one class vastly overpowered)

**Test approach:**
Play the game end-to-end multiple times, keeping a log of where difficulty spikes occur. Adjust stats in ScriptableObjects and re-test. Repeat until satisfied with the curve.

---

### Sprint 5: Meta-Progression & Polish
**Goal**: Meta-progression system allows unlocking new items and class variants between runs, game is feature-complete and ready for Beta, WebGL build is generated.
**Estimated effort**: 5 tasks, roughly 5–6 hours

---

#### Task 5.1: Implement MetaProgression System
- **Feature**: Meta-Progression
- **System**: GameManager, MetaUnlock
- **Priority**: Post-MVP
- **Depends on**: Tasks 1.2, 3.5

**What to build:**
Create the MetaProgression MonoBehaviour as a persistent system (stored in a save file or PlayerPrefs). Track unlocked items, class variants, and dungeon modifiers. Each unlock has a condition (e.g., "reach floor 5", "kill 10 enemies", "find the legendary sword") and a reward (e.g., new starting item, new class variant). On game start, apply unlocked rewards. When conditions are met during a run, register the unlock for next run.

**Files to create/modify:**
- `Assets/Scripts/Core/MetaProgression.cs` — new MonoBehaviour managing unlocks
- `Assets/Scripts/Core/MetaUnlock.cs` — ScriptableObject for unlock definitions (from Task 1.4)
- `Assets/Scripts/Core/GameManager.cs` — integrate MetaProgression (modify from Task 1.2)

**Implementation notes:**
Use PlayerPrefs or JSON serialization to save/load meta state (which unlocks are active). MetaProgression should be a singleton that persists across runs. When a condition is met (e.g., player reaches floor 5), log the unlock. At the end of the run, call `ProcessUnlocks()` to save completed conditions. On game start, apply unlocks to the player (e.g., add starting items to the Warrior class). For MVP, create 5–10 simple unlocks: "reach floor 3", "reach floor 5", "reach floor 10", "find 3 items", etc., with rewards like starting potions or alternate armor.

**Acceptance criteria:**
- [ ] Unlocks persist across runs
- [ ] Unlock conditions are checked during gameplay
- [ ] Completed unlocks award rewards
- [ ] Rewards are applied on next run

**Test approach:**
Play until reaching floor 5. Exit to MainMenu. Start a new run and verify an unlock reward is applied (e.g., starting item visible in inventory). Play to floor 10 and verify the win-condition unlock triggers.

---

#### Task 5.2: Implement Unlock Display in MainMenu
- **Feature**: Meta-Progression UI
- **System**: None
- **Priority**: Post-MVP
- **Depends on**: Tasks 1.5, 5.1

**What to build:**
Replace the "Meta-Progression Coming Soon" placeholder in MainMenu with a functional display. Show the player's accumulated unlocks: newly unlocked items, class variants, modifiers. Display as a scrollable list or grid. Allow the player to preview what each unlock does before selecting a class and starting a run.

**Files to create/modify:**
- `Assets/Scenes/MainMenu.unity` — add unlock display UI (modify from Task 1.5)
- `Assets/Scripts/UI/UnlockDisplayPanel.cs` — new MonoBehaviour to show unlocks

**Implementation notes:**
Query MetaProgression for all active unlocks. For each, display a card showing the unlock name, description, and reward (e.g., "Starting Potion — Heals 10 HP at the start of a run"). Use a Grid Layout Group to arrange cards. Include a "Recently Unlocked" section at the top (unlocks from the last 2 runs). For MVP, keep this simple—just a list of names and descriptions.

**Acceptance criteria:**
- [ ] Unlock display shows all active unlocks
- [ ] Each unlock displays name and reward
- [ ] Recently unlocked items are highlighted
- [ ] UI is readable and organized

**Test approach:**
After completing a run where an unlock is triggered, return to MainMenu. Verify the unlock appears in the display with its name and reward. Start another run, complete it, and verify a different unlock appears.

---

#### Task 5.3: Create Class Variants (Unlock Variants of Rogue & Mage)
- **Feature**: Player Classes
- **System**: None
- **Priority**: Post-MVP
- **Depends on**: Tasks 4.2, 4.3, 5.1

**What to build:**
Create variant ClassDefinitions for Rogue and Mage: e.g., "Rogue (Assassin)" with higher starting Attack, "Rogue (Shadowblade)" with Dodge ability, "Mage (Pyromancer)" with fire spells, "Mage (Frostmancer)" with ice spells. Each variant is a separate ClassDefinition asset that can be unlocked through meta-progression. Update the class selection screen to show locked variants grayed out.

**Files to create/modify:**
- `Assets/ScriptableObjects/Classes/RogueAssassin.asset`, `RogueShadowblade.asset`, `MagePyromancer.asset`, `MageFrostmancer.asset` — new ClassDefinition variants
- `Assets/Scripts/UI/ClassSelectionPanel.cs` — display locked variants grayed out (modify from Task 4.1)
- `Assets/Scripts/Core/MetaProgression.cs` — check unlock status when class is selected (modify from Task 5.1)

**Implementation notes:**
Each variant is a new ClassDefinition with tweaked stats and an unlockable flag. When the class selection panel initializes, query MetaProgression to see which variants are unlocked. Display locked variants with a "Locked" label and disable their selection. When a variant is unlocked through meta-progression, it becomes available for selection. For MVP, create 2 variants per class (4 total). Save additional variants for post-MVP.

**Acceptance criteria:**
- [ ] Class variants have distinct stats and abilities
- [ ] Variants are shown as locked initially
- [ ] Unlocking a variant makes it selectable
- [ ] Variant stats are applied correctly when selected

**Test approach:**
Start the game and verify only base Warrior, Rogue, and Mage are available. Complete a run that unlocks a variant. Return to class selection and verify the variant is now selectable. Start a run with the variant and verify the stats are correct.

---

#### Task 5.4: Implement Auto-Save & Run History
- **Feature**: Infrastructure
- **System**: GameManager
- **Priority**: Post-MVP
- **Depends on**: Tasks 1.2, 3.5

**What to build:**
Add automatic saving of run data (final floor reached, items found, class used, time played) to a run history log. Each time a run ends (player death or win), save a summary. Display the run history in MainMenu, showing recent runs and statistics (highest floor, average floor, total runs). This provides a satisfying meta-narrative and helps the player track progress.

**Files to create/modify:**
- `Assets/Scripts/Core/RunData.cs` — serializable class for storing run info
- `Assets/Scripts/Core/GameManager.cs` — save run data on run end (modify from Task 1.2)
- `Assets/Scripts/UI/RunHistoryPanel.cs` — display run history in MainMenu
- `Assets/Scenes/MainMenu.unity` — add run history UI (modify from Task 1.5)

**Implementation notes:**
Create a RunData struct with: final floor, class used, items collected, run duration, timestamp. On game over or win, serialize RunData to JSON and save to a persistent file (e.g., `Application.persistentDataPath/run_history.json`). In MainMenu, deserialize the file and display the last 10 runs in a scrollable list. Calculate and display stats: highest floor reached, average floor, total runs. For MVP, keep this simple—just a list of runs with floor and class.

**Acceptance criteria:**
- [ ] Run data is saved at the end of each run
- [ ] Run history displays recent runs
- [ ] Statistics are calculated correctly
- [ ] Player can see their progression over time

**Test approach:**
Play 3–5 runs, reaching different floors with different classes. Return to MainMenu. Verify run history shows all runs with correct floor and class data. Verify highest floor and average floor calculations are correct.

---

#### Task 5.5: Generate WebGL Build & Finalize for Beta
- **Feature**: Infrastructure
- **System**: None
- **Priority**: Post-MVP
- **Depends on**: All other tasks

**What to build:**
Configure the project for WebGL export. Adjust settings for web compatibility (canvas size, keyboard input, no frame rate cap). Build a WebGL version of the game and test it in a browser. Finalize the game as "Beta" (feature-complete, playable end-to-end, ready for wider testing).

**Files to create/modify:**
- `ProjectSettings/ProjectSettings.asset` — WebGL build settings (platform selection, canvas size, etc.)
- `Assets/WebGLTemplates/` — custom HTML template (optional, for branding)
- Build output: `Builds/WebGL/`

**Implementation notes:**
Switch build platform to WebGL in Build Settings. Adjust player settings: set canvas size to 1920x1080, disable fullscreen, ensure all input systems work with keyboard (arrow keys/WASD should work). Build to a folder (e.g., `Builds/WebGL`). Test the build locally using a simple HTTP server (e.g., `python -m http.server` in the build folder). Verify the game runs, input works, and no console errors appear. Create a release notes file documenting the Beta version and known limitations. Upload to itch.io for playtesting.

**Acceptance criteria:**
- [ ] WebGL build completes without errors
- [ ] Game runs in a web browser
- [ ] Input (WASD, spacebar) works in browser
- [ ] No major console errors or warnings
- [ ] Game is feature-complete and playable end-to-end

**Test approach:**
Build to WebGL. Open the HTML file in a web browser (Chrome, Firefox, Safari). Play through a complete run from start to finish. Verify movement, combat, item pickup, and floor progression all work. Test class selection and meta-progression. Confirm no input lag or rendering issues.

---

## Dependency Graph

```
Task 1.1 (Project Setup)
  → Task 1.2 (GameManager)
    → Task 1.5 (MainMenu)
      → Task 4.1 (Class Selection)
    → Task 1.3 (TurnManager)
      → Task 2.2 (PlayerController)
        → Task 2.3 (Movement Action)
      → Task 2.5 (EnemyAI)
        → Task 3.1 (Combat Action)
      → Task 2.4 (EntityStats)
        → Task 3.2 (InventorySystem)
          → Task 3.3 (Loot Drops)
          → Task 4.5 (Item Data)
            → Task 4.6 (Equipment Bonuses)
  → Task 1.4 (ScriptableObjects)
    → Task 3.6 (Warrior Class)
    → Task 4.2 (Rogue Class)
    → Task 4.3 (Mage Class)
    → Task 4.5 (Items)
    → Task 5.3 (Class Variants)

Task 1.6 (Build Settings)
  ← depends on all scene creation tasks

Task 2.1 (DungeonGenerator)
  → Task 2.6 (Spawning)
    → Task 3.4 (Floor Progression)
      → Task 4.7 (Boss Encounters)

Task 3.4 (Floor Progression)
  → Task 3.5 (Permadeath)

Task 4.4 (HUD)
  ← depends on EntityStats, InventorySystem

Task 4.8 (Balance)
  ← depends on all gameplay systems

Task 5.1 (MetaProgression)
  → Task 5.2 (Unlock Display)
    → Task 5.3 (Class Variants)

Task 5.4 (Run History)
  ← depends on MetaProgression

Task 5.5 (WebGL Build)
  ← depends on all other tasks
```

---

## Post-MVP Backlog

| ID | Feature | Description | Depends on |
|----|---------|-------------|------------|
| B1 | Rogue Abilities | Implement Dodge mechanic (30% evade chance) and Stealth (avoid enemy detection) | Task 4.2, Combat System |
| B2 | Mage Spellcasting | Implement mana system and spell actions (Fireball, Frostbolt, etc.) with mana costs | Task 4.3, InventorySystem |
| B3 | Advanced Enemy AI | Pathfinding (A*), group tactics, special abilities for certain enemy types | Task 2.5, DungeonGenerator |
| B4 | Boss Unique Abilities | Each boss has a signature attack (e.g., AOE damage, heal, summon minions) | Task 4.7 |
| B5 | Consumable Items | Potions with effects (healing, stat boost, temporary invulnerability) that can be used mid-run | Task 3.2, InventorySystem |
| B6 | Procedural Difficulty Modifiers | Unlock modifiers (e.g., "Harder Enemies", "Less Loot", "Curse") that affect run difficulty/rewards | Task 5.1, DungeonGenerator |
| B7 | Audio System | Background music, sound effects for actions (attack, pickup, death), ambient dungeon sounds | Task 1.1 |
| B8 | Advanced Animations | Sprite animations for combat (attack, hit, death), walking, ability casting | Task 2.2, 2.5 |
| B9 | Particle Effects | Visual feedback for combat (hit sparks, loot sparkles), spell effects, boss attacks | Task 3.1, 4.7 |
| B10 | Leaderboard / Ascension Mode | Track top scores, unlock harder difficulty, prestige system with greater rewards | Task 5.4 |
| B11 | Tutorial / Onboarding | Interactive tutorial on first launch explaining mechanics, controls, class abilities | Task 4.1 |
| B12 | Accessibility | Colorblind-friendly UI, configurable key bindings, difficulty accessibility options | Task 4.4 |
| B13 | Additional Classes | Paladin, Ranger, Barbarian, Warlock with unique mechanics | Task 4.1, 5.3 |
| B14 | Dungeon Themes | Different visual themes per floor (cave, crypt, temple, void) with unique tile sets | Task 2.1 |
| B15 | Secret Rooms & Encounters | Hidden rooms with elite enemies, treasure, or lore; random NPC encounters | Task 2.1, 3.2 |

---

## Summary

**DungeonDelve SDD** breaks down the roguelike into 32 actionable tasks across 6 sprints:

- **Sprint 1 (Infrastructure)**: Bootstrap, managers, ScriptableObjects, MainMenu
- **Sprint 2 (Movement & Turns)**: Grid movement, procedural dungeon, enemy spawning
- **Sprint 3 (Combat)**: Combat system, loot drops, permadeath, first win condition
- **Sprint 4 (Polish & Classes)**: Class selection, three playable classes, equipment system, HUD, bosses, balance
- **Sprint 5 (Meta)**: Meta-progression, unlocks, run history, WebGL build

Each task specifies files to create, implementation notes, acceptance criteria, and test approaches. By following this roadmap sequentially, a solo developer can build a complete, playable roguelike in approximately 25–35 hours of focused development.

The MVP (Sprints 1–4, 20 tasks) delivers a feature-complete, balanced, three-class roguelike with procedural dungeons, turn-based combat, permadeath, and a clear win condition (reach floor 10).

Post-MVP content (Sprint 5 + Backlog) adds meta-progression, WebGL support, advanced features (spellcasting, abilities, procedural modifiers), and polish (audio, animations, accessibility).
