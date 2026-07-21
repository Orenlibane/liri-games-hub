# DungeonDelve — Software Design Document

## 1. Architecture Overview

### 1.1 High-Level Architecture
```
├── Core Systems
│   ├── Game State Management (GameManager, GameState)
│   ├── Turn Management (TurnManager)
│   ├── Entity System (EntityStats, Entity base class)
│   └── UI Framework (UIManager, Canvas stack)
├── Gameplay Systems
│   ├── Dungeon Generation (DungeonGenerator, TileData, RoomTemplate)
│   ├── Player Control (PlayerController, InputHandler)
│   ├── Combat System (CombatResolver, Ability system)
│   ├── Enemy AI (EnemyAI, StateTree/BehaviorTree)
│   └── Inventory System (InventoryManager, ItemStack)
├── Progression
│   ├── Meta-Progression (MetaProgressionManager, UnlockData)
│   └── Run Statistics (RunStatistics, AchievementTracker)
└── Content
    ├── ScriptableObjects (ClassDefinition, ItemData, EnemyConfig, RoomTemplate)
    ├── Scenes (Bootstrap, MainMenu, Gameplay, MetaScreen)
    └── Assets (Sprites, Tilemap, Palette)
```

### 1.2 Key MonoBehaviours & Classes
- **GameManager**: Singleton managing game state (menu → gameplay → death → meta-progression)
- **TurnManager**: Turn order queue, action processing, round execution
- **DungeonGenerator**: Procedural dungeon creation using room templates + random corridors
- **PlayerController**: Player input handling, movement validation, ability selection
- **EntityStats**: Base stats container (HP, Attack, Defense, Speed)
- **Entity**: Base class for Player, Enemies; contains EntityStats, position, health state
- **EnemyAI**: Decision-making for enemy movement and attacks
- **InventoryManager**: Equipment slots, consumable management, item dropping
- **MetaProgressionManager**: Persistence layer for unlocks, stats tracking

### 1.3 ScriptableObject Architecture
- **ClassDefinition**: Stores class-specific starting stats, abilities, sprites
- **ItemData**: Item properties (rarity, stats, special effects), icon, prefab
- **EnemyConfig**: Enemy stat templates, AI behavior weights, loot tables
- **RoomTemplate**: Predefined room layouts (start, treasure, combat, boss), tilemap data
- **MetaUnlock**: Unlockable content (class variants, items, modifiers)

### 1.4 Scene Structure
1. **Bootstrap**: Initialization (load meta-progression, instantiate GameManager)
2. **MainMenu**: Menu UI, run start, meta-progression screen
3. **Gameplay**: Dungeon view, HUD, inventory screen, combat log
4. **Pause/Options**: Pause menu, settings

---

## 2. Detailed System Design

### 2.1 Game State Machine
```
States:
  - MainMenu → StartRun
  - Gameplay → EnemyTurn → PlayerTurn (loop)
  - Gameplay → BossEncounter (every 5 floors)
  - Combat → LevelUp / ItemReward / ContinueFloor
  - Death → MetaProgressionScreen → MainMenu
```

### 2.2 Turn System
**Turn Flow:**
1. Player declares action (move, attack, ability, item, pass)
2. Action validation (pathfinding, range, resource check)
3. Player action execution
4. Enemy AI processes (per enemy in speed order)
5. Round effects (poison tick, blessing, etc.)
6. Check win/lose conditions
7. UI updates

**TurnManager responsibilities:**
- Maintain turn order queue (sorted by Speed stat)
- Handle action requests from PlayerController
- Execute all enemy actions in order
- Invoke callbacks for UI updates

### 2.3 Dungeon Generation
**Algorithm:**
1. Initialize root room at floor entrance (start room template)
2. For each floor (1-10):
   - Generate 8-15 random rooms (combat, treasure, elite, safe)
   - Connect rooms with corridors using A* pathfinding
   - Place enemy spawns based on room type and floor depth
   - Place 2-4 item pickups per floor
   - Every 5th floor: Place boss room at end
3. Export to tilemap for Gameplay scene

**Room Templates:**
- StartRoom: No enemies, entrance marked
- TreasureRoom: 1-2 items, no enemies
- CombatRoom: 2-5 enemies scaled to floor
- EliteRoom: 1 elite enemy, rarer loot
- BossRoom: Boss enemy, high-value loot
- CorridorTile: Plain tile, occasional trap

**Procedural Seeding:** Use run seed for consistent dungeon across retries in same session

### 2.4 Combat System

**Entity Stats:**
```csharp
HP: int (current + max)
Attack: int (base damage 1d6 + attack stat)
Defense: int (damage reduction %)
Speed: int (turn order)
Armor: int (from equipment)
```

**Combat Resolution:**
1. Attacker rolls: 1d20 + Attack vs Defender armor
2. If hit: Damage = 1d6 + Attack - Defense/2
3. Apply status effects (bleed, stun, poison)
4. Defender retaliate (if not stunned)
5. HP check → death if ≤ 0

**Abilities (Class-Specific):**
- **Warrior**: Block (reduce damage next turn), Cleave (AoE attack)
- **Rogue**: Backstab (high damage from stealth), Dash (move 2 tiles + attack)
- **Mage**: Fireball (AoE spell), Mana Shield (convert mana to armor)

**Status Effects:**
- Bleed: 1 damage/turn for 3 turns
- Stun: Skip next turn
- Poison: 1 damage/turn for 5 turns
- Block: Reduce next damage by 50%

### 2.5 Player Classes

**Warrior**
- Starting HP: 30
- Starting Attack: 8
- Starting Defense: 5
- Ability: Block, Cleave
- Playstyle: Tank, sustained damage

**Rogue**
- Starting HP: 18
- Starting Attack: 12
- Starting Defense: 2
- Ability: Backstab, Dash
- Playstyle: High risk, high reward

**Mage**
- Starting HP: 15
- Starting Attack: 6
- Mana: 20
- Starting Defense: 1
- Ability: Fireball, Mana Shield
- Playstyle: Control, ranged damage

### 2.6 Inventory & Items

**Slots:**
- Weapon (1)
- Armor (1)
- Ring (2)
- Consumables (unlimited)

**Item Rarities & Effects:**
- Common: +1 Attack weapon, +1 Defense armor
- Rare: +3 Attack, special effect (e.g., lifesteal on weapon)
- Epic: +5 Attack, powerful passive (e.g., +50% crit chance)

**Consumables:**
- Health Potion: +10 HP
- Mana Potion (Mage only): +10 Mana
- Explosive: AoE damage item
- Key: Unlock treasure rooms

**Loot System:**
- Floor rewards: 1-2 items + 50-200 gold per floor
- Boss rewards: 1 epic item + 500 gold
- Chests: Random item based on rarity weights

### 2.7 Enemy AI

**AI States:**
- Idle: Wait for player proximity
- Alert: Move toward player
- Attack: Use best available ability
- Flee: Run if HP < 30%

**Decision Tree:**
```
IF can_attack_player:
  → Attack highest-threat target
ELSE IF can_move_closer:
  → Move toward player
ELSE:
  → Pass turn (wait for player)
```

**Enemy Types:**
- Goblin: Low HP (5), low attack (3), fast (speed 6)
- Orc: Medium HP (12), medium attack (6), slow (speed 3)
- Troll: High HP (20), medium attack (5), very slow (speed 2), regen 1 HP/turn

### 2.8 Meta-Progression System

**Unlocks (Earned on Death):**
- Starting Items: Unlock common weapons/armor as run-starter items
- Class Variants: Warrior (Paladin), Rogue (Assassin), Mage (Sorcerer)
- Dungeon Modifiers: Harder difficulty, more/fewer enemies, boss mutations

**Persistence:**
- Save to PlayerPrefs or JSON file
- Track total runs, total kills, max floor reached
- Unlock requirements: "Reach floor 5", "Kill 50 goblins", "Equip 3 rare items"

**Display:**
- Meta-progression screen in MainMenu
- Show unlocked items, locked items (with requirements), achievements

---

## 3. User Interface Design

### 3.1 Screen Hierarchy

**Main Menu**
- Title + art
- "New Run" button → Class selection
- "Continue" button (if run in progress)
- "Meta-Progression" button
- "Settings" button

**Class Selection**
- 3 class cards (Warrior, Rogue, Mage)
- Stats comparison
- "Start Run" button

**Gameplay HUD**
- Top-left: Player stats (HP, mana/block status)
- Top-right: Floor indicator, enemy count
- Center: Isometric/tilemap view
- Bottom: Action buttons (Move, Attack, Ability, Inventory, Wait)
- Right panel: Mini-map
- Log: Last 3 actions in corner

**Inventory Screen**
- Equipment grid (slots)
- Consumable list
- Drop/equip/use buttons

**Pause Menu**
- Resume
- Settings
- Return to Menu

**Death Screen**
- Stats summary (floor reached, enemies killed, loot)
- Unlock notification (if new unlock earned)
- "Back to Menu" button

### 3.2 Visual Feedback
- Toast notifications for item pickup
- Floating damage numbers on hit
- Status effect icons on entities
- Enemy health bars on hover
- Color coding: enemy (red), NPC (yellow), item (gold)

### 3.3 Accessibility
- Colorblind mode (alternate palette)
- High contrast option
- Text scaling
- Remappable controls

---

## 4. Technical Implementation Details

### 4.1 Pathfinding & Grid Movement
- **System**: A* pathfinding for enemy movement
- **Grid**: 32px tiles (matching 32px PPU)
- **Obstacles**: Walls, enemies, destructible objects
- **Diagonals**: Allowed (8 directions) or restricted (4 directions) - design decision

### 4.2 Serialization & Save Data
**File Structure:**
```json
{
  "runState": {
    "currentFloor": 3,
    "playerClass": "Warrior",
    "playerStats": { "hp": 25, "maxHP": 30, "attack": 10 },
    "inventory": [ { "itemId": "sword_iron", "rarity": "common" } ],
    "dungeonSeed": 12345
  },
  "metaProgression": {
    "totalRuns": 15,
    "maxFloor": 8,
    "unlockedItems": [ "sword_steel", "armor_leather" ],
    "achievements": { "first_boss_kill": true }
  }
}
```

### 4.3 Resource Management
- **Memory**: Unload unused scenes, pool enemy objects
- **Performance**: Culling (off-screen entities don't process AI)
- **Audio**: Background music + SFX channels (separate volume)

### 4.4 Cross-Platform Considerations
**PC (Primary):**
- Keyboard input (WASD, arrow keys, ability hotkeys)
- Mouse for UI
- Fullscreen + windowed modes

**WebGL (Secondary):**
- Touch input (for mobile browsers)
- UI optimization (larger buttons)
- Lower resolution export for faster load

---

## 5. Development Tasks & Sprint Plan

### Sprint 0: Foundation (Weeks 1-2)
**Goal**: Basic framework, no gameplay yet

1. **Project Setup** (2 days)
   - Initialize Unity project
   - Set up folder structure (Scripts, Prefabs, ScriptableObjects, Scenes, Art)
   - Install UI Toolkit (if using) or legacy UI
   - Git repository setup

2. **Core Classes & Architecture** (3 days)
   - Implement Entity base class (position, health, stats)
   - Implement EntityStats (HP, Attack, Defense, Speed)
   - Implement GameManager singleton
   - Implement GameState enum + state machine basics

3. **Input & Bootstrap Scene** (2 days)
   - Bootstrap scene → MainMenu scene loading
   - InputHandler for keyboard/touch
   - Pause menu framework

4. **ScriptableObject Templates** (2 days)
   - Create ClassDefinition SO template
   - Create ItemData SO template
   - Create EnemyConfig SO template
   - Create empty instances (3 classes, 5 items, 3 enemies)

5. **Basic UI Setup** (3 days)
   - MainMenu prefab (buttons, layout)
   - Class selection screen
   - HUD canvas structure
   - Pause menu

**Sprint 0 Deliverable**: Game boots to menu, can select class (no actual game start yet)

---

### Sprint 1: Dungeon & Movement (Weeks 3-5)
**Goal**: Playable dungeon with player movement, no combat

1. **DungeonGenerator** (5 days)
   - Room template system (ScriptableObjects)
   - Procedural layout generation (random room placement + corridors)
   - A* pathfinding for corridor generation
   - Tilemap creation and assignment
   - Seeding system for consistent generation

2. **Grid-Based Movement** (3 days)
   - PlayerController movement system
   - Collision detection (walls, enemies)
   - Validation before movement
   - Animation blending for smooth movement

3. **TurnManager Basics** (2 days)
   - Turn queue (speed-based ordering)
   - Action submission system
   - Basic turn cycle (player → enemies → end turn)

4. **Dungeon Visualization** (3 days)
   - Sprite atlas setup (16-bit pixel art, 32px)
   - Tilemap rendering
   - Player & enemy sprite display
   - Fog of war (optional: only show explored tiles)

5. **Item Pickup System** (2 days)
   - Item spawning in rooms
   - Collision detection + pickup
   - Visual feedback (gold tint, toast notification)

**Sprint 1 Deliverable**: Can descend 2 floors, move around, pick up items; no enemies act yet

---

### Sprint 2: Basic Combat (Weeks 6-8)
**Goal**: Functional turn-based combat with 3 basic enemy types

1. **Combat Resolver** (4 days)
   - Hit/miss calculation (1d20 + Attack)
   - Damage calculation (1d6 + Attack - Defense)
   - Status effect application (bleed, stun, poison)
   - Health depletion → death

2. **EnemyAI Decision Tree** (3 days)
   - Pathfinding to player
   - Attack/ability selection
   - Flee behavior (HP threshold)
   - Turn order processing

3. **Enemy Types Implementation** (3 days)
   - Goblin (fast, low health)
   - Orc (balanced)
   - Troll (slow, regen)
   - Spawn in room layouts based on difficulty

4. **Ability System (Warrior Only)** (2 days)
   - Block ability (reduce next damage by 50%)
   - Cleave ability (AoE attack)
   - Ability queue + execution

5. **Combat UI & Feedback** (2 days)
   - Floating damage numbers
   - Status effect icons
   - Combat log (last 5 actions)
   - Health bar updates

**Sprint 2 Deliverable**: Can fight enemies, enemies can fight back; can die

---

### Sprint 3: Class Abilities & Balance (Weeks 9-11)
**Goal**: All 3 classes playable with unique abilities; basic balance pass

1. **Rogue Abilities** (2 days)
   - Backstab (high damage, requires setup)
   - Dash (move 2 tiles + attack)
   - Stealth/detection system

2. **Mage Abilities & Mana System** (3 days)
   - Mana pool (max 20)
   - Fireball (AoE, costs mana)
   - Mana Shield (convert mana to armor)
   - Mana regen rate (2 per turn)

3. **Inventory System Refinement** (2 days)
   - Equipment slots (weapon, armor, 2 rings)
   - Stat calculation from equipment
   - Drop/swap mechanics
   - Consumable management (potions, explosives, keys)

4. **Balance Pass** (3 days)
   - Adjust starting stats for each class
   - Adjust enemy AI weights
   - Adjust item drop rates
   - Difficulty curve per floor

5. **Special Items Implementation** (2 days)
   - Rare items with special effects (lifesteal, crit bonus)
   - Epic items (powerful passives)
   - Consumables (potions, explosives)

**Sprint 3 Deliverable**: All 3 classes playable, balanced for 10-floor run; items affect gameplay

---

### Sprint 4: Progression & Bosses (Weeks 12-14)
**Goal**: Boss encounters every 5 floors; level-up/reward system; meta-progression framework

1. **Boss Encounter System** (3 days)
   - Boss enemy type + stat scaling
   - Boss room trigger (floor 5, 10)
   - Boss AI (more complex attack patterns)
   - Boss loot (guaranteed epic item + gold)

2. **Level-Up/Reward System** (3 days)
   - Floor completion: item reward choice (pick 1 of 3)
   - Stat escalation per floor (enemy HP/damage +10%)
   - Healing upon floor clear (optional)
   - Upgrade selection (e.g., +2 Attack or +1 Defense)

3. **Meta-Progression Manager** (3 days)
   - Save/load run state to JSON
   - Save/load meta-progression (unlocks, statistics)
   - PlayerPrefs integration (or custom save file)
   - Run statistics tracking (kills, damage dealt, items collected)

4. **Win Condition** (1 day)
   - Floor 10 boss defeat = victory
   - Victory screen with summary stats
   - Unlock reward notification

5. **Death & Game Over Flow** (2 days)
   - Death screen with stats
   - Meta-progression unlock check
   - Return to main menu with persistence

**Sprint 4 Deliverable**: Can complete full 10-floor run with bosses; saves all progress; win condition works

---

### Sprint 5: Meta-Progression & Content (Weeks 15-17)
**Goal**: Unlockable content drives replayability; multiple runs feel rewarding

1. **Unlock System** (3 days)
   - Unlock data structure (requirements, unlocked content)
   - Display unlocks in meta-progression screen
   - Grant unlocks on death (with notification)
   - Examples: "Unlock Iron Sword after reaching floor 5"

2. **Class Variants** (3 days)
   - Warrior → Paladin (more defense, healing ability)
   - Rogue → Assassin (higher crit, stealth focus)
   - Mage → Sorcerer (more mana, spell variants)
   - Selectable in class menu after unlock

3. **Dungeon Modifiers** (2 days)
   - Hard mode (more enemies, less loot)
   - Chaos mode (random enemy spawns)
   - Blessing mode (guaranteed rarer items)
   - Selectable before run start

4. **Achievement System** (2 days)
   - Define 10-15 achievements (e.g., "Kill 100 goblins", "Equip 3 rare items")
   - Track during run
   - Display in meta-progression
   - Grant meta-unlock rewards

5. **Run Statistics Screen** (1 day)
   - Display: total runs, best floor, total kills, total gold
   - Leaderboard data structure (if multiplayer planned)
   - Export stats option

**Sprint 5 Deliverable**: Multiple unlocks earned per run; class variants playable; achievements tracked

---

### Sprint 6: Polish & Optimization (Weeks 18-20)
**Goal**: Bug fixes, performance, visual/audio polish

1. **UI Polish** (3 days)
   - Menu animations (fade, slide)
   - Inventory UI refinement
   - Tooltip system for items/abilities
   - Keyboard navigation for all menus

2. **Visual Effects** (3 days)
   - Particle effects for spells (fireball, cleave)
   - Screen shake on big hits
   - Sprite animation for idle/walk/attack
   - Smooth transitions between screens

3. **Audio Integration** (2 days)
   - Background music (exploration, boss, game over)
   - SFX (hit, ability, pickup, level up, death)
   - Volume sliders in settings
   - Mute option

4. **Optimization Pass** (2 days)
   - Object pooling for projectiles/effects
   - Culling (off-screen entities don't tick)
   - Asset compression
   - Performance profiling (target 60 FPS)

5. **Bug Fixes & QA** (2 days)
   - Edge case testing (full inventory, soft locks)
   - Platform testing (PC fullscreen, WebGL)
   - Save/load validation
   - Crash reporting

**Sprint 6 Deliverable**: Game feels polished; no crashes; 60 FPS target met; ready for closed beta

---

### Sprint 7: Platform Optimization & Release Prep (Weeks 21-22)
**Goal**: WebGL export; platform-specific optimization; build distribution

1. **WebGL Build** (2 days)
   - Build settings optimization
   - Asset compression for web
   - Loading screen
   - Local storage vs PlayerPrefs for save data

2. **PC Build Optimization** (1 day)
   - Exe creation
   - Installer setup (optional)
   - Platform-specific controls (gamepad support)

3. **Documentation** (2 days)
   - Gameplay guide (classes, items, tips)
   - Controls reference card
   - Troubleshooting FAQs

4. **Final QA & Balancing** (1 day)
   - Full playthrough (all classes, all modifiers)
   - Difficulty balance final pass
   - Economy balance (gold, item drop rates)

**Sprint 7 Deliverable**: Shippable build on PC + WebGL; documentation complete; ready for release

---

## 6. Risk Assessment & Mitigation

| Risk | Severity | Mitigation |
|------|----------|-----------|
| Procedural dungeon too simple/repetitive | Medium | Test with 20+ generated dungeons early; iterate on templates |
| Balance issues (game too hard/easy) | Medium | Playtesting at Sprint 2+; adjust enemy/item stats in data |
| Save system bugs (data loss) | High | Implement robust serialization; backup saves; validate on load |
| Performance on WebGL | Medium | Profile early; use asset compression; consider resolution scaling |
| Scope creep (too many features) | High | Strict feature freeze at Sprint 5; defer "nice-to-haves" to post-launch |
| Permadeath frustration (players quit) | Medium | Tune difficulty curve; ensure early wins feel rewarding; meta-progression motivates retries |

---

## 7. Success Metrics

**MVP Targets (End of Sprint 4):**
- Full 10-floor run completable in 20-30 min
- All 3 classes playable with distinct playstyles
- No critical bugs (crashes, softlocks)
- Framerate: 60 FPS on target PC specs

**Post-Launch (Sprint 5+):**
- 5+ unlockable items per class
- 3+ class variants
- 15+ achievements
- Avg. session time: 25 min (successful run)
- Avg. rerun rate: 50% of players attempt a second run

---

## 8. Appendix: ScriptableObject Templates

### ItemData
```
- itemId: string
- displayName: string
- description: string
- sprite: Sprite
- rarity: Rarity (Common, Rare, Epic)
- itemType: ItemType (Weapon, Armor, Ring, Consumable)
- statModifiers: StatModifier[] (attack +3, defense +1)
- specialEffect: string (e.g., "lifesteal_25")
- consumableEffect: Action (e.g., "heal_10_hp")
```

### ClassDefinition
```
- className: string
- description: string
- classSprite: Sprite
- startingStats: EntityStats
- abilities: Ability[] (Block, Cleave for Warrior)
- startingItems: ItemData[]
- playstyleDescription: string
```

### EnemyConfig
```
- enemyName: string
- sprite: Sprite
- baseStats: EntityStats
- abilities: Ability[]
- aiWeights: AIDecisionWeights (attack%, move%, flee%)
- lootTable: LootEntry[] (item drop chance, rarity)
- floorScaling: float (HP/damage multiplier per floor)
```

### RoomTemplate
```
- roomType: RoomType (Start, Treasure, Combat, Elite, Boss)
- tilemap: TileBase[][]
- spawns: SpawnPoint[] (position, enemy type)
- loot: LootPoint[] (position, item rarity)
- exits: Vector2[] (doorway positions)
```

---

## 9. Revision History
- **v1.0** (2026-03-21): Initial SDD based on PRD; 7-sprint plan; MVP scope locked at Sprint 4

