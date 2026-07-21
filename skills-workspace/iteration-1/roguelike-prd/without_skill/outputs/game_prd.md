# Roguelike Dungeon Crawler - Product Requirements Document

## 1. Executive Summary

**Game Title:** [TBD - Dungeon Runner / Depths Unknown / Endless Descent]

**Genre:** Roguelike Dungeon Crawler with Turn-Based Combat

**Platform:** PC (Windows, Mac, Linux via Steam/Epic Games Store)

**Target Audience:** Players aged 16+, fans of roguelikes (Hades, Dead Cells, Slay the Spire), turn-based tactics, and pixel art aesthetics

**Core Concept:** A procedurally generated dungeon crawler where players select from three character classes and battle through infinite floors of randomly generated dungeons. Permadeath is permanent, but progression between runs is maintained through meta-progression unlocks and persistent upgrades.

---

## 2. Game Overview

### 2.1 Core Gameplay Loop

1. **Pre-Run Setup:** Player selects character class (Warrior, Rogue, Mage) and optionally applies meta-progression unlocks
2. **Dungeon Exploration:** Navigate procedurally generated dungeon floors, discover items, and fight enemies
3. **Combat:** Turn-based tactical combat where player and enemies alternate actions
4. **Progression:** Defeat enemies, collect loot, equip items, gain XP, and advance through floors
5. **Boss Encounters:** Every 5 levels, face a unique boss with special mechanics
6. **Permadeath:** Upon player death, the run ends and all in-run progress is lost
7. **Meta-Progression:** Unlock permanent bonuses, new items, and class variants based on previous run achievements
8. **Loop:** Start a new run with accumulated permanent bonuses

### 2.2 Target Session Length

- **Single Run:** 30-90 minutes (varies by player skill and decisions)
- **Session:** 1-3 runs per session (45-270 minutes)
- **Replayability:** Infinite runs with procedural generation and meta-progression unlocks

---

## 3. Design Pillars

1. **Procedural Generation:** Every dungeon is unique; no two runs feel identical
2. **Meaningful Choices:** Item selection, class abilities, and skill trees create varied playstyles
3. **Permadeath Stakes:** Permanent character death creates tension and consequence
4. **Progress That Persists:** Meta-progression ensures no run is "wasted," even after permadeath
5. **Pixel Art Aesthetic:** Retro-inspired visuals with modern UI/UX clarity
6. **Balanced Accessibility:** Difficulty scales with player unlocks; new players start reasonable

---

## 4. Player Classes

### 4.1 Warrior

**Philosophy:** High durability, close-range combat, straightforward mechanics

**Base Stats:**
- Health: 150 HP
- Damage: 12-15
- Defense: 20%
- Speed: 4 (turn-based initiative; lower = slower)

**Class Abilities:**
- **Slash:** Basic attack dealing 12-15 damage
- **Shield Bash:** Costs 1 AP (action point), deals 8 damage + stuns enemy for 1 turn
- **Defensive Stance:** Costs 1 AP, gain 50% damage reduction for 2 turns
- **Berserker Rage:** Ultimate (5 turn cooldown), deal double damage for 3 turns, take 10% more damage

**Class Unlocks (via Meta-Progression):**
- Unlock: Parry ability (counter-attack on enemy hit)
- Unlock: Armor Plating passive (reduce all damage by 5%)
- Unlock: Cleave ability (hit 2 enemies in melee range)

**Playstyle:** Tank-oriented; survive long fights, absorb damage, deal consistent damage

---

### 4.2 Rogue

**Philosophy:** High damage, mobility, high-risk/high-reward gameplay

**Base Stats:**
- Health: 100 HP
- Damage: 16-20
- Defense: 10%
- Speed: 6 (faster than others)

**Class Abilities:**
- **Quick Slash:** Basic attack dealing 16-20 damage
- **Dash:** Costs 1 AP, move 2 tiles away from current position
- **Backstab:** Costs 1 AP, if enemy is not facing player, deal 30 damage
- **Shadow Clone:** Ultimate (6 turn cooldown), create a clone that attacks the same enemy once

**Class Unlocks (via Meta-Progression):**
- Unlock: Evasion passive (20% chance to dodge incoming damage)
- Unlock: Poison Strike ability (apply poison; enemy takes 5 damage per turn for 3 turns)
- Unlock: Cloak of Shadows (gain invisibility for 1 turn; next attack is guaranteed crit)

**Playstyle:** Glass cannon; high damage output, lower survivability, relies on positioning and smart ability usage

---

### 4.3 Mage

**Philosophy:** Ranged damage, crowd control, utility-based gameplay

**Base Stats:**
- Health: 110 HP
- Damage: 10-14
- Defense: 5%
- Speed: 5 (average)

**Class Abilities:**
- **Fireball:** Costs 1 AP, deal 16 damage to target and nearby enemies in 1-tile radius
- **Frost Nova:** Costs 1 AP, deal 8 damage and slow all enemies in 2-tile radius (30% movement reduction for 2 turns)
- **Teleport:** Costs 1 AP, move to any visible tile within 4 tiles
- **Meteor Storm:** Ultimate (6 turn cooldown), rain meteors on 3 random tiles; each deals 25 damage

**Class Unlocks (via Meta-Progression):**
- Unlock: Mana Shield passive (10% of HP converted to temporary shield)
- Unlock: Chain Lightning ability (damage primary target, bounce to 2 nearby enemies)
- Unlock: Arcane Mastery passive (all abilities cost 1 less AP, minimum 0)

**Playstyle:** Crowd control and AoE damage; maintain distance, control enemy movements, deal burst damage

---

## 5. Core Systems

### 5.1 Procedural Dungeon Generation

#### 5.1.1 Dungeon Architecture

**Floor Structure:**
- Each floor is a grid-based dungeon (20x20 to 40x40 tiles, randomized per floor)
- Rooms connected by corridors; enemies placed in rooms and corridors
- 1-3 encounters per floor (enemy groups)
- 1 staircase leading to next floor
- Multiple pathways; players choose route through floor

**Floor Progression:**
- Floors 1-5: Tutorial difficulty, basic enemy types
- Floors 6-10: Difficulty ramps; new enemy types introduced
- Floors 11-15: Medium difficulty; elite enemies appear
- Floors 16-20: High difficulty; dangerous enemies
- Floors 21-25: Extreme difficulty; boss floor
- Floors 26+: Procedural generation continues; difficulty scales with meta-progression

#### 5.1.2 Enemy Placement

- **Placement Algorithm:** Rooms are seeded with enemies based on floor difficulty
- **Variation:** Randomized enemy types per floor (e.g., Goblins, Skeletons, Demons)
- **Elite Enemies:** 10% chance per enemy to be "elite" variant (higher stats, special abilities)
- **Boss Encounters:** Every 5 floors (Floors 5, 10, 15, 20, 25, 30, etc.), 1 unique boss fight replaces normal encounters

---

### 5.2 Turn-Based Combat System

#### 5.2.1 Combat Flow

1. **Turn Order:** Determined by character Speed stat (higher speed = earlier turn)
2. **Player Turn:**
   - Player spends Action Points (AP) to take actions (move, attack, use ability)
   - Player can move up to their movement speed (typically 2-3 tiles) per turn
   - Player can perform one main action (attack, use ability) per turn
   - Player can end turn early
3. **Enemy Turns:** Each enemy takes its turn in speed order
4. **Round End:** After all entities act, new round begins
5. **Victory:** All enemies defeated
6. **Defeat:** Player HP reaches 0

#### 5.2.2 Action Point System

- **AP (Action Points):** Resource spent on abilities; regenerates each turn
- **Base AP per Turn:** 2 AP
- **AP Costs:**
  - Basic Attack: 1 AP
  - Movement: 0 AP (built into turn allowance)
  - Ability Use: 0-2 AP (varies by ability)
- **AP Upgrades:** Unlock via meta-progression to gain +1 AP per turn

#### 5.2.3 Damage Calculation

```
Damage = [Base Damage × Skill Modifier] ± Variance ± (Enemy Defense % reduction)
Critical Hit: 50% bonus damage, 15% base crit chance (increases with items/abilities)
Ability Modifiers: Each ability has unique damage multipliers
```

#### 5.2.4 Status Effects

- **Stun:** Affected unit skips their next turn
- **Slow:** Affected unit's speed reduced by 30% for X turns
- **Poison:** Affected unit loses 5 HP per turn for X turns
- **Bleed:** Affected unit loses 3 HP per turn for X turns
- **Vulnerability:** Affected unit takes 25% more damage for X turns
- **Shield:** Absorb X damage before affecting HP

---

### 5.3 Loot & Equipment System

#### 5.3.1 Item Types

**Weapons:**
- Unique to class (Warrior: Swords/Axes; Rogue: Daggers/Bows; Mage: Staves/Wands)
- Rarity: Common, Uncommon, Rare, Epic, Legendary
- Stats: Base damage, attack speed, special effects
- Special Effects: Bonus crit chance, lifesteal, AOE, etc.

**Armor:**
- Helmets, Chests, Gloves, Boots (4 equip slots per set)
- Each provides defense % and additional stats (HP, resistances)
- Special Effects: Status resistance, ability cost reduction, stat bonuses

**Accessories:**
- Rings, Amulets (2 slots)
- Provide stat bonuses (HP, damage, defense, crit chance, etc.)
- No rarity limitations; stackable effects

**Consumables:**
- Health Potions: Restore 50 HP (carry max 5)
- Mana Potions: Restore 2 AP (if applicable to class; carry max 3)
- Scrolls of Teleportation: Instant escape from combat (carry max 2)
- Blessing Scrolls: +20% damage for 3 turns (carry max 3)

#### 5.3.2 Loot Drop Rules

- **Enemy Drops:** 30% chance per enemy defeated
- **Chest Loot:** 1-2 chests per floor contain 1-2 items
- **Boss Drops:** Guaranteed 1 Epic+ weapon or armor piece
- **Scaling:** Drop rarity increases with floor number

#### 5.3.3 Item Usage

- **Equip:** Weapons/armor automatically equipped when found (can switch between drops)
- **Auto-Manage:** Inventory automatically highlights best items for player class
- **Discard:** Drop unwanted items (no selling; loot is for in-run use only)

---

### 5.4 Progression & Experience

#### 5.4.1 In-Run Leveling

- **XP Per Enemy:** Based on enemy difficulty (1-10 XP per enemy)
- **Level Caps:** Max Level 20 per run
- **Leveling Rewards:** Each level grants +5 Max HP and +1 ability point to spend

#### 5.4.2 Ability Points

- **Earned:** 1 per level (max 20 per run)
- **Usage:** Spent to unlock/upgrade class abilities
- **Ability Trees:** Each class has 3 upgrade paths (e.g., Warrior: Tank, Damage, Crowd Control)
- **Upgrades:** Enhance ability damage, reduce cooldown, add effects

---

### 5.5 Permadeath & Run Persistence

#### 5.5.1 Permadeath Rules

- Player death = immediate run end
- All in-run items, XP, levels, equipment are lost
- Player respawns at main menu
- Run statistics saved (depth reached, enemies killed, items found, etc.)

#### 5.5.2 Meta-Progression Unlocks

**Unlock Mechanics:**
- Achievements triggered during runs (e.g., "Defeat 50 Goblins," "Reach Floor 10," "Find 5 Rare Items")
- Unlocks are permanent and apply to all future runs
- Unlocks take effect in the next run started

**Unlock Categories:**

1. **Class Upgrades:**
   - New abilities (e.g., Warrior unlocks "Cleave")
   - Passive bonuses (e.g., +10% crit chance)
   - Variants/skins (visual changes)

2. **Starting Bonuses:**
   - +5 Max HP per unlock
   - +1 AP per turn per unlock
   - +5% damage per unlock
   - Starting item grants (e.g., "Start with Iron Sword")

3. **Item Unlocks:**
   - New weapons/armor become available in loot pools
   - New accessories with unique effects

4. **Difficulty Modifiers:**
   - Hard Mode: Unlock after beating Floor 10
   - Ironman Mode: Unlock after beating Floor 20 (no item drops, permadeath, increased rewards)

---

## 6. User Interface & UX

### 6.1 Main Menu

**Buttons:**
- Play (start new run with class selection)
- Continue (resume last active run, if available)
- Settings (audio, graphics, gameplay options)
- Collection (view unlocked items, achievements, run statistics)
- Quit

---

### 6.2 Class Selection Screen

**Display:**
- Three class cards with artwork, stats, abilities, playstyle description
- Difficulty indicator per class (Warrior: Easy, Rogue: Medium, Mage: Medium)
- Option to select class and apply meta-progression unlocks before starting

---

### 6.3 In-Game UI

**Top Bar:**
- Player name/class icon
- Current Floor number
- Current HP / Max HP (with health bar)
- Current AP / Max AP (with action point bar)

**Right Side Panel:**
- Equipment display (weapon, armor pieces, accessories)
- Current item stats and comparisons to equipped items

**Bottom Bar:**
- Ability hotkeys (1-4) with cooldown timers
- Consumable items with quantity
- Current turn order indicator

**Mini-Map:**
- Small map of current floor with player position
- Revealed rooms/corridors only (fog of war)
- Enemy positions visible when in same room

---

### 6.4 Inventory & Equipment

**Grid Layout:**
- Equipment slots: Weapon, Head, Chest, Hands, Feet, Ring 1, Ring 2
- Automatic swap highlights best stat upgrades
- Drag-and-drop item management
- Quick-compare tool (highlight item to see stat changes)

---

### 6.5 Death Screen & Run Summary

**Upon Death:**
- "You Died" screen with run statistics:
  - Depth reached (e.g., Floor 12)
  - Enemies killed
  - Items collected
  - Gold earned (if applicable)
  - New achievements unlocked (if any)
- Option: "Return to Menu" or "View Run Statistics"

---

### 6.6 Collection & Progression Screen

**Tabs:**
- Achievements (list of meta-progression unlocks with progress)
- Items (catalog of all found items with rarity/source)
- Statistics (total runs, deepest floor, enemies killed, playtime)
- Unlocks (preview of next available meta-progression unlock)

---

## 7. Audio Design

### 7.1 Music

- **Main Menu:** Atmospheric, epic theme (looping, 2-3 minutes)
- **Exploration:** Ambient dungeon themes (varies per floor type)
- **Combat:** Upbeat, action-oriented battle music (varies per enemy type)
- **Boss Fights:** Unique, intense boss themes per boss
- **Victory:** Triumphant short jingle
- **Defeat:** Sad/ominous short jingle

### 7.2 Sound Effects

- **UI:** Menu selections, button clicks (subtle)
- **Movement:** Footsteps on stone/dungeon tiles
- **Attacks:** Unique sounds per weapon type and ability (swish, boom, etc.)
- **Damage:** Hit impacts, enemy damage/death sounds
- **Loot:** Item pickup chime, gold collection sound
- **Status Effects:** Visual + audio cues for stun, poison, etc.

### 7.3 Audio Implementation

- Master volume control
- Separate sliders for: Music, SFX, UI Sounds
- Mute individual audio categories
- Fade-in/fade-out transitions between scenes

---

## 8. Visual Design & Art Style

### 8.1 Art Direction

- **Style:** Pixel art, 2D top-down isometric perspective
- **Resolution:** 16x16 or 32x32 tile size (adjustable for accessibility)
- **Color Palette:** Dark, gothic dungeon aesthetic with colorful character/item highlights
- **Animation:** Simple sprite animation (walk cycles, attack animation, idle)

### 8.2 Character Design

- **Warrior:** Heavy armor, sword/axe, tank-like appearance
- **Rogue:** Light leather, daggers, nimble appearance
- **Mage:** Robes, staff, mystical aura effects
- **Variants:** Skin options unlocked via meta-progression (different armor styles, weapon skins)

### 8.3 Enemy Design

- **Visual Distinction:** Each enemy type has unique pixel art sprite
- **Size:** Varies (small goblins, larger ogres, huge bosses)
- **Status Effects:** Visual indicators on enemies (glow effects for buffs/debuffs)

### 8.4 Environmental Tiles

- **Walkable:** Stone floor, carpet, grass
- **Obstacles:** Walls, pillars, crates
- **Interactive:** Doors, chests, stairs (clear visual indication)
- **Lighting:** Fog of war (unexplored areas darker) with progressive reveal

---

## 9. Technical Specifications

### 9.1 Engine & Platform

- **Engine:** Unity (2022 LTS or newer)
- **Language:** C#
- **Target Platform:** PC (Windows, macOS, Linux)
- **Minimum Requirements:**
  - OS: Windows 10, macOS 10.12, Ubuntu 18.04
  - Processor: Intel i5 / AMD Ryzen 5
  - RAM: 4 GB
  - Storage: 2 GB available
  - GPU: Integrated graphics (Intel HD 620 equivalent)

### 9.2 Game Systems Architecture

**Core Modules:**
1. **Dungeon Generation:** Procedural generation system (rooms, corridors, enemies)
2. **Combat System:** Turn-based combat state machine
3. **AI System:** Enemy behavior trees and pathfinding
4. **Inventory System:** Item management and equipping
5. **Progression System:** XP, leveling, meta-progression unlocks
6. **Persistence System:** Save/load game state and run statistics
7. **UI System:** Menu navigation, in-game HUD
8. **Audio System:** Music and sound effect management

### 9.3 Data Persistence

- **Local Save Files:** Player meta-progression stored locally (JSON format)
- **Run Statistics:** Separate file per run (optional cloud backup)
- **Settings:** User preferences (audio, graphics, controls)
- **No Online Features:** Fully single-player offline experience

---

## 10. Monetization Strategy

**Monetization Model:** Premium Purchase (One-time purchase, no DLC planned for initial release)

**Distribution:**
- Steam (primary)
- Epic Games Store (secondary)
- GOG (optional, DRM-free)

**Post-Launch Content (Optional):**
- Additional classes (Paladin, Berserker, etc.) - potential DLC
- New boss types and dungeon themes - free updates
- Cosmetic skins and item variants - potential cosmetic DLC

---

## 11. Launch Scope & Milestones

### 11.1 MVP (Minimum Viable Product) Scope

**In-Scope for Launch:**
- 3 character classes (Warrior, Rogue, Mage) fully playable
- Procedurally generated dungeon system (floors 1-25)
- Turn-based combat with basic abilities
- 5 enemy types with varied behaviors
- 5 unique boss encounters (Floors 5, 10, 15, 20, 25)
- Loot system (weapons, armor, accessories)
- Permadeath mechanics
- Meta-progression (20+ unlock achievements)
- UI/UX for menu, combat, inventory
- Pixel art visuals and animations
- Audio (music, sound effects)

**Out-of-Scope for Launch:**
- Online multiplayer / co-op
- Trading / social features
- Seasonal content / battle pass
- Advanced AI difficulty scaling
- Controller support (initial KB+M only)
- Mobile version

### 11.2 Development Timeline (Estimate)

- **Phase 1 (Weeks 1-4):** Core engine setup, dungeon generation, basic combat
- **Phase 2 (Weeks 5-8):** Class abilities, combat refinement, AI implementation
- **Phase 3 (Weeks 9-12):** Loot system, progression, UI framework
- **Phase 4 (Weeks 13-16):** Meta-progression, boss design, visual polish
- **Phase 5 (Weeks 17-20):** Audio, content variety, bug fixes, optimization
- **Phase 6 (Weeks 21-24):** QA testing, balance adjustments, final polish
- **Launch:** Week 24-25

---

## 12. Success Metrics & KPIs

### 12.1 Engagement Metrics

- **Average Session Length:** Target 60 minutes
- **Daily Active Users (DAU):** Track post-launch
- **Run Completion Rate:** % of players reaching Floor 10+ (target 40%)
- **Meta-Progression Unlock Rate:** % of players unlocking 50%+ available unlocks (target 30%)
- **Return Rate:** % of players returning after 7 days (target 50%)

### 12.2 Content Metrics

- **Unique Runs:** Track procedural variety (goal: no two runs feel identical)
- **Average Dungeon Depth:** Average floor reached before permadeath (target 8+)
- **Item Diversity:** Track which weapons/items are most commonly found/used

### 12.3 Monetization Metrics (Post-Launch)

- **Sales:** Units sold in first month
- **Review Score:** Target 80+ Metacritic
- **Player Retention:** % of players retained after 1 month, 3 months

---

## 13. Risk Analysis & Mitigation

| Risk | Impact | Mitigation |
|------|--------|-----------|
| Procedural generation creates unplayable dungeons | High | Implement strict generation validation; test extensively |
| Permadeath too punishing, players quit early | High | Balance difficulty curve; offer meta-progression rewards frequently |
| Combat feels slow or tedious | Medium | Fast turn resolution; implement "auto-combat" option for easier encounters |
| Limited content variety (repetitive gameplay) | Medium | Ensure procedural variation; plan post-launch content pipeline |
| Performance issues on target hardware | Medium | Optimize rendering; cap simultaneous enemies per encounter |
| Balance issues (one class overpowered) | Medium | Extensive playtesting; balance patches post-launch |

---

## 14. Post-Launch Support Plan

### 14.1 Day-1 Patches

- Critical bug fixes
- Balance adjustments based on closed beta feedback

### 14.2 Content Updates (Roadmap)

- **Month 1:** Bug fixes, balance patches
- **Month 2-3:** New enemy types, additional boss variety
- **Month 4:** Community feedback implementation, difficulty options
- **Month 6:** New class or major content expansion (DLC)

### 14.3 Community Engagement

- Regular dev blogs detailing patch notes and design decisions
- Discord community server for player feedback
- Monthly balance update announcements
- Community events (speedrun challenges, etc.)

---

## 15. Appendices

### 15.1 Glossary

- **AP:** Action Points, resource spent per turn in combat
- **Permadeath:** Permanent character death; run ends, all progress lost
- **Meta-Progression:** Permanent unlocks that persist between runs
- **Procedural Generation:** Computer-generated unique content (dungeons)
- **Roguelike:** Game with permadeath and procedural generation
- **Turn-Based Combat:** Combat where each entity takes turns (not real-time)
- **XP:** Experience points earned for defeating enemies and completing objectives

### 15.2 Example Boss Encounters

**Floor 5 Boss: Goblin King**
- 80 HP, 12 attack damage, 10% defense
- Abilities: Cleave (damage 2 enemies), Regenerate (heal 20 HP every 3 turns)
- Loot: Epic Sword or Epic Armor

**Floor 10 Boss: Skeleton Mage**
- 100 HP, 14 attack damage, 5% defense
- Abilities: Fireball (AOE damage), Bone Armor (gain 50% damage reduction for 2 turns)
- Loot: Epic Staff or Epic Ring

**Floor 15 Boss: Dragon Wyvern**
- 150 HP, 18 attack damage, 15% defense
- Abilities: Dragon Breath (AOE damage + stun), Tail Swipe (cleave damage), Flight (dodge attacks for 1 turn)
- Loot: Legendary Weapon or Legendary Armor

---

## 16. Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2026-03-21 | Initial PRD document creation |

---

**Document Status:** APPROVED FOR DEVELOPMENT

**Last Updated:** March 21, 2026

**Next Review:** Upon completion of Phase 1 (Week 4)
