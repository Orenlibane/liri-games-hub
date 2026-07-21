# Dungeon Crawler — Product Requirements Document

## 1. Game Overview & Vision

### 1.1 Elevator Pitch
A turn-based roguelike dungeon crawler with procedurally generated floors, tactical combat, and persistent meta-progression. Choose from three classes, fight through enemy-filled dungeons, discover loot and equipment, and face escalating boss encounters every five floors. Death is permanent, but unlocks and upgrades earned between runs fuel long-term progression.

This document describes the complete product, architecture, and development plan for a PC-exclusive roguelike dungeon crawler game built in Unity.

### 1.2 Genre & References
- **Primary genre**: Roguelike, turn-based tactical dungeon crawler
- **Sub-genres**: Procedural generation, permadeath, meta-progression
- **Reference games**:
  - *Hades* — Draw from permadeath with meta-progression unlocks, escalating difficulty, and multiple class/weapon variety keeping each run fresh
  - *Dungeon Crawl Stone Soup* — Turn-based tactical combat, procedural dungeon generation, equipment-driven progression
  - *Slay the Spire* — Deck-building/run-based progression, strategic decision-making on each floor, meta-progression between runs

### 1.3 Target Platform
- **Primary platform**: PC (Windows, Mac, Linux via Steam or itch.io)
- **Minimum spec**: 4GB RAM, any modern CPU (2015+), integrated graphics
- **Target resolution**: 1280x720 (scalable up to 1920x1080)
- **Aspect ratio**: 16:9

### 1.4 Art Style & Tone
- **Visual direction**: 2D pixel art, top-down isometric-style view (retro dungeon crawler aesthetic)
- **Mood and atmosphere**: Dark fantasy, procedural dungeon exploration with moments of triumph and dread
- **Color palette**: Dark stone grays and blacks (dungeon), accent colors per class (blue for Mage, red for Warrior, green for Rogue), bright item/equipment highlights
- **UI style**: Pixel-perfect UI with clear readability, retro-RPG HUD (health bars, inventory grid, action buttons)

### 1.5 Target Audience
- **Who**: Indie game enthusiasts aged 18-40 who enjoy roguelikes, turn-based strategy, and long-form progression
- **Session length**: 15-60 minutes per run (average 30-40 min), multiple runs encouraged over weeks/months
- **Difficulty approach**: Hard but fair; permadeath is core, but meta-progression ensures meaningful long-term advancement. Difficulty scaling optional (difficulty modifiers unlock as the player progresses)

---

## 2. Core Mechanics & Systems

### 2.1 Core Gameplay Loop
```
Enter Dungeon Floor
  → Explore procedurally generated layout (moving, seeing enemies/items)
  → Encounter enemy or item
    → Combat Turn (player action → enemy reaction → feedback)
    → Loot pickup (add to inventory)
  → Move to next room
  → Reach stairs (advance floor) or die
Repeat until boss floor or death
  → If death: return to main menu, unlock meta progression, restart
  → If boss defeated: advance to next floor set
```

**Moment-to-moment**: Player stands in a dungeon room, visible enemies in line of sight. Player chooses action (attack, ability, move, use item). Enemy reacts. Environmental feedback (damage numbers, status effects, sounds). Next player turn.

### 2.2 Player Mechanics

#### Mechanic: **Class Selection**
- **Input**: Main menu class choice (Warrior, Rogue, Mage)
- **Behavior**: Locks in base stats, abilities, starting equipment for this run
- **Feedback**: UI highlight of selected class, character portrait, stat sheet preview
- **Edge cases**: Cannot change mid-run; class choice is permanent until run ends

#### Mechanic: **Movement**
- **Input**: Arrow keys or WASD to move in cardinal directions (no diagonal movement)
- **Behavior**: Player moves one tile per turn. Collision with walls/enemies blocks movement
- **Feedback**: Player sprite animates to new tile, camera centers on player
- **Edge cases**: Cannot move off-map; blocked tiles prevent movement and consume no action point

#### Mechanic: **Attack (Basic)**
- **Input**: Click enemy or hotkey while adjacent/in range
- **Behavior**: Damage calculated as `base_attack + weapon_bonus - enemy_defense`, variance ±10%
- **Feedback**: Hit/miss indicator (damage number or "Miss"), enemy hurt animation, hit sound
- **Edge cases**: Attack range varies by class/weapon (Warrior 1 tile, Rogue 2 tiles, Mage 3 tiles)

#### Mechanic: **Ability Usage**
- **Input**: Hotkey (E, R, etc.) or UI button while cooldown allows and resources available
- **Behavior**: Class-specific ability (Warrior: Cleave, Rogue: Backstab, Mage: Fireball) with resource cost and cooldown
- **Feedback**: Ability animation, AoE highlight (if applicable), status effect application, resource bar update
- **Edge cases**: Cannot cast without required resources; cannot spam if on cooldown

#### Mechanic: **Item Pickup**
- **Input**: Walk over item or "Loot" button when standing on item
- **Behavior**: Item moves to inventory. If inventory full, show "Full" prompt
- **Feedback**: Item collect sound, visual pop-up showing item name/rarity, inventory updated
- **Edge cases**: Inventory limited to 10 slots; player must manage what to carry

#### Mechanic: **Equipment Swap**
- **Input**: Inventory UI, drag/drop or "Equip" button on item
- **Behavior**: Un-equip current item, equip new one. Stats recalculate immediately
- **Feedback**: Stat comparison tooltip, character sprite updates visually (equipment shown), sound cue
- **Edge cases**: Cannot un-equip if it's the only weapon; restrictions by class (Mage can't equip heavy armor)

#### Mechanic: **Potion Use**
- **Input**: Inventory UI or hotkey
- **Behavior**: Consume one potion, apply effect (e.g., heal 50 HP). One potion per turn max
- **Feedback**: Healing animation (glow, numbers), HP bar updates, potion count decreases
- **Edge cases**: Cannot use if no potions; cannot overheal past max HP

#### Mechanic: **Stair Descent**
- **Input**: Walk onto stair tile or "Go Down" button
- **Behavior**: Save room state, generate next floor, reload dungeon. Floor counter increments
- **Feedback**: Transition screen (brief fade), floor number announced, new room loaded
- **Edge cases**: Boss floors are generated differently (single large arena)

### 2.3 Game Systems

#### System: **Turn-Based Combat**
- **Purpose**: Core interaction between player and enemies; tactical depth
- **Rules**:
  - Each turn, player acts first (move, attack, ability, item, or pass)
  - Enemies then react (move toward player or attack if in range)
  - Turns are discrete; real-time does not exist
  - Each action ends turn; next entity in initiative order acts
- **Player-facing data**:
  - Current HP / Max HP
  - Action status ("Ready", "Cooldown: 2 turns", etc.)
  - Enemy threat indicators (red outline if in attack range)
- **Internal state**:
  - Entity positions (grid-based)
  - Cooldown counters per ability
  - Status effect durations (poison, stun, bleed)
  - Initiative queue
- **Interactions**:
  - Triggers damage calculation (Combat System)
  - Triggers status effect application (Status Effect System)
  - Feeds data to UI (health bars, ability cooldowns)

#### System: **Procedural Dungeon Generation**
- **Purpose**: Each floor is unique; player must adapt to new layouts every floor
- **Rules**:
  - Seed-based generation (same seed = same floor, for testing/replication)
  - Room count: 4–8 rooms per floor
  - Room sizes: 10x10 to 15x15 tiles
  - Layout: Rooms connected by hallways, roughly linear progression toward stairs
  - Boss floors: Single large arena (20x20)
- **Player-facing data**:
  - Visible map (explored rooms only; fog of war on unexplored)
  - Enemy positions and types (visible in line of sight)
  - Item locations (visible in line of sight)
- **Internal state**:
  - Full floor layout (room connectivity, enemy placements, item spawns)
  - Seed for this floor
  - Explored rooms cache
- **Interactions**:
  - Driven by floor progression (ascending stairs triggers generation of next floor)
  - Supplies enemies to spawn (Enemy Spawn System)
  - Supplies items to place (Loot System)

#### System: **Enemy AI**
- **Purpose**: Enemies provide challenge and tactical variety
- **Rules**:
  - Behavior: State machine (Idle, Chase, Attack, Retreat)
  - Idle: Patrol or wait
  - Chase: Move toward player when in sight range (8 tiles)
  - Attack: Use melee or ranged attacks if in range (varies by enemy type)
  - Retreat: If HP < 30%, move away (some enemies only)
  - No diagonal movement (cardinal only)
- **Player-facing data**:
  - Enemy health bar and type name
  - Enemy ability indicators (telegraph effects)
- **Internal state**:
  - Current HP
  - Target (player or None)
  - Behavior state
  - Attack cooldown
- **Interactions**:
  - Reacts to player actions (chases, attacks)
  - Feeds combat results to Combat System
  - Provides loot when defeated (Loot System)

#### System: **Loot & Inventory**
- **Purpose**: Gear progression, tactical load-out decisions
- **Rules**:
  - Inventory: 10 slots, stackable items (potions) or 1 per slot (equipment)
  - Rarity tiers: Common, Uncommon, Rare, Legendary (affects stat values and drop rates)
  - Equipment slots: Head, Chest, Hands, Feet, Weapon (each grants stat bonuses)
  - Drop rate: ~60% chance per enemy, higher for higher-floor enemies
  - Potions: Healing (restore 50 HP), Mana (restore 30 mana), Damage Boost (+2 attack for 3 turns)
- **Player-facing data**:
  - Inventory grid with item icons, names, rarity colors
  - Equipment slots showing current gear
  - Stat deltas when hovering over items ("This sword is +5 Attack, -2 Defense")
- **Internal state**:
  - Item definitions (ScriptableObjects per item)
  - Current inventory array
  - Current equipment equipped
- **Interactions**:
  - Equipment affects player stats (feeds to Combat System)
  - Items spawn on floor (Procedural Generation)
  - Potions used in Combat System

#### System: **Player Progression (Per-Run)**
- **Purpose**: Player power curve within a single run
- **Rules**:
  - Player starts at level 1 with base class stats
  - XP gained per enemy defeated: `enemy_level * 10` XP
  - Level up thresholds: Exponential (`100 * level^1.5`)
  - On level up: +1 to two random stats (Attack, Defense, Max HP, or Mana), healing fully
  - Stat boosts also from equipment
- **Player-facing data**:
  - Current level and XP bar
  - Stat sheet (Attack, Defense, Max HP, Mana)
- **Internal state**:
  - Current XP total
  - Current level
  - Base stats per class
  - Equipment stat bonuses
- **Interactions**:
  - Stat changes affect damage calculations (Combat System)
  - Feeding to UI (level display, stat sheet)

#### System: **Meta-Progression (Between Runs)**
- **Purpose**: Long-term progression despite permadeath; incentivize repeated play
- **Rules**:
  - Currency: "Essence" earned from defeated enemies and found in chests (1-5 per enemy, 10-50 per floor)
  - Essence persists across deaths and resets
  - Unlock trees:
    - **Warrior**: Improved Cleave damage (3 upgrades), +1 Max HP per level (2 upgrades), etc. (max 10 upgrades)
    - **Rogue**: Increased backstab range, reduced ability cooldowns, poison damage boost
    - **Mage**: Increased fireball AoE, reduced mana costs, bonus starting mana
  - Cost: 50-500 Essence per unlock (scaling with tier)
  - Unlocks apply to new runs of that class (permanent until reset)
- **Player-facing data**:
  - Essence balance (displayed in main menu)
  - Upgrade tree UI showing unlocked/available/locked upgrades
- **Internal state**:
  - Total Essence earned this lifetime
  - Unlocked upgrades per class (saved to disk)
- **Interactions**:
  - Earned during Combat System (essence drops on defeat)
  - Applied to new runs (modifies starting stats/abilities)
  - Displayed in main menu

#### System: **Status Effects**
- **Purpose**: Tactical depth, crowd control, risk/reward from abilities
- **Rules**:
  - Types: Stun (skip next turn), Poison (1 damage per turn for N turns), Bleed (2 damage per turn), Slow (reduced movement), Weakness (reduced damage dealt)
  - Applied by abilities or items
  - Duration measured in turns
  - Multiple effects stack; duration decrements each turn
- **Player-facing data**:
  - Icons above character showing active effects and remaining duration
- **Internal state**:
  - Effect type, duration, potency (e.g., "Poison: 5 turns at 1 dmg/turn")
- **Interactions**:
  - Applied by Combat System (ability hits)
  - Resolved at turn start (damage dealt, turn skipped, etc.)
  - Displayed by UI System

#### System: **Boss Encounters**
- **Purpose**: Escalating challenge every 5 floors; boss loot for major upgrades
- **Rules**:
  - Boss floors: 5, 10, 15, 20 (configurable)
  - Boss arena: Single large room (20x20)
  - Boss stats: Scales with player level (Boss_HP = 100 + 20 * (floor / 5))
  - Boss abilities: 2-3 signature attacks with longer cooldowns (3-5 turns)
  - Boss loot: Guaranteed rare or legendary item + 100-200 Essence
  - If player dies to boss: Run ends, no essence earned (as penalty)
- **Player-facing data**:
  - Boss name and health bar (large, prominent)
  - Boss ability indicators (telegraph at 1 turn before cast)
- **Internal state**:
  - Boss type and tier
  - Boss attack patterns and cooldowns
  - Boss arena state
- **Interactions**:
  - Triggered by reaching floor 5, 10, etc.
  - Uses Combat System for combat resolution
  - Supplies boss loot on defeat

#### System: **Save/Load & Run State**
- **Purpose**: Persist run state across sessions; allow pause and resume
- **Rules**:
  - One active run at a time (current floor state)
  - Saving: Automatic on floor transition or manual via menu
  - Loading: Resume button on main menu shows last floor/run info
  - Death: Game over screen, offer "View Stats" or "Try Again"
  - Run stats: Floors cleared, enemies killed, essence earned, time elapsed
- **Player-facing data**:
  - Continue button on main menu (greyed if no active run)
  - Run summary after death (floors, kills, essence earned)
- **Internal state**:
  - Active run file (dungeon layout, player state, enemy positions, items)
  - Completed runs history (for leaderboard/stats tracking)
- **Interactions**:
  - Triggered by floor transitions
  - Triggered by death/run end
  - Triggered by pause menu save

### 2.4 AI & NPC Behavior

#### AI Decision-Making
- **Approach**: State machine (Idle, Chase, Attack, Retreat)
- **Decision tree**:
  1. Can I see the player? → Chase (move closer)
  2. Am I adjacent/in range? → Attack
  3. Am I low HP (<30%)? → Retreat (for cowardly enemies)
  4. Otherwise → Idle (patrol or wait)

#### Enemy Types
- **Basic Slime** (Floor 1–3): Low HP, slow movement, melee attack
- **Goblin** (Floor 2–5): Medium HP, erratic movement, ranged attack
- **Skeleton** (Floor 4–8): Higher HP, melee + ranged (varies), chasing behavior
- **Orc** (Floor 6–10): High HP, powerful melee, will charge if can see player
- **Fire Elemental** (Floor 8–15): Medium HP, ranged fire attack (AoE), keeps distance
- **Boss: Lich** (Floor 5): High HP, magic attacks, summons minions
- **Boss: Warlord** (Floor 10): Very high HP, cleave attacks, armor reduces damage
- **Boss: Demon** (Floor 15+): Extreme HP, multiple dangerous abilities, teleportation

#### Difficulty Scaling
- ⚠️ **Assumption**: Base difficulty is medium. Advanced players can unlock "Hard" mode (enemies have 50% more HP, deal 20% more damage) from meta-progression.
- Enemy stats scale with player level: `enemy_stat = base_stat + (player_level * 0.5)`
- Floor-based scaling: Each floor, enemy base HP +10%, damage +5%

### 2.5 Level / World Design

#### Level Structure
- **Type**: Procedurally generated, semi-linear progression
- **Layout**: 20 dungeon floors total
  - Floors 1–4: Introductory (simple room layouts, basic enemies)
  - Floors 5–9: Mid-game (3-room chains, tougher enemies, rare loot)
  - Floors 10–14: Late-game (complex layouts, elite enemies, legendary loot)
  - Floors 15–20: Endgame (maze-like, boss-level monsters, meta-progression gating)
- **Progression**: Linear descent (floor N must complete floor N–1); no branching

#### Key Landmarks
- **Staircase**: Always at end of each floor, player goal is to reach stairs
- **Treasure chests**: 1–2 per floor, contain high-value loot or essence
- **Pillars/Walls**: Terrain obstacles, create combat tactics (line of sight blocking)
- **Boss arena**: Distinctive large chamber with boss on floor 5, 10, 15, 20

#### Procedural Parameters (Tuned per Floor)
- Enemy density: 3–8 per floor (scales with progression)
- Loot density: 2–4 items + 1 chest per floor
- Room interconnectedness: Loosely linear (avoids circular loops to minimize backtracking)

---

## 3. Unity Technical Architecture

### 3.1 Project Structure
```
Assets/
├── Scripts/
│   ├── Core/
│   │   ├── GameManager.cs           # Persistent singleton, run state
│   │   ├── TurnManager.cs           # Turn order, turn execution
│   │   ├── MetaProgressionManager.cs # Essence tracking, upgrades
│   │   └── EventBus.cs              # Global event system
│   ├── Player/
│   │   ├── PlayerController.cs      # Player input and logic
│   │   ├── PlayerStats.cs           # Health, mana, level, exp
│   │   ├── PlayerAbilities.cs       # Ability definitions and casting
│   │   ├── ClassDefinition.cs       # Class-specific behavior (Warrior/Rogue/Mage)
│   │   └── InventoryManager.cs      # Item management
│   ├── Enemies/
│   │   ├── EnemyBase.cs             # Parent class for all enemies
│   │   ├── EnemyAI.cs               # State machine AI
│   │   ├── EnemySpawner.cs          # Spawning logic per floor
│   │   └── BossController.cs        # Boss-specific behavior
│   ├── Systems/
│   │   ├── DungeonGenerator.cs      # Procedural generation
│   │   ├── LootSystem.cs            # Item drops and treasures
│   │   ├── CombatResolver.cs        # Damage calc, hit/miss logic
│   │   ├── StatusEffectSystem.cs    # Status effect application/resolution
│   │   ├── SaveLoadManager.cs       # Run persistence
│   │   └── ItemDefinition.cs        # Item data (generic, used with SO)
│   ├── UI/
│   │   ├── HUD.cs                   # In-game HUD (health, level, etc.)
│   │   ├── InventoryUI.cs           # Inventory screen
│   │   ├── MenuManager.cs           # Main menu, pause menu
│   │   ├── CombatLogUI.cs           # Floating damage numbers, hit feedback
│   │   └── UpgradeTreeUI.cs         # Meta-progression upgrade display
│   └── Utilities/
│       ├── Constants.cs             # Game constants (layer names, tags)
│       ├── Extensions.cs            # Utility methods
│       └── GridHelper.cs            # Grid-based pathfinding, line of sight
├── ScriptableObjects/
│   ├── ClassData/
│   │   ├── WarriorData.asset
│   │   ├── RogueData.asset
│   │   └── MageData.asset
│   ├── EnemyData/
│   │   ├── SlimeData.asset
│   │   ├── GoblinData.asset
│   │   └── ... (per enemy type)
│   ├── ItemData/
│   │   ├── Weapons/
│   │   │   ├── IronSword.asset
│   │   │   └── ...
│   │   ├── Armor/
│   │   └── Potions/
│   ├── AbilityData/
│   │   ├── CleaveAbility.asset
│   │   └── ...
│   ├── DifficultyScaling.asset      # Difficulty modifiers
│   └── GameConfig.asset             # Global game settings
├── Prefabs/
│   ├── Player.prefab
│   ├── Enemies/
│   │   ├── Slime.prefab
│   │   ├── Goblin.prefab
│   │   └── ...
│   ├── Items/
│   │   ├── Weapon.prefab
│   │   ├── Armor.prefab
│   │   └── Potion.prefab
│   ├── UI/
│   │   ├── DamageNumber.prefab
│   │   ├── StatusEffectIcon.prefab
│   │   └── ...
│   └── Effects/
│       ├── HitEffect.prefab
│       ├── ExplosionEffect.prefab
│       └── ...
├── Scenes/
│   ├── Bootstrap.unity       # Load game managers, don't-destroy-on-load
│   ├── MainMenu.unity        # Title, class select, meta-progression UI
│   ├── Gameplay.unity        # Dungeon floors (additive loading)
│   ├── LoadingScreen.unity   # Transition screen (optional)
│   └── GameOver.unity        # Death screen, stats display
├── Art/
│   ├── Sprites/
│   │   ├── Character/
│   │   │   ├── Player/
│   │   │   │   ├── WarriorIdle.png
│   │   │   │   ├── WarriorAttack.png
│   │   │   │   └── ...
│   │   │   └── Enemies/
│   │   │       ├── SlimeIdle.png
│   │   │       └── ...
│   │   ├── Environment/
│   │   │   ├── Wall.png
│   │   │   ├── Floor.png
│   │   │   └── ...
│   │   ├── Items/
│   │   │   ├── Sword.png
│   │   │   ├── Armor.png
│   │   │   └── Potion.png
│   │   └── UI/
│   │       ├── Button.png
│   │       ├── Panel.png
│   │       └── ...
│   ├── Tilesets/
│   │   └── DungeonTileset.png
│   └── Materials/
│       ├── Character.mat
│       └── Environment.mat
├── Audio/
│   ├── Music/
│   │   ├── MainMenuLoop.wav
│   │   ├── DungeonLoop.wav
│   │   └── BossLoop.wav
│   ├── SFX/
│   │   ├── Hit.wav
│   │   ├── Miss.wav
│   │   ├── Ability_Cleave.wav
│   │   ├── EnemyDeath.wav
│   │   └── ...
│   └── UI/
│       ├── ButtonClick.wav
│       └── MenuSelect.wav
├── Resources/
│   └── (Minimal use; prefer ScriptableObjects)
└── Config/
    └── GameSettings.json     # Runtime config (optional JSON loading)
```

### 3.2 Scene Hierarchy

#### Bootstrap Scene
- **Purpose**: Initialize persistent game managers on startup; ensure they load once
- **Content**:
  - `GameManager` (DontDestroyOnLoad)
  - `MetaProgressionManager` (DontDestroyOnLoad)
  - `EventBus` (DontDestroyOnLoad)
  - `AudioManager` (DontDestroyOnLoad)
- **Flow**: On startup, Bootstrap auto-loads, initializes managers, then loads MainMenu scene additively or via scene switch

#### MainMenu Scene
- **Purpose**: Title screen, class selection, meta-progression UI, save slot selection
- **Content**:
  - Canvas (UI root)
    - TitlePanel (title graphic, "Press Start" text)
    - ClassSelectPanel (three class buttons: Warrior, Rogue, Mage)
    - MetaProgressionPanel (essence display, upgrade tree)
    - ContinueButton (greyed if no active run)
    - SettingsButton
  - Button sounds on interaction
- **Flow**: Player selects class → loads Gameplay scene; or player clicks "Continue" → loads last save

#### Gameplay Scene (Additive)
- **Purpose**: The active dungeon floor; procedurally generated each time
- **Content**:
  - `TileGrid` (Grid Layout, instantiated each floor based on generation)
  - `Player` (prefab instance, placed by SpawnManager)
  - `EnemyContainer` (parent for all enemies spawned this floor)
  - `ItemContainer` (parent for all items spawned this floor)
  - `HUD` Canvas (health bar, level, inventory quick view, ability buttons)
  - `Camera` (follows player, orthographic)
  - `DungeonGenerator` script (generates layout and populates)
- **Flow**: On load, DungeonGenerator runs, creates floor layout, spawns player/enemies/items. Player interacts. On stair descent, scene remains but floor data reloads (or scene unloads and reloads for clean state)

#### GameOver Scene
- **Purpose**: Death screen, run summary, options to retry or return to menu
- **Content**:
  - Canvas (UI root)
    - DeathBanner ("Game Over")
    - StatsPanel
      - FloorsCleared (e.g., "Reached Floor 7")
      - EnemiesKilled (e.g., "Killed 23 enemies")
      - EssenceEarned (e.g., "Earned 150 Essence")
      - RunTime (e.g., "Time: 18 minutes")
    - RetryButton
    - MenuButton
  - Run data cached in GameManager before this scene loads
- **Flow**: On player death, load this scene; player clicks "Retry" (return to MainMenu) or "Menu" (return to MainMenu with stats saved)

#### Loading Screen Scene (Optional)
- **Purpose**: Brief transition when generating floor or loading save
- **Content**:
  - Canvas with spinning icon or progress bar
  - "Generating Floor 3..." text
- **Flow**: Show for 0.5–1.5 seconds while dungeon generates; auto-advance when ready

### 3.3 MonoBehaviour Architecture

#### `GameManager` (Singleton)
- **Responsibility**: Persistent game state, scene lifecycle, overall game flow
- **Key serialized fields**: None (or GameSettings asset reference)
- **Unity callbacks**: `Awake()` (singleton setup, DontDestroyOnLoad), `OnDestroy()`
- **Public API**:
  - `void StartNewRun(ClassType classType)`
  - `void SaveCurrentRun()`
  - `void LoadRun(int saveSlot)`
  - `void EndRun(RunEndReason reason)` — called on death or victory
  - `RunData GetCurrentRunData()` — returns active run state
- **Dependencies**: Requires access to SaveLoadManager, MetaProgressionManager

#### `PlayerController` (MonoBehaviour on Player prefab)
- **Responsibility**: Player input processing, movement, ability execution, interaction with environment
- **Key serialized fields**: 
  - `float movementSpeed`
  - `int maxInventorySlots`
  - `Animator playerAnimator`
- **Unity callbacks**: `Update()` (input polling), `OnCollisionEnter2D()` (item pickup collision)
- **Public API**:
  - `void Move(Vector2Int direction)`
  - `void AttackEnemy(EnemyBase target)`
  - `void CastAbility(int abilityIndex)`
  - `void UseItem(int inventorySlot)`
  - `void PickupItem(ItemBase item)`
  - `void DescendStairs()`
  - `bool TryEquip(ItemBase item)`
- **Dependencies**: PlayerStats, InventoryManager, Animator, Collider2D

#### `PlayerStats` (MonoBehaviour on Player prefab)
- **Responsibility**: Health, mana, level, XP, stat calculation
- **Key serialized fields**:
  - `int baseAttack`, `int baseDefense`, `int baseMaxHealth`, `int baseMaxMana`
  - `ClassData classData`
- **Unity callbacks**: `Awake()` (init from class data)
- **Public API**:
  - `void TakeDamage(int amount)`
  - `void Heal(int amount)`
  - `void RestoreMana(int amount)`
  - `void GainXP(int amount)` — triggers level up if threshold reached
  - `int CalculateOutgoingDamage(ItemBase weapon, AbilityData ability = null)`
  - `int CalculateIncomingDamage(int baseDamage)` — applies defense
  - `PropertyType GetFinalStat(StatType stat)` — attack, defense, HP, mana (includes equipment bonuses)
- **Dependencies**: InventoryManager (for equipment stat bonuses), EventBus (fires OnLevelUp, OnHealthChanged)

#### `InventoryManager` (MonoBehaviour on Player prefab)
- **Responsibility**: Item storage, equipment management, stat bonus aggregation
- **Key serialized fields**:
  - `int maxSlots = 10`
  - `EquipmentSlots currentEquipment` — struct with Head, Chest, Hands, Feet, Weapon slots
- **Unity callbacks**: `Awake()` (init inventory array)
- **Public API**:
  - `bool TryAddItem(ItemBase item)` — returns false if inventory full
  - `void RemoveItem(int slotIndex)`
  - `void EquipItem(ItemBase item)` — fails if slot already occupied or class incompatible
  - `void UnequipSlot(EquipmentSlotType slot)`
  - `ItemBase GetEquippedWeapon()`
  - `int GetStatBonus(StatType stat)` — sums all equipment bonuses for this stat
  - `void UsePotion(PotionType type)` — removes potion, applies effect
  - `ItemBase[] GetInventory()` — returns copy of inventory array
- **Dependencies**: ItemBase (for item queries)

#### `TurnManager` (Singleton, on DontDestroyOnLoad)
- **Responsibility**: Turn order, action queue, ensuring one action per entity per turn
- **Key serialized fields**: None
- **Unity callbacks**: `Update()` (processes turn queue)
- **Public API**:
  - `void RegisterEntity(EntityBase entity)` — add to turn queue
  - `void UnregisterEntity(EntityBase entity)` — remove from queue (on death)
  - `void RequestAction(EntityBase entity, ActionData action)` — queue action
  - `void EndTurn()` — move to next entity in queue
  - `int GetCurrentTurnNumber()` — for duration tracking
  - `EntityBase GetCurrentActor()` — who's turn is it
  - `event Action<EntityBase> OnTurnStarted` — fired when entity's turn begins
- **Dependencies**: EventBus (fires OnTurnStarted)

#### `EnemyAI` (MonoBehaviour on Enemy prefab)
- **Responsibility**: AI decision-making, pathfinding, state transitions
- **Key serialized fields**:
  - `EnemyData enemyData` (ScriptableObject reference)
  - `float sightRange = 8f`
  - `float attackRange = 1f`
- **Unity callbacks**: `Start()` (init from data), `Update()` (state logic, called once per frame but only acts on turn)
- **Public API**:
  - `ActionData DecideAction()` — returns next action (move, attack, ability, idle)
  - `void NotifyPlayerSeen(Transform player)`
  - `void OnDamageTaken(int amount)` — triggers retreat behavior if HP low
  - `Vector2Int GetPathToTarget(Vector2Int targetPos, int maxSteps)` — returns next step
- **Dependencies**: EnemyStats (health, AI properties), GridHelper (pathfinding), EventBus

#### `DungeonGenerator` (MonoBehaviour on DungeonGenerator object in Gameplay scene)
- **Responsibility**: Procedural generation of floor layout, room connectivity, enemy/item placement
- **Key serialized fields**:
  - `int floorNumber`
  - `int minRooms = 4`, `maxRooms = 8`
  - `int roomMinSize = 10`, `roomMaxSize = 15`
  - `Tilemap dungeonTilemap` — reference to Tilemap to paint
  - `int randomSeed` — if 0, use system random; else use for reproducibility
- **Unity callbacks**: `Start()` (generate floor, spawn entities)
- **Public API**:
  - `void GenerateFloor(int floorNumber, int seed = 0)` — creates layout
  - `void SpawnPlayer(PlayerController player)`
  - `void SpawnEnemies()` — uses EnemySpawner
  - `void SpawnItems()` — uses LootSystem
  - `Vector2Int FindStaircase()` — returns location of stairs for UI
  - `bool IsWalkable(Vector2Int pos)` — check if tile is navigable
  - `List<Vector2Int> GetLineOfSight(Vector2Int origin, float range)` — FOV check
- **Dependencies**: Tilemap, Random (seeded), EnemySpawner, LootSystem

#### `CombatResolver` (Static utility or Singleton)
- **Responsibility**: Centralized damage calculation, hit/miss rolls, effect application
- **Key serialized fields**: None (all logic)
- **Unity callbacks**: None
- **Public API**:
  - `CombatResult CalculateDamage(EntityBase attacker, EntityBase defender, DamageType damageType, int baseDamage)` — returns damage amount, hit/miss, crit
  - `void ApplyDamage(EntityBase target, int amount)`
  - `void ApplyStatusEffect(EntityBase target, StatusEffect effect)`
  - `void RemoveStatusEffect(EntityBase target, StatusEffectType type)`
  - `List<StatusEffect> GetActiveEffects(EntityBase entity)`
  - `int RollHitChance(int attackerAccuracy, int defenderEvasion)` — returns 0-100, >50 is hit
- **Dependencies**: None (pure logic)

#### `MetaProgressionManager` (Singleton, on DontDestroyOnLoad)
- **Responsibility**: Essence tracking, upgrade tree persistence, serialization
- **Key serialized fields**: `string savePath = "Resources/MetaProgressionSave.json"`
- **Unity callbacks**: `Awake()` (load from disk), `OnApplicationQuit()` (save to disk)
- **Public API**:
  - `void AddEssence(int amount)`
  - `int GetEssence()`
  - `bool TryPurchaseUpgrade(ClassType class, UpgradeID upgradeID)` — deducts essence if affordable
  - `bool IsUpgradeUnlocked(ClassType class, UpgradeID upgradeID)`
  - `List<UpgradeData> GetUpgradeTree(ClassType class)` — returns full tree
  - `void SaveToDisk()`
  - `void LoadFromDisk()`
- **Dependencies**: Serialization library (Newtonsoft.Json or custom)

#### `StatusEffectSystem` (Singleton)
- **Responsibility**: Manage active status effects, trigger damage/effects each turn
- **Key serialized fields**: None
- **Unity callbacks**: `OnEnable()` (subscribe to TurnManager.OnTurnStarted)
- **Public API**:
  - `void ApplyEffect(EntityBase target, StatusEffect effect)`
  - `void RemoveEffect(EntityBase target, StatusEffectType type)`
  - `void ResolveTurnEffects(EntityBase entity)` — called at turn start; applies damage, stuns, etc.
  - `List<StatusEffect> GetEffects(EntityBase entity)`
- **Dependencies**: TurnManager, CombatResolver

#### `SaveLoadManager` (Singleton)
- **Responsibility**: Serialize/deserialize run data to disk
- **Key serialized fields**: `string runSaveFile = "CurrentRun.save"`
- **Unity callbacks**: None (static methods)
- **Public API**:
  - `void SaveRun(RunData data)`
  - `RunData LoadRun(int slotIndex)`
  - `bool HasSavedRun(int slotIndex)`
  - `void DeleteRun(int slotIndex)`
  - `List<RunSummary> GetRunHistory()` — for stats/leaderboard
- **Dependencies**: Serialization library

### 3.4 ScriptableObject Data Design

#### `ClassData` (ScriptableObject)
- **Fields**:
  ```csharp
  public string className; // "Warrior", "Rogue", "Mage"
  public int baseAttack, baseDefense, baseMaxHealth, baseMaxMana;
  public Sprite classIcon;
  public AbilityData[] startingAbilities;
  public ItemBase startingWeapon;
  public ItemBase[] startingArmor; // head, chest, hands, feet
  ```
- **Purpose**: Each class has different starting stats and abilities; SO makes it designer-tunable
- **Instances**: 3 (Warrior, Rogue, Mage)

#### `EnemyData` (ScriptableObject)
- **Fields**:
  ```csharp
  public string enemyName;
  public int baseHealth, baseAttack, baseDefense;
  public float sightRange, attackRange;
  public AIBehaviorType behavior; // Chase, Idle, Retreat
  public AbilityData[] abilities;
  public float essenceDropMin, essenceDropMax;
  public Sprite sprite;
  public AnimationClip[] animations;
  ```
- **Purpose**: Enemy stats and behavior easily tunable without code; used by EnemyAI
- **Instances**: 8–10 (one per enemy type: Slime, Goblin, Skeleton, etc.)

#### `AbilityData` (ScriptableObject)
- **Fields**:
  ```csharp
  public string abilityName; // "Cleave", "Fireball", "Backstab"
  public int manaCost, cooldownTurns;
  public float range, areaOfEffect;
  public int baseDamage;
  public DamageType damageType; // Melee, Magic, Physical
  public StatusEffect[] statusEffectsApplied;
  public AnimationClip castAnimation;
  public AudioClip sfx;
  public Sprite icon;
  ```
- **Purpose**: Ability definitions for both player and enemies; balance tuning
- **Instances**: 12–15 (multiple per class, plus enemy abilities)

#### `ItemData` (ScriptableObject, parent class)
- Variants: `WeaponData`, `ArmorData`, `PotionData`
- **WeaponData fields**:
  ```csharp
  public string itemName;
  public Sprite icon;
  public int attackBonus, defenseBonus;
  public AbilityData linkedAbility; // e.g., sword grants a cleave ability
  public ItemRarity rarity; // Common, Uncommon, Rare, Legendary
  public string description;
  ```
- **ArmorData fields**:
  ```csharp
  public EquipmentSlotType slotType; // Head, Chest, etc.
  public int defenseBonus, healthBonus;
  public ItemRarity rarity;
  ```
- **PotionData fields**:
  ```csharp
  public PotionType type; // Healing, Mana, DamageBoost
  public int potencyAmount;
  public ItemRarity rarity;
  ```
- **Purpose**: Items are data-driven; designers add variety without coding
- **Instances**: 20–30 total (weapons, armor, potions)

#### `DifficultyScaling` (ScriptableObject)
- **Fields**:
  ```csharp
  public float enemyHPMultiplier; // 1.0 = normal, 1.5 = hard
  public float enemyDamageMultiplier;
  public float essenceDropMultiplier;
  public float lootRarityBoost; // increase chance of rare drops
  ```
- **Purpose**: Quick tuning of overall difficulty without touching enemy data
- **Instances**: 3 (Normal, Hard, Legendary)

#### `GameConfig` (ScriptableObject)
- **Fields**:
  ```csharp
  public int totalFloors = 20;
  public int[] bossFloors = { 5, 10, 15, 20 }; // which floors have bosses
  public int minRoomsPerFloor = 4, maxRoomsPerFloor = 8;
  public float turnDuration = 0.5f; // how long each action animates
  public bool enableAutoSave = true;
  ```
- **Purpose**: Global game tuning in one place
- **Instances**: 1 (singleton)

### 3.5 Input System

**System**: New Input System package (InputAction assets)

**Input Action Map**:
```
Gameplay
├── Movement
│   ├── Up (W, ArrowUp)
│   ├── Down (S, ArrowDown)
│   ├── Left (A, ArrowLeft)
│   └── Right (D, ArrowRight)
├── Actions
│   ├── Attack (Space, RMB)
│   ├── Ability_1 (E)
│   ├── Ability_2 (R)
│   ├── Ability_3 (Q)
│   ├── UseItem (X)
│   ├── DescendStairs (Space)
│   └── OpenInventory (I)
└── UI
    ├── OpenMenu (Esc)
    ├── Confirm (Enter)
    └── Cancel (Esc)

Menu
└── Navigate (Arrow keys, WASD)
    ├── Confirm (Enter, Space)
    └── Cancel (Esc)
```

**Control Schemes**: 
- ⚠️ **Assumption**: Keyboard+Mouse only (no gamepad support in MVP); gamepad can be added post-launch
- Mouse clicks on enemies/items for interaction (alternative to keyboard)
- UI navigation via arrow keys

**Rebinding**: Not in MVP; can be added in meta-progression settings

### 3.6 Physics & Collision

**2D Physics**: Using Unity's 2D physics system

**Layers**:
- Player (Layer 8)
- Enemies (Layer 9)
- Items (Layer 10)
- Walls/Terrain (Layer 11)
- Projectiles (Layer 12, for future spells)

**Collision Matrix**:
- Player collides with: Walls, Enemies (for bump detection, not damage)
- Enemies collide with: Walls, Player, other Enemies (pushback)
- Items collide with: Player (pickup on touch)
- Projectiles collide with: Walls, Enemies

**Rigidbody Setup**:
- Player: Kinematic (controlled by script, not physics)
- Enemies: Kinematic (AI moves them, not physics)
- Items: Static (no physics interaction)
- Walls: Static

**Raycasting**: Used for line-of-sight checks (enemy sight range), pathfinding validation

### 3.7 Rendering & Visual Effects

**Render Pipeline**: Built-in (Unity Standard Pipeline) — lightweight, suitable for pixel art 2D

**Camera**:
- Orthographic projection (2D isometric view)
- Pixel-perfect rendering (use `PixelPerfectCamera` component)
- Camera follows player with slight smoothing (Cinemachine not needed for simplicity, but can add)
- FOV: Covers 10x10 tiles at 1280x720 resolution

**Lighting**: 
- Minimal lighting (pixel art doesn't require realistic lights)
- Optional: Layer-based fog of war (darker tiles for unexplored rooms, brightens when visited)

**Particle Systems**:
- Hit effect (small spark burst on damage)
- Heal effect (glow particles on healing)
- Ability effects (Fireball AoE, Cleave slash animation)
- Potion pickup shimmer

**Shaders**: 
- Standard sprite shader (no custom shaders needed for MVP)
- Possible future: simple palette swap for damage flashes or status effect tinting

**Post-Processing**: None in MVP

### 3.8 Audio Architecture

**System**: AudioManager singleton (simple, no FMOD/Wwise for MVP)

**Audio Assets**:
- **Music**:
  - MainMenuLoop (60 seconds, loops)
  - DungeonLoop (120 seconds, loops; can add variation per floor later)
  - BossLoop (90 seconds, loops; more intense)
- **SFX**:
  - Hit (melee attack sound)
  - Miss (whiff sound)
  - Ability cast (distinct sound per ability or generic)
  - EnemyDeath (splat/explosion)
  - PlayerHurt (player take damage)
  - ItemPickup (collect sound)
  - LevelUp (fanfare, short)
  - StairDescend (transition sound)
- **UI**:
  - ButtonClick (menu navigation)
  - MenuSelect (class selection click)

**AudioManager Implementation**:
- Singleton with DontDestroyOnLoad
- Public methods: `PlaySFX(AudioClip clip, float volume)`, `PlayMusic(AudioClip clip, bool loop)`
- SFX uses simple AudioSource pooling (5–10 sources, reuse when done)
- Music crossfades between tracks (fade out old, fade in new over 0.5s)

**Mixer Groups** (optional for volume control):
- Master
- Music
- SFX
- UI

### 3.9 Save / Load System

**Storage Approach**: JSON file (using Newtonsoft.Json or SimpleJSON)

**What Gets Saved**:
- **Run Data**:
  - Player class, level, HP, mana, XP
  - Current floor number
  - Inventory (item IDs and counts)
  - Equipment (slot assignments)
  - Active status effects
  - Dungeon layout (floor seed, room layout, enemy positions, items)
  - Time elapsed
  - Essence earned this run
- **Meta-Progression Data**:
  - Total Essence collected (lifetime)
  - Unlocked upgrades per class
  - Run history (timestamp, floors reached, essence earned)

**Save Locations**:
- Run data: `%APPDATA%/LocalLow/[CompanyName]/DungeonCrawler/CurrentRun.json` (or `~/.config/` on Linux)
- Meta-progression: `%APPDATA%/LocalLow/[CompanyName]/DungeonCrawler/MetaProgression.json`
- Run history: `%APPDATA%/LocalLow/[CompanyName]/DungeonCrawler/RunHistory.json`

**Save Slots**: 
- ⚠️ **Assumption**: Only one active run slot (no multiple simultaneous saves); simplifies design. Can expand to 3 slots in post-MVP update.

**Auto-Save**: On each floor transition or every 2 minutes during gameplay (if gameplay turn exists)

**Cloud Save**: Not in MVP; can integrate later with Steam Cloud or custom backend

### 3.10 Performance Budget

**Target Framerate**: 60 FPS on minimum spec

**Memory Budget**:
- Target: <500 MB RAM in-game (excluding OS)
- Typical allocation: 100 MB assets, 200 MB game state, 200 MB reserve

**Draw Calls**:
- Target: <50 draw calls per frame (tile batch, sprites batched)
- Use sprite atlas for all enemies, items, UI
- Enable GPU instancing on materials

**Object Pooling**:
- Damage number prefabs (20 pool size)
- Particle effects (10 pool size)
- Enemies (spawn/despawn per floor, no persistent pooling needed initially)

**Level-of-Detail**: Not needed (2D pixel art has no LOD)

**Culling**: Tile-based culling (don't render tiles outside camera bounds)

**Loading Strategy**:
- Scenes use additive loading for smooth transitions
- DungeonGenerator runs on-demand (takes <1 second for floor generation on modern hardware)

---

## 4. Milestones & Scope

### 4.1 MVP Definition

The absolute minimum to have a playable, fun game loop:

1. **Turn-based combat system** — Player can move, attack enemies, take damage, die
2. **Procedurally generated dungeon floor** — At least one room type, random enemy spawning
3. **Class selection** — Warrior, Rogue, Mage with distinct abilities
4. **Inventory and equipment** — Pick up items, equip weapons, stat changes reflected in damage
5. **Enemy AI** — Enemies chase and attack player, pathfinding works
6. **Permadeath and game-over screen** — Death ends run, shows stats
7. **Floor progression** — Stairs lead to next floor, counter increments
8. **Basic meta-progression** — Essence earned and spent on simple upgrades
9. **UI essentials** — Health/mana bars, ability buttons, inventory quick-view
10. **Audio** — Basic SFX for hits, abilities, death; simple looping music

**Out of MVP**:
- Boss encounters (can be first skill to add post-MVP)
- Status effects (Stun, Poison, etc.) — simplified or removed for MVP
- Difficult scaling or Hard mode
- Run history leaderboard
- Advanced item rarity tiers (Rare/Legendary — use just Common/Uncommon)

### 4.2 Development Phases

#### Phase 1: Prototype (Weeks 1–2)
**Goal**: Prove the core loop is fun and functional

**In**:
- Grid-based movement
- Basic attack (click enemy, roll damage)
- One room, spawn 3 enemies manually
- Player health and death detection
- Game-over screen

**Out**: Procedural generation, inventory, abilities, meta-progression

**Success criteria**: Player can move, attack enemy, enemy can attack back, combat feels responsive

---

#### Phase 2: Alpha (Weeks 3–6)
**Goal**: All core systems in place, fully playable (even if rough art/balance)

**In**:
- Procedural dungeon generation (4–6 rooms per floor)
- 3 classes with 2 abilities each
- Full inventory and equipment system
- Loot dropping (common rarity only)
- Floor progression to Floor 5
- Meta-progression: Essence tracking, basic upgrade tree
- HUD with health, level, inventory quick-access
- Audio: Music and basic SFX
- Enemy types: Basic, Ranged (placeholder sprites)

**Out**: Boss encounters, status effects, visual effects, advanced tuning

**Success criteria**: Can play through 5 floors, economy (essence/loot) feels balanced, core loop repeatable

---

#### Phase 3: Beta (Weeks 7–10)
**Goal**: Content complete, polish and balance

**In**:
- All 20 floors with increasing difficulty
- Boss encounters (Floors 5, 10, 15)
- All enemy types (Slime, Goblin, Skeleton, Orc, Elemental, Boss variants)
- Status effects (Stun, Poison, Bleed, Slow, Weakness)
- Full visual effects (particles, animations, hit feedback)
- Rare and Legendary item rarity tiers, loot tables balanced
- Advanced meta-progression: Full upgrade tree (15–20 upgrades per class)
- UI polish: Better fonts, layout refinement, hover tooltips
- Balancing pass on enemy stats, ability cooldowns, loot rates
- Save/load fully tested
- Bug fixes from Alpha feedback

**Out**: Advanced features (leaderboards, Steam achievements), platform optimization

**Success criteria**: Game is fully playable, win condition clear, no game-breaking bugs, balance feels fair

---

#### Phase 4: Release (Weeks 11–12)
**Goal**: Final polish, platform-specific build, launch preparation

**In**:
- Final balance pass based on playtesting
- Performance optimization (draw call reduction, memory profiling)
- UI animation polish
- Pause menu with quit/resume/settings
- Game icon and store assets (if publishing on Steam/itch)
- Trailer or GIF for marketing
- Build testing on Windows/Mac/Linux

**Out**: Post-launch features (future updates, DLC)

**Success criteria**: Game builds for all platforms, performance targets met, ready to ship

### 4.3 Risk Assessment

| Risk | Impact | Mitigation |
|------|--------|-----------|
| **Procedural generation creates boring/broken layouts** | High | Prototype generation early (Phase 1). Seed test with many runs. Use constraint-based generation (ensure path to stairs exists, connectivity validated). |
| **Combat feels sluggish/unresponsive** | High | Implement snappy input feedback immediately. Test with player feedback mid-Alpha. Iterate on turn timing and animation speed. |
| **Meta-progression economy breaks (essence too easy/hard to earn)** | Medium | Playtest and track essence rates. Adjust drop rates per floor based on target progression (expect 2–4 runs to unlock one upgrade). |
| **Art bottleneck (too much pixel art to create)** | Medium | Use tileset and sprite atlas; procedural enemies use palette swaps. Keep enemy count low (8 types). Placeholder art acceptable in Alpha; add final polish in Beta. |
| **Difficulty scaling too hard (boss at floor 5 is unbeatable)** | Medium | Adjust boss HP/damage curves based on playtesting. Ensure meta-progression unlocks provide meaningful power growth (10% per upgrade). |
| **Save/load data corruption** | Low | Use robust JSON serialization, test save/load cycle early and often. Manual backup of save file on new run. |

---

## 5. SDD Handoff

This section breaks down every feature and system into a format ready for development iteration. The SDD (Software Design Document) skill will consume this and generate actionable tasks.

### 5.1 Feature Breakdown

#### Feature: **Main Menu & Class Selection**
- **Description**: Player launches game, sees title screen, selects class (Warrior/Rogue/Mage) to start a run
- **Inputs**: 
  - Game launch
  - Player mouse click on class button or keyboard selection
- **Outputs**:
  - Selected class persists to Gameplay scene
  - Gameplay scene loads with player of chosen class
  - Class-specific UI (portrait, stats preview) displayed during selection
- **Dependencies**: PlayerStats system, ClassData ScriptableObjects
- **Acceptance criteria**:
  - [ ] Title screen displays with three class buttons
  - [ ] Class selection input (click/keyboard) registers
  - [ ] Selected class stat sheet shows correctly (attack, defense, HP, mana)
  - [ ] Clicking "Start" transitions to Gameplay with correct class instantiated
  - [ ] Esc returns to title if pressed before Start
- **Priority**: MVP
- **Estimated complexity**: Low

---

#### Feature: **Player Movement**
- **Description**: Player moves on grid one tile per turn in cardinal directions (up/down/left/right), collides with walls and enemies, animation updates smoothly
- **Inputs**:
  - Arrow keys or WASD
  - One press = one tile move = ends turn
- **Outputs**:
  - Player position updates
  - Camera follows player
  - Player sprite faces correct direction
  - Turn marker advances to next entity
- **Dependencies**: TurnManager, PlayerController, grid collision checks
- **Acceptance criteria**:
  - [ ] Arrow keys move player one tile per turn
  - [ ] Player cannot pass through walls (collision detected)
  - [ ] Player cannot pass through enemies (stopped by NPC)
  - [ ] Movement sprite animates smoothly
  - [ ] Camera tracks player without jarring jumps
  - [ ] Edge of map prevents movement (e.g., can't walk off screen)
- **Priority**: MVP
- **Estimated complexity**: Low

---

#### Feature: **Basic Attack**
- **Description**: Player targets adjacent/nearby enemy and deals damage based on attack stat, weapon, and roll. Enemy health decreases, hit feedback shown
- **Inputs**:
  - Click enemy or press Space while adjacent, or hotkey while targeting
- **Outputs**:
  - Damage calculated (base attack + weapon bonus - enemy defense ± variance)
  - Hit/miss roll determined
  - Enemy health updated
  - Damage number floats on screen
  - Hit/miss sound plays
  - Turn advances to next entity
- **Dependencies**: CombatResolver, PlayerStats, InventoryManager, PlayerAnimator
- **Acceptance criteria**:
  - [ ] Attack initiates when player adjacent to enemy and input given
  - [ ] Damage formula applies (attack - defense)
  - [ ] Damage variance ±10% works (test 100 hits, confirm range)
  - [ ] Hit chance roll applied (varies by accuracy, not always 100% hits)
  - [ ] Damage number appears above target, fades after 1 second
  - [ ] Enemy HP decreases by correct amount
  - [ ] If enemy HP <= 0, enemy dies and is removed
  - [ ] Hit sound plays (distinct from miss)
  - [ ] Attack animation plays on player
- **Priority**: MVP
- **Estimated complexity**: Medium

---

#### Feature: **Class-Specific Abilities**
- **Description**: Each class has 2–3 unique abilities (e.g., Warrior Cleave, Rogue Backstab, Mage Fireball) with mana cost, cooldown, and special effects (AoE, status effects)
- **Inputs**:
  - Hotkey (E, R, Q) or ability button UI
- **Outputs**:
  - Ability executes if mana available and cooldown expired
  - Mana depleted
  - Cooldown timer starts
  - Special effect resolved (damage, AoE, status effect)
  - Ability animation plays
  - Turn ends
- **Dependencies**: PlayerStats (mana), PlayerAbilities system, CombatResolver, StatusEffectSystem, Animator
- **Acceptance criteria**:
  - [ ] All three classes have at least 1 ability implemented
  - [ ] Ability requires mana cost; cannot cast without sufficient mana
  - [ ] Cooldown enforced; cannot spam ability
  - [ ] AoE abilities affect all enemies in radius
  - [ ] Damage applies correctly to targets
  - [ ] Cooldown duration decrements each turn
  - [ ] Ability animation and SFX play
  - [ ] Mana bar updates after cast
  - [ ] Cooldown indicator shows remaining turns on UI
- **Priority**: MVP
- **Estimated complexity**: High

---

#### Feature: **Turn-Based Combat System**
- **Description**: Combat proceeds in turns: player acts, then each enemy acts in sequence. One action per entity per turn. Turn order managed centrally.
- **Inputs**:
  - Player action (move, attack, ability, item, skip turn)
  - Enemy AI decision
- **Outputs**:
  - Action resolves immediately
  - Turn counter increments
  - Next entity's turn begins
  - Status effects and cooldowns decrement
- **Dependencies**: TurnManager, CombatResolver, EnemyAI
- **Acceptance criteria**:
  - [ ] Player action ends turn (moves to first enemy)
  - [ ] Each enemy attacks/moves in sequence after player
  - [ ] Damage resolves instantly (no queuing delays)
  - [ ] Turn counter visible and increments per cycle
  - [ ] Status effect durations decrement each turn
  - [ ] Ability cooldowns decrement each turn
  - [ ] If player dies mid-turn, game over triggers before enemies act further
- **Priority**: MVP
- **Estimated complexity**: High

---

#### Feature: **Enemy Spawning & Difficulty**
- **Description**: Enemies spawn at room generation based on floor number. Difficulty increases with floor (higher HP, damage, more enemies per room)
- **Inputs**:
  - Floor number (from DungeonGenerator)
- **Outputs**:
  - Enemy prefabs instantiated at random valid spawns in room
  - Enemy stats scaled by floor
  - Enemy count: 2–5 per room (increases per floor)
- **Dependencies**: DungeonGenerator, EnemyBase, EnemyData ScriptableObjects
- **Acceptance criteria**:
  - [ ] Enemies spawn at start of floor (not on top of player)
  - [ ] Enemy type varies per floor (Slime on 1–3, Goblin on 2–5, etc.)
  - [ ] Enemy count increases: Floor 1 = 2–3, Floor 10 = 4–5
  - [ ] Enemy stats scale: `base_stat * (1 + 0.1 * floor_number)`
  - [ ] Enemies do not spawn off-map or in walls
- **Priority**: MVP
- **Estimated complexity**: Low

---

#### Feature: **Enemy AI & Pathfinding**
- **Description**: Enemies detect player, chase when in sight range, attack when adjacent. Pathfind around obstacles to reach player.
- **Inputs**:
  - Player position (visible to enemy if in sight range)
  - Dungeon layout
- **Outputs**:
  - Enemy moves toward player (or takes other action)
  - Enemy attacks player if adjacent and has action
  - Pathfinding avoids walls
- **Dependencies**: EnemyAI, GridHelper (pathfinding), DungeonGenerator (layout data)
- **Acceptance criteria**:
  - [ ] Enemy chases player when player in sight range (8 tiles)
  - [ ] Enemy does not see through walls (line of sight checked)
  - [ ] Pathfinding finds path around obstacles
  - [ ] Enemy moves toward player each turn
  - [ ] Enemy attacks when adjacent (range 1, or melee type)
  - [ ] Ranged enemies attack from distance (range 3, or ranged type)
  - [ ] Enemy stops chasing if player out of sight
  - [ ] No infinite loops in pathfinding (max steps enforced)
- **Priority**: MVP
- **Estimated complexity**: High

---

#### Feature: **Procedural Dungeon Generation**
- **Description**: Each floor is randomly generated with different room layouts, enemy positions, and item placements. Layout is connected and playable (path to stairs guaranteed).
- **Inputs**:
  - Floor number
  - Random seed (for reproducibility)
- **Outputs**:
  - Floor layout (rooms, walls, hallways, stairs)
  - Enemy positions per room
  - Item positions per room
  - Walkable tile data
- **Dependencies**: DungeonGenerator, Random (seeded)
- **Acceptance criteria**:
  - [ ] Floor generates in <1 second on modern hardware
  - [ ] 4–8 rooms per floor (varies)
  - [ ] All rooms connected (no isolated areas)
  - [ ] Path from start to stairs always exists
  - [ ] Stairs spawn in last room (or central location)
  - [ ] Same seed produces same layout (reproducible)
  - [ ] Tilemap displays correctly (walls and floors visible)
  - [ ] Playable boundaries enforced (can't go off-map)
- **Priority**: MVP
- **Estimated complexity**: High

---

#### Feature: **Item Pickup & Inventory**
- **Description**: Player walks over item or clicks "Loot" to add to inventory (max 10 slots). Inventory grid shows items, rarity colors. Equipment can be managed (equip/unequip).
- **Inputs**:
  - Walk over item or click "Pick up" / "Loot"
  - Open inventory (I key)
  - Drag/drop or "Equip" button on item
- **Outputs**:
  - Item moves to inventory
  - If inventory full, show "Full" prompt
  - Equipment slot updates visually
  - Stats recalculate when equipped
  - UI inventory grid updates
- **Dependencies**: InventoryManager, PlayerStats, ItemBase
- **Acceptance criteria**:
  - [ ] Walking over item displays "Loot" prompt
  - [ ] Clicking or pressing hotkey adds item to inventory
  - [ ] Inventory holds max 10 items (stackable for potions)
  - [ ] If full, cannot pick up (show warning message)
  - [ ] Inventory UI opens and shows all items with icons
  - [ ] Equipment slots show currently equipped items
  - [ ] Clicking "Equip" swaps current equipment with new item
  - [ ] Stats update immediately when equipment changes
  - [ ] Cannot unequip if it's only weapon (prevented in UI)
  - [ ] Item details show: name, rarity, bonuses
- **Priority**: MVP
- **Estimated complexity**: Medium

---

#### Feature: **Loot Drops & Rarity System**
- **Description**: Enemies drop items on death (60% chance). Rarity tiers (Common, Uncommon, Rare, Legendary) affect stats and visual appearance (color-coded UI)
- **Inputs**:
  - Enemy defeated
- **Outputs**:
  - Item generated (random type, rarity based on drop table)
  - Item spawns at enemy position
  - UI shows item name in corresponding rarity color
- **Dependencies**: LootSystem, ItemData ScriptableObjects
- **Acceptance criteria**:
  - [ ] ~60% of defeated enemies drop loot
  - [ ] Loot rarity distribution: 50% Common, 30% Uncommon, 15% Rare, 5% Legendary
  - [ ] Legendary items have ≥50% higher stats than Common
  - [ ] Item type matches enemy (appropriate difficulty)
  - [ ] Item icon appears at enemy death location
  - [ ] Item rarity color displayed in inventory (Gray=Common, Green=Uncommon, Blue=Rare, Gold=Legendary)
  - [ ] Floor-based scaling: Higher floors drop higher rarity (Legendary chance increases by 2% per floor)
- **Priority**: MVP
- **Estimated complexity**: Medium

---

#### Feature: **Player Health & Damage**
- **Description**: Player has health that decreases when damaged. Health bar displays. Death triggers game-over screen.
- **Inputs**:
  - Enemy attack or damage effect
- **Outputs**:
  - Health decreases
  - Health bar animates/updates
  - At HP <= 0, player dies, game over
- **Dependencies**: PlayerStats, CombatResolver, Health bar UI
- **Acceptance criteria**:
  - [ ] Player has HP and MaxHP based on class
  - [ ] Taking damage reduces HP by correct amount
  - [ ] Health bar displays and updates smoothly
  - [ ] HP cannot exceed MaxHP (healing caps at max)
  - [ ] When HP <= 0, player is removed and game over screen shown
  - [ ] Game over screen shows floors cleared, enemies killed, essence earned
- **Priority**: MVP
- **Estimated complexity**: Low

---

#### Feature: **Floor Progression & Stairs**
- **Description**: Stairs are placed at end of each floor. Player walks onto stairs to advance to next floor. Floor counter increments. New floor generates automatically.
- **Inputs**:
  - Player walks onto stair tile
  - Or "Go Down" button when on stairs
- **Outputs**:
  - Current floor number increments
  - Next floor generates
  - Player position resets (near stairs of new floor)
  - Enemy and item lists refreshed
  - HUD shows new floor number
- **Dependencies**: DungeonGenerator, Floor progression state
- **Acceptance criteria**:
  - [ ] Stairs visible on tilemap at end of each floor
  - [ ] Walking onto stairs triggers floor transition
  - [ ] Transition screen briefly shows "Descending..." or similar
  - [ ] Next floor generates without errors
  - [ ] Floor counter visible in HUD increments
  - [ ] Player position on new floor is valid (not in wall)
  - [ ] All enemies/items from previous floor cleared
  - [ ] Can progress through at least 5 floors without crashing
- **Priority**: MVP
- **Estimated complexity**: Low

---

#### Feature: **Permadeath & Game Over**
- **Description**: When player dies, run ends permanently. Game over screen shows stats and options to restart or return to menu.
- **Inputs**:
  - Player HP drops to 0
- **Outputs**:
  - Game pauses
  - Game over screen loads
  - Run stats displayed (floors cleared, enemies killed, essence earned, time elapsed)
  - "Try Again" or "Menu" buttons available
- **Dependencies**: GameManager, GameOver scene, run stats tracking
- **Acceptance criteria**:
  - [ ] Game pauses immediately when player dies
  - [ ] Game over screen displays within 1 second
  - [ ] Floors cleared shows accurate count
  - [ ] Enemies killed count accurate
  - [ ] Essence earned this run displayed correctly
  - [ ] Time elapsed shows in MM:SS format
  - [ ] "Try Again" button returns to main menu (new run)
  - [ ] "Menu" button returns to main menu
- **Priority**: MVP
- **Estimated complexity**: Low

---

#### Feature: **Meta-Progression & Essence System**
- **Description**: Essence currency earned during runs persists between deaths. Spent on permanent upgrades to classes. UI shows Essence balance and upgrade tree.
- **Inputs**:
  - Enemies defeated (earn essence)
  - Upgrade tree menu opened
  - Player clicks upgrade to purchase
- **Outputs**:
  - Essence balance updates
  - Unlocked upgrades persist (saved to disk)
  - Next run starts with unlocked bonuses active
  - Upgrade tree UI shows purchased upgrades (checkmark) vs. locked
- **Dependencies**: MetaProgressionManager, SaveLoadManager, UpgradeTreeUI
- **Acceptance criteria**:
  - [ ] Enemies drop 1–5 Essence on defeat (scales with floor)
  - [ ] Essence balance persists after death (shown in main menu)
  - [ ] Upgrade tree UI displays all upgrades for selected class
  - [ ] Locked upgrades show cost and requirements
  - [ ] Player can purchase upgrade if Essence >= cost
  - [ ] After purchase, Essence deducted and upgrade locked in
  - [ ] Unlocked upgrade applied to next run of that class
  - [ ] At least 3–5 upgrades per class in MVP (can expand later)
  - [ ] Essence saved to disk and reloads on game restart
- **Priority**: MVP
- **Estimated complexity**: Medium

---

#### Feature: **HUD & UI Display**
- **Description**: In-game HUD shows player health, mana, level, floor number, ability cooldowns, and mini action buttons. Clear, readable, pixel-perfect.
- **Inputs**:
  - Game state changes (health, level, cooldown)
  - Player input (ability button)
- **Outputs**:
  - HUD updates in real-time
  - Ability buttons show cooldown progress
  - Status information always visible
- **Dependencies**: PlayerStats, UI Canvas, EventBus (for state change events)
- **Acceptance criteria**:
  - [ ] Health bar shows current/max HP with text
  - [ ] Mana bar shows current/max mana
  - [ ] Level display shows current level and XP bar
  - [ ] Floor counter shows current floor number
  - [ ] Ability buttons (E, R, Q) visible with cooldown overlay if active
  - [ ] Ability button disables/greys if insufficient mana
  - [ ] Status effects show as icons above player
  - [ ] All text is readable at 1280x720 (pixel-perfect)
  - [ ] HUD responds to window resize without distortion
- **Priority**: MVP
- **Estimated complexity**: Medium

---

#### Feature: **Audio System**
- **Description**: Background music loops during gameplay. SFX play for hits, abilities, item pickups, deaths, level-ups. Audio manager handles mixing and playback.
- **Inputs**:
  - Combat actions (attack, ability, damage taken)
  - Item pickup
  - Level up event
  - Scene transitions
- **Outputs**:
  - Music loops seamlessly
  - SFX plays with appropriate volume and timing
  - Audio stops/changes on scene transition
- **Dependencies**: AudioManager singleton, AudioClips, AudioSource
- **Acceptance criteria**:
  - [ ] Main menu music loops without gaps
  - [ ] Dungeon music loops seamlessly during gameplay
  - [ ] Hit sound plays when player attacks
  - [ ] Miss sound plays on failed attack
  - [ ] Ability SFX plays per ability (distinct or generic)
  - [ ] Enemy death sound plays on kill
  - [ ] Item pickup sound plays on loot
  - [ ] Level up fanfare plays on level gain
  - [ ] Volume levels are balanced (music not too loud vs. SFX)
  - [ ] No audio crackling or distortion at normal volumes
- **Priority**: MVP
- **Estimated complexity**: Low

---

#### Feature: **Pause Menu & Settings**
- **Description**: Player can pause game (Esc key), see pause menu with options to resume, quit to menu, or adjust settings (volume sliders). Pause freezes all game time.
- **Inputs**:
  - Esc key during gameplay
  - Menu button clicks (Resume, Quit, Settings)
  - Volume slider adjustment
- **Outputs**:
  - Game pauses (TurnManager halts, timers freeze)
  - Pause menu UI overlays screen
  - Volume settings persist across sessions
- **Dependencies**: MenuManager, PlayerPrefs or SaveLoadManager, TurnManager
- **Acceptance criteria**:
  - [ ] Pressing Esc opens pause menu
  - [ ] Pressing Esc again or "Resume" button unpauses
  - [ ] All timers and animations freeze while paused
  - [ ] "Quit to Menu" button returns to main menu
  - [ ] Volume sliders adjust Master, Music, and SFX volumes
  - [ ] Volume settings saved and restored on startup
  - [ ] Pause menu is not obstructed by game UI
  - [ ] Cannot unpause if game over (no resume option on game over screen)
- **Priority**: MVP
- **Estimated complexity**: Low

---

#### Feature: **Save & Load System**
- **Description**: Game state auto-saves on floor transition. Player can resume interrupted run from main menu "Continue" button.
- **Inputs**:
  - Floor transition
  - Game exit (graceful shutdown)
  - Main menu "Continue" button clicked
- **Outputs**:
  - Current run serialized to JSON file
  - On resume, run state reloaded and gameplay continues
  - Save slot shows "Floor X" progress indicator
- **Dependencies**: SaveLoadManager, GameManager, JSON serialization
- **Acceptance criteria**:
  - [ ] Game saves on each floor transition (no crashes = no lost runs)
  - [ ] Save file contains all run state (player stats, inventory, floor, enemy positions)
  - [ ] Resume button loads and resumes correctly (player position, enemies, items intact)
  - [ ] Save file is JSON (human-readable for testing)
  - [ ] Old save is overwritten on new save (only one active save slot in MVP)
  - [ ] If no save file exists, Continue button is greyed out
  - [ ] Can load save file multiple times without corruption
- **Priority**: MVP
- **Estimated complexity**: Medium

---

#### Feature: **Status Effects (Poison, Stun, Bleed)**
- **Description**: Abilities apply status effects (Poison: 1 dmg/turn for N turns, Stun: skip next turn, Bleed: 2 dmg/turn). Effects display as icons, resolve at turn start.
- **Inputs**:
  - Ability hits with status effect attached
- **Outputs**:
  - Status effect applied to target
  - Icon appears above target
  - Duration counter shown
  - Each turn: effect damage/consequence applied, duration decrements
- **Dependencies**: StatusEffectSystem, CombatResolver, UI (status icons)
- **Acceptance criteria**:
  - [ ] Poison applies 1 damage per turn, lasts N turns
  - [ ] Stun skips target's next turn
  - [ ] Bleed applies 2 damage per turn, lasts N turns
  - [ ] Multiple effects can stack (poison + bleed at once)
  - [ ] Status effect icon displays above character
  - [ ] Duration text shows remaining turns
  - [ ] Effects resolve at turn start before entity acts
  - [ ] Effects correctly expire after duration
  - [ ] Removed effect icon disappears from UI
- **Priority**: Post-MVP / Optional for Alpha, but test in Beta
- **Estimated complexity**: Medium

---

#### Feature: **Boss Encounters (Floor 5, 10, 15, 20)**
- **Description**: Every 5 floors, player faces boss enemy in large arena. Boss has more HP, special abilities, and drops rare loot. Defeat is required to progress.
- **Inputs**:
  - Descending stairs at floor 5/10/15/20
- **Outputs**:
  - Boss arena loads (single large room, 20x20)
  - Boss spawns with boss-tier stats
  - Boss attacks with special abilities
  - Boss death drops rare loot and bonus Essence
  - Victory unlocks next floor progression
- **Dependencies**: BossController, DungeonGenerator (boss room), CombatResolver, LootSystem
- **Acceptance criteria**:
  - [ ] Floor 5 has a boss (e.g., Lich)
  - [ ] Boss arena is single large chamber (no other enemies)
  - [ ] Boss has 3x player HP approximately
  - [ ] Boss has 2–3 special abilities with cooldowns
  - [ ] Boss abilities have telegraph (player warned 1 turn before)
  - [ ] Defeating boss drops guaranteed rare/legendary item
  - [ ] Boss death awards 100–200 Essence
  - [ ] Stairs appear after boss defeat (progression unlocked)
  - [ ] Boss difficulty scales with run level
  - [ ] If player dies to boss, no essence earned (run ends as loss)
- **Priority**: Post-MVP / Early Beta (but test mechanics in Alpha)
- **Estimated complexity**: High

---

#### Feature: **Experience & Leveling**
- **Description**: Player gains XP from defeated enemies. At XP threshold, player levels up (increases 2 random stats, heals fully).
- **Inputs**:
  - Enemy defeated (XP gained: `enemy_level * 10`)
- **Outputs**:
  - XP bar advances
  - At threshold, player levels up (level increments, stats increase, heal to full HP)
  - Level up animation/sound plays
- **Dependencies**: PlayerStats, CombatResolver, EventBus
- **Acceptance criteria**:
  - [ ] Enemy defeat awards XP (1 XP per enemy_level, minimum 5)
  - [ ] XP bar displays and fills toward next level
  - [ ] Level thresholds scale exponentially (`100 * level^1.5`)
  - [ ] Level up increases Attack and one other stat by +1
  - [ ] Level up fully heals player HP
  - [ ] Level up animation/sound plays
  - [ ] Player can reach level 10+ in a full run (progression feels substantial)
- **Priority**: Post-MVP / Alpha (core progression feature)
- **Estimated complexity**: Low

---

#### Feature: **Item Stat Comparison Tooltips**
- **Description**: Hovering over an item in inventory shows stat delta (e.g., "+5 Attack, -2 Defense") compared to currently equipped item in that slot.
- **Inputs**:
  - Mouse hover over inventory item
- **Outputs**:
  - Tooltip appears with item name, rarity, stats
  - Stat differences highlighted in green (gain) or red (loss)
- **Dependencies**: InventoryManager, InventoryUI
- **Acceptance criteria**:
  - [ ] Tooltip appears within 0.3 seconds of hover
  - [ ] Item name and rarity displayed
  - [ ] All relevant stats shown (Attack, Defense, HP, etc.)
  - [ ] Stat deltas calculated correctly (new item stat - current item stat)
  - [ ] Gains shown in green, losses in red
  - [ ] Tooltip disappears when mouse leaves item
  - [ ] Tooltip does not obscure other UI
- **Priority**: Post-MVP / Polish (nice-to-have UX improvement)
- **Estimated complexity**: Low

---

### 5.2 System Contracts

#### System: **PlayerController**
**Public Interface:**
```csharp
public class PlayerController : MonoBehaviour
{
    // Input & Movement
    public void Move(Vector2Int direction);
    
    // Combat
    public void AttackEnemy(EnemyBase target);
    public void CastAbility(int abilityIndex);
    
    // Inventory
    public void PickupItem(ItemBase item);
    public void UseItem(int inventorySlot);
    public bool TryEquip(ItemBase item);
    
    // State
    public bool IsAlive { get; }
    public Vector2Int CurrentPosition { get; }
    
    // Events
    public event Action<int> OnDamageTaken;
    public event Action<int> OnLevelUp;
    public event Action<ItemBase> OnItemPickedUp;
}
```

**Depends on:**
- PlayerStats — for stat queries and health management
- InventoryManager — for item management
- TurnManager — to signal turn end
- CombatResolver — to execute attacks

**Depended on by:**
- TurnManager — queries current position, requests actions
- EnemyAI — checks player position for targeting
- GameManager — monitors player death
- UI systems — display health, level, inventory

**Internal state:**
- Current position (Vector2Int)
- Current facing direction
- Input state (cached last frame input)
- Ability cooldown counters

**Invariants:**
- Player position is always on walkable tile
- Player cannot be at same position as wall
- IsAlive == (Health > 0)

---

#### System: **PlayerStats**
**Public Interface:**
```csharp
public class PlayerStats : MonoBehaviour
{
    // Health & Mana
    public void TakeDamage(int amount);
    public void Heal(int amount);
    public void RestoreMana(int amount);
    public void SetMana(int amount);
    
    // Progression
    public void GainXP(int amount);
    public void LevelUp();
    
    // Stat Queries
    public int GetFinalAttack();
    public int GetFinalDefense();
    public int GetFinalMaxHealth();
    public int GetFinalMaxMana();
    
    // Properties
    public int CurrentHealth { get; }
    public int MaxHealth { get; }
    public int CurrentMana { get; }
    public int MaxMana { get; }
    public int Level { get; }
    public int CurrentXP { get; }
    public int XPToNextLevel { get; }
    
    // Events
    public event Action<int, int> OnHealthChanged; // current, max
    public event Action<int, int> OnManaChanged;   // current, max
    public event Action OnLevelUp;
    public event Action OnDeath;
}
```

**Depends on:**
- InventoryManager — to query equipment stat bonuses
- MetaProgressionManager — to apply meta-progression bonuses

**Depended on by:**
- CombatResolver — queries attack/defense for damage calc
- UI systems — display health bar, mana bar, level
- PlayerController — checks health for death condition
- GameManager — monitors for death event

**Internal state:**
- currentHealth, maxHealth
- currentMana, maxMana
- currentXP, level
- baseStats (attack, defense, derived from class and meta-progression)

**Invariants:**
- CurrentHealth <= MaxHealth
- CurrentHealth >= 0
- CurrentMana <= MaxMana
- Level >= 1
- If CurrentHealth == 0, OnDeath fires

---

#### System: **InventoryManager**
**Public Interface:**
```csharp
public class InventoryManager : MonoBehaviour
{
    // Item Management
    public bool TryAddItem(ItemBase item);
    public void RemoveItem(int slotIndex);
    public ItemBase GetItem(int slotIndex);
    
    // Equipment
    public void EquipItem(ItemBase item); // throws if slot occupied or incompatible
    public void UnequipSlot(EquipmentSlotType slot);
    public ItemBase GetEquippedItem(EquipmentSlotType slot);
    public ItemBase GetEquippedWeapon();
    
    // Stat Bonuses
    public int GetStatBonus(StatType stat); // sums all equipment bonuses
    
    // Potions
    public int GetPotionCount(PotionType type);
    public void UsePotion(PotionType type); // throws if none available
    
    // Queries
    public int GetFreeSlots();
    public bool IsFull { get; }
    public ItemBase[] GetInventorySnapshot(); // read-only copy
    
    // Events
    public event Action<ItemBase> OnItemAdded;
    public event Action<int> OnItemRemoved;
    public event Action<EquipmentSlotType, ItemBase> OnItemEquipped;
}
```

**Depends on:**
- ItemBase (for item type queries)

**Depended on by:**
- PlayerStats — queries for stat bonuses
- PlayerController — adds/removes items, uses potions
- UI systems — display inventory grid, equipment slots
- LootSystem — supplies items to inventory

**Internal state:**
- itemSlots (array of 10 ItemBase)
- equippedItems (dict of EquipmentSlotType → ItemBase)

**Invariants:**
- itemSlots.Length == 10 (or maxSlots)
- No null items in empty slots (use sentinel or List)
- Each equipment slot has at most one item
- Equipped items are never in itemSlots (only in equippedItems)

---

#### System: **TurnManager**
**Public Interface:**
```csharp
public class TurnManager : MonoBehaviour
{
    // Entity Management
    public void RegisterEntity(EntityBase entity);
    public void UnregisterEntity(EntityBase entity);
    
    // Turn Execution
    public void RequestAction(EntityBase entity, ActionData action);
    public void EndTurn();
    public void ExecuteNextTurn();
    
    // Queries
    public EntityBase GetCurrentActor();
    public int GetCurrentTurnNumber();
    public bool IsPlayerTurn { get; }
    
    // Events
    public event Action<EntityBase> OnTurnStarted; // fired when entity's turn begins
    public event Action<EntityBase> OnTurnEnded;
    public event Action OnRoundComplete; // fired when all entities have acted
}
```

**Depends on:**
- EntityBase (for entity registration)
- EventBus (to fire global events)

**Depended on by:**
- PlayerController — signals turn end after action
- EnemyAI — signals turn end after action
- StatusEffectSystem — subscribes to OnTurnStarted to resolve effects
- UI systems — displays turn number, highlights whose turn it is

**Internal state:**
- entities (ordered list of EntityBase)
- currentActorIndex
- currentTurnNumber
- actionQueue (optional, for future queueing)

**Invariants:**
- currentActorIndex >= 0 && < entities.Count
- currentTurnNumber increments monotonically
- Each entity acts at most once per round

---

#### System: **CombatResolver**
**Public Interface:**
```csharp
public static class CombatResolver
{
    // Damage Calculation
    public static CombatResult CalculateDamage(
        EntityBase attacker,
        EntityBase defender,
        ItemBase weapon,
        int baseDamage);
    
    // Damage Application
    public static void ApplyDamage(EntityBase target, int amount);
    public static void ApplyHealing(EntityBase target, int amount);
    
    // Hit/Miss Rolls
    public static bool RollHit(int attackerAccuracy, int defenderEvasion);
    public static bool RollCrit(int critChance);
    
    // Status Effects
    public static void ApplyStatusEffect(EntityBase target, StatusEffect effect);
}

public struct CombatResult
{
    public int damageAmount;
    public bool isHit;
    public bool isCrit;
    public Vector3 hitPosition; // for VFX
}
```

**Depends on:**
- EntityBase (for stat queries)
- StatusEffectSystem (to apply effects)

**Depended on by:**
- PlayerController — calculates attack damage
- EnemyAI — calculates attack damage
- AbilitySystem — resolves ability damage

**Internal state:**
- None (static, stateless)

**Invariants:**
- damageAmount >= 0 (can be 0 for miss)
- If isHit == false, damageAmount == 0 (misses deal no damage)

---

#### System: **DungeonGenerator**
**Public Interface:**
```csharp
public class DungeonGenerator : MonoBehaviour
{
    // Generation
    public void GenerateFloor(int floorNumber, int seed = 0);
    
    // Queries
    public bool IsWalkable(Vector2Int pos);
    public Vector2Int FindStaircase();
    public List<Vector2Int> GetRoomCenters();
    public List<Vector2Int> FindSpawnPoints(int count); // random walkable tiles
    
    // Debugging
    public void DebugShowLayout(); // draw gizmos of layout
    
    // Events
    public event Action OnFloorGenerated;
}
```

**Depends on:**
- Tilemap (to paint tiles)
- Random (seeded)
- GridHelper (for connectivity checks)

**Depended on by:**
- GameManager — requests floor generation
- EnemySpawner — queries spawn points
- LootSystem — queries item placement locations
- PlayerController — checks IsWalkable for movement

**Internal state:**
- floorLayout (2D array of TileType or RoomData)
- rooms (list of Room with bounds, connectivity)
- seed (current floor seed)
- currentFloorNumber

**Invariants:**
- Layout is fully generated before requests to IsWalkable, FindStaircase, etc.
- All rooms are connected (path guaranteed from start to stairs)
- Stairs exist at exactly one location

---

#### System: **EnemyAI**
**Public Interface:**
```csharp
public class EnemyAI : MonoBehaviour
{
    // Decision Making
    public ActionData DecideAction();
    
    // Perception
    public void NotifyPlayerSeen(Transform playerTransform);
    public void NotifyPlayerLost();
    public bool CanSeePlayer { get; }
    
    // State
    public EnemyBehaviorState CurrentBehavior { get; }
    
    // Events
    public event Action<ActionData> OnActionDecided;
}

public enum EnemyBehaviorState { Idle, Chasing, Attacking, Retreating }

public class ActionData
{
    public ActionType type; // Move, Attack, Ability, Wait
    public Vector2Int targetPosition; // for Move
    public EntityBase targetEntity;   // for Attack/Ability
}
```

**Depends on:**
- GridHelper — pathfinding
- PlayerController — for target position
- EnemyStats — for behavior thresholds

**Depended on by:**
- TurnManager — requests action each turn
- PlayerController — enemy acts in response

**Internal state:**
- currentBehavior (state)
- targetEntity (player or None)
- abilityChargeCounts (cooldowns)
- retreatCounter (how many turns in retreat)

**Invariants:**
- targetEntity is non-null iff currentBehavior is Chasing or Attacking
- Ability cooldowns >= 0

---

#### System: **MetaProgressionManager**
**Public Interface:**
```csharp
public class MetaProgressionManager : MonoBehaviour
{
    // Essence Management
    public void AddEssence(int amount);
    public int GetEssence();
    
    // Upgrades
    public bool TryPurchaseUpgrade(ClassType classType, int upgradeID);
    public bool IsUpgradeUnlocked(ClassType classType, int upgradeID);
    public List<UpgradeData> GetUpgradeTree(ClassType classType);
    
    // Persistence
    public void SaveToDisk();
    public void LoadFromDisk();
    
    // Events
    public event Action<int> OnEssenceChanged; // new total
    public event Action<int> OnUpgradeUnlocked;
}

public class UpgradeData
{
    public int id;
    public string name;
    public int essenceCost;
    public List<int> requirements; // prerequisite upgrade IDs
    public Func<PlayerStats, MetaProgressionBonus> applyBonus;
}
```

**Depends on:**
- SaveLoadManager — for persistence
- PlayerStats — to apply bonuses to stats

**Depended on by:**
- GameManager — subscribes to upgrades, applies bonuses to new runs
- UpgradeTreeUI — displays upgrade tree
- EnemyAI — essence drop rates may scale with progression

**Internal state:**
- totalEssence (lifetime)
- unlockedUpgrades (dict of ClassType → set of int upgradeIDs)
- savePath

**Invariants:**
- totalEssence >= 0
- unlockedUpgrades persists across sessions (saved to disk)

---

#### System: **SaveLoadManager**
**Public Interface:**
```csharp
public static class SaveLoadManager
{
    // Save/Load
    public static void SaveRun(RunData data);
    public static RunData LoadRun();
    public static bool HasSavedRun();
    
    // History
    public static void LogRunCompletion(RunSummary summary);
    public static List<RunSummary> GetRunHistory(int maxRecent = 10);
    
    // Utility
    public static void DeleteCurrentRun();
}

public class RunData
{
    public ClassType playerClass;
    public int currentFloor;
    public int playerLevel, playerXP;
    public int playerHealth, playerMaxHealth;
    public int playerMana, playerMaxMana;
    public ItemBase[] inventory;
    public EquipmentSlots equipped;
    public List<StatusEffect> activeEffects;
    public int essenceThisRun;
    public long timeElapsedMS;
    public int randomSeed; // for floor regeneration if resuming
    public Vector2Int playerPosition;
}

public class RunSummary
{
    public int floorsCleared;
    public int enemiesKilled;
    public int essenceEarned;
    public long timeElapsedMS;
    public DateTime completionTime;
    public bool isVictory;
}
```

**Depends on:**
- JSON serialization library (Newtonsoft.Json or similar)
- GameManager (for run data)

**Depended on by:**
- GameManager — saves/loads on floor transition and run end
- MetaProgressionManager — saves/loads meta-progression data
- MainMenu UI — checks for saved run to show Continue button

**Internal state:**
- currentRunData (cached in memory during gameplay)
- runHistoryList

**Invariants:**
- Save file is valid JSON (parseable)
- Save file contains complete run state (no missing fields)

---

#### System: **LootSystem**
**Public Interface:**
```csharp
public class LootSystem : MonoBehaviour
{
    // Item Generation
    public ItemBase GenerateRandomItem(int floorNumber, ItemRarity? forcedRarity = null);
    
    // Spawning
    public void SpawnLoot(Vector2Int position, ItemBase item);
    
    // Drop Rates
    public float GetDropChance(int floorNumber);
    public float GetRarityChance(ItemRarity rarity, int floorNumber);
    
    // Events
    public event Action<ItemBase, Vector2Int> OnLootSpawned;
}
```

**Depends on:**
- ItemData ScriptableObjects (item definitions)
- Random

**Depended on by:**
- EnemyBase — requests item drop on death
- GameManager — supplies loot data to UI
- DungeonGenerator — spawns initial floor loot

**Internal state:**
- itemDatabase (list of ItemData SO references)
- rarityWeights (probabilities per rarity tier)

**Invariants:**
- rarityWeights sum to 1.0 (100% probability distribution)
- All items in database are valid and have sprites

---

#### System: **StatusEffectSystem**
**Public Interface:**
```csharp
public class StatusEffectSystem : MonoBehaviour
{
    // Effect Management
    public void ApplyEffect(EntityBase target, StatusEffect effect);
    public void RemoveEffect(EntityBase target, StatusEffectType type);
    public void RemoveAllEffects(EntityBase target);
    
    // Queries
    public List<StatusEffect> GetActiveEffects(EntityBase target);
    public int GetEffectDuration(EntityBase target, StatusEffectType type);
    
    // Resolution (called by TurnManager)
    public void ResolveTurnEffects(EntityBase entity);
    
    // Events
    public event Action<EntityBase, StatusEffect> OnEffectApplied;
    public event Action<EntityBase, StatusEffectType> OnEffectExpired;
}

public class StatusEffect
{
    public StatusEffectType type; // Poison, Stun, Bleed, etc.
    public int duration; // remaining turns
    public int potency; // damage per turn or effect strength
}
```

**Depends on:**
- TurnManager — subscribes to OnTurnStarted
- CombatResolver — to apply damage effects
- EntityBase — for effect targets

**Depended on by:**
- AbilitySystem — applies effects on ability hit
- UI systems — display status icons
- EnemyAI — uses status effects to modify behavior (e.g., skip turn if stunned)

**Internal state:**
- activeEffects (dict of EntityBase → list of StatusEffect)

**Invariants:**
- No duplicate effect types on same entity (or stack separately if allowed)
- duration >= 0
- potency >= 0

---

#### System: **AudioManager**
**Public Interface:**
```csharp
public class AudioManager : MonoBehaviour
{
    // Music
    public void PlayMusic(AudioClip clip, bool loop = true, float fadeDuration = 0.5f);
    public void StopMusic(float fadeDuration = 0.5f);
    
    // SFX
    public void PlaySFX(AudioClip clip, float volume = 1f, float pitch = 1f);
    public void PlaySFXAt(AudioClip clip, Vector3 position, float volume = 1f);
    
    // Volume Control
    public void SetVolume(AudioMixerGroup group, float volume); // 0-1
    public float GetVolume(AudioMixerGroup group);
    
    // Events
    public event Action<AudioClip> OnMusicChanged;
}
```

**Depends on:**
- AudioSource
- AudioMixer (optional, for mixing groups)

**Depended on by:**
- PlayerController (play hit/miss/ability sounds)
- EnemyBase (play death sound)
- GameManager (play music on scene load)
- UI systems (play button click sounds)

**Internal state:**
- currentMusicSource (AudioSource)
- sfxSourcePool (list of pooled AudioSources)
- volumeSettings (dict of mixer group → volume)

**Invariants:**
- Only one music track plays at a time
- SFX sources are either playing or available for reuse

---

End of PRD.
