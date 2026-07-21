---
name: unity-art-spec
description: >
  Generates a complete art and asset specification for a Unity game — sprite dimensions,
  animation frame counts and timing, naming conventions, texture atlas layout, audio
  format requirements, and import settings. Output is a reference document that can be
  handed to an artist, used when generating AI art, or followed when sourcing assets
  from asset stores. Use this skill when the user needs to define what art assets to
  create, wants a spec before commissioning or generating art, needs naming conventions
  for sprites and audio, wants to know sprite sizes for their resolution, needs animation
  frame breakdowns, asks "what size should my sprites be", "how many frames for X
  animation", "what format for audio", "art requirements", "asset spec", "what art do
  I need to make", or wants to organize their asset pipeline. Also trigger when
  the user wants to use AI art tools (Midjourney, DALL-E, Stable Diffusion) to create
  game art and needs a spec to prompt them correctly.
---

# Unity Art Specification Skill

You generate a complete asset specification document for a Unity game. This spec is the bridge between the game design (PRD) and the actual art creation — whether done by hand, commissioned, or generated with AI tools.

## Before generating

Read the PRD if available, specifically:
- **Section 1.4** — Art style and color palette direction
- **Section 1.3** — Target platform and resolution
- **Section 2** — Game systems (determines what characters, items, environments are needed)
- **Section 3.7** — Render pipeline and sprite settings (PPU, etc.)

Ask if not specified:
1. **Art style**: pixel art, hand-drawn, vector, 3D low-poly, photo-realistic?
2. **Target resolution**: 1080p, 1440p, 4K, mobile (various)?
3. **Pixels per unit (PPU)**: how many pixels = 1 Unity unit? (16, 32, 64, 100 are common)
4. **Is this 2D or 3D**?

---

## Pixel Art Specification (2D games)

### Base unit and resolution math

The relationship between PPU, sprite size, and screen real estate:

```
If PPU = 16 and a character is 1.5 units tall:
→ Sprite height = 16 × 1.5 = 24 pixels

At 1920×1080 with orthographic camera size 5:
→ Screen height = 10 Unity units = 10 × 16 = 160 pixels visible
→ Render at 1× pixel scale, upscale via camera or canvas
```

Recommended PPU values by style:
- Ultra-low (NES-like): PPU = 8, characters ~16×16px
- Classic (16-bit-like): PPU = 16, characters ~16×24 or 32×32px
- Detailed pixel art: PPU = 32, characters ~32×48 or 48×64px
- Hi-bit pixel art: PPU = 64, characters ~64×96px

### Sprite size table

Generate a table for every entity type:

| Entity | Sprite Size | PPU | Notes |
|--------|------------|-----|-------|
| Player (idle) | 32×48px | 32 | 1 unit wide, 1.5 units tall |
| Player (attack, wider) | 64×48px | 32 | Extra horizontal space for swing |
| Small enemy (goblin) | 24×32px | 32 | Shorter than player |
| Large enemy (boss) | 96×96px | 32 | 3×3 units |
| Floor tile | 32×32px | 32 | 1×1 unit tileable |
| Wall tile | 32×32px | 32 | Match floor tile size |
| Door | 32×64px | 32 | 1×2 units |
| Small item (potion) | 16×16px | 32 | 0.5×0.5 units |
| UI health icon | 24×24px | 1 (UI) | Screen-space, not world-space |

### Animation frame breakdown

For every animated entity, specify each animation:

```
## Player Animations

### Idle
- Frames: 4
- FPS: 8
- Loop: yes
- Notes: Subtle breathing/bobbing. Frame 0 = neutral, Frame 2 = slightly down.

### Walk
- Frames: 8
- FPS: 12
- Loop: yes
- Notes: Full walk cycle. Feet should return to start on frame 8→0 loop.

### Run
- Frames: 8
- FPS: 16
- Loop: yes
- Notes: More body lean than walk. Arms pump opposite to legs.

### Attack (sword swing)
- Frames: 6
- FPS: 18
- Loop: no (play once, return to idle)
- Notes:
  - Frame 0–1: Wind-up (hold back)
  - Frame 2–4: Swing (hitbox active on frame 2)
  - Frame 5: Recovery
- Sprite canvas: 64×48 (wider for swing arc)

### Jump
- Frames: 4
- FPS: 10
- Loop: no
- Notes: Frame 0=crouch, 1=launch, 2=peak, 3=fall. Hold frame 3 during fall.

### Land
- Frames: 3
- FPS: 14
- Loop: no (snap back to idle after)

### Hurt
- Frames: 3
- FPS: 12
- Loop: no
- Notes: Flash white on frame 0 (handled by shader, not art)

### Death
- Frames: 8
- FPS: 10
- Loop: no
- Notes: Ends on final frame (stays visible until despawn)
```

### Tileset specification

```
## Dungeon Tileset

### Tile size: 32×32px
### Atlas layout: 16 tiles wide, as many rows as needed

### Required tiles:
Floor tiles (variations for visual interest):
  - Floor_Plain (4 variants, slight random noise)
  - Floor_Crack (2 variants)
  - Floor_Stain (2 variants)
  - Floor_Rubble (edge decoration, not walkable)

Wall tiles (autotile-friendly, Wang/blob tile set):
  - Wall_Solid (interior fill)
  - Wall_Top (exposed top edge)
  - Wall_Corner_OuterTL, TR, BL, BR
  - Wall_Corner_InnerTL, TR, BL, BR
  - Wall_Edge_H, Wall_Edge_V

Props (separate sprites, not tiles):
  - Torch (animated, 4 frames)
  - Barrel
  - Chest_Closed, Chest_Open
  - Door_Closed, Door_Open (animated, 4 frames)
  - Stalagmite (2 variants)
```

---

## Vector / Hand-drawn (UI and non-pixel)

For UI elements and non-pixel art styles:

```
## UI Art Specification

### Resolution: Design at 1920×1080, export at 2× for high-DPI
### Format: PNG with transparency

### Required UI assets:
Buttons:
  - Button_Primary: 200×60px (9-slice, 12px border)
  - Button_Secondary: 200×60px (9-slice, 12px border)
  - Button_Icon: 48×48px (circular, no text)

HUD elements:
  - HealthBar_BG: 200×20px (9-slice, 4px border)
  - HealthBar_Fill: 192×12px (solid, tiled horizontally)
  - ManaBar_BG: 200×16px
  - ManaBar_Fill: 192×8px
  - Portrait_Frame: 96×96px (frame only, PNG with hole in center)
  - XP_Bar_BG, XP_Bar_Fill: same as health bar

Icons (all 48×48px with 4px padding from edge):
  - Icon_Sword, Icon_Shield, Icon_Bow, Icon_Staff
  - Icon_Potion_Health, Icon_Potion_Mana
  - Icon_Gold, Icon_Key, Icon_Gem
  - Icon_Skill_[SkillName] (one per player skill)
```

---

## Audio specification

```
## Audio Assets

### Format requirements:
- SFX: WAV, 44.1kHz, 16-bit, mono
- Music: OGG Vorbis, 44.1kHz, stereo
- Voice (if any): WAV, 44.1kHz, 16-bit, mono

### Import settings (Unity):
- SFX: Load Type = Decompress on Load (if < 200KB) or Compressed in Memory
- Music: Load Type = Streaming
- All SFX: Preload Audio Data = true

### Required SFX:
Player:
  - sfx_player_footstep_stone (2–3 variants, randomize pitch ±5%)
  - sfx_player_footstep_grass
  - sfx_player_attack_swing (2 variants)
  - sfx_player_attack_hit_flesh
  - sfx_player_attack_hit_miss (sword whoosh)
  - sfx_player_hurt (2 variants)
  - sfx_player_death
  - sfx_player_jump
  - sfx_player_land
  - sfx_player_pickup_item

Enemies:
  - sfx_enemy_goblin_alert
  - sfx_enemy_goblin_attack
  - sfx_enemy_goblin_hurt
  - sfx_enemy_goblin_death
  - sfx_enemy_skeleton_bones (ambient, looped)
  - [repeat pattern for each enemy type]

Environment:
  - sfx_env_torch_loop (looped)
  - sfx_env_door_open
  - sfx_env_door_close
  - sfx_env_chest_open
  - sfx_env_level_complete

UI:
  - sfx_ui_button_hover
  - sfx_ui_button_click
  - sfx_ui_menu_open
  - sfx_ui_menu_close
  - sfx_ui_item_equip
  - sfx_ui_levelup

### Music tracks:
  - music_mainmenu (loop, ~2 min)
  - music_dungeon_tense (loop, ~3 min, plays during normal dungeon)
  - music_dungeon_combat (loop, ~2 min, plays when enemies are aware)
  - music_boss (loop, ~4 min)
  - music_gameover (stinger, no loop, ~5 sec)
  - music_levelcomplete (stinger, ~3 sec)
```

---

## Naming conventions

All assets should follow a consistent naming scheme:

```
## Naming Convention

### Sprites
[type]_[subject]_[variant/state]
Examples:
  spr_player_idle
  spr_player_run
  spr_enemy_goblin_attack
  spr_tile_floor_plain
  spr_tile_wall_top
  spr_item_potion_health
  spr_ui_button_primary

### Audio
[type]_[category]_[description]_[variant]
Examples:
  sfx_player_footstep_stone_01
  sfx_player_footstep_stone_02
  sfx_ui_button_click
  music_dungeon_tense

### Animations
[subject]_[action]
Examples:
  player_idle
  player_run
  goblin_attack

### Prefabs
[subject]_[variant] (PascalCase)
Examples:
  Player
  Enemy_Goblin
  Enemy_SkeletonWarrior
  Item_HealthPotion
  Tile_DungeonFloor
```

---

## AI art generation prompts

If the user is generating assets with AI tools, provide prompt templates:

```
## Midjourney Prompts for Pixel Art Assets

### Style reference prompt (use as suffix on all):
"--style pixel art, 32x32 pixel sprite, flat color, black outline,
 game asset, transparent background, centered, single frame --ar 1:1 --v 6"

### Player character (idle):
"small dungeon adventurer, holding sword, wearing leather armor,
 pixel art game sprite, front-facing" [+ style suffix]

### Goblin enemy:
"small green goblin monster, hunched posture, glowing red eyes,
 holding crude wooden club, pixel art game sprite" [+ style suffix]

### Floor tile:
"stone dungeon floor tile, seamless tiling, grey cobblestone,
 subtle cracks, top-down view, pixel art" [+ style suffix]
```

---

## Output format

Deliver the spec as a markdown file the user can save and reference throughout development. End with:

```
📁 Save spec to: Assets/_Docs/ArtSpec.md (or share with artist)

📊 Total asset count:
   Sprites: ~[N] (including animation frames)
   Audio SFX: ~[N]
   Music tracks: [N]
   UI assets: ~[N]

⏱️ Estimated art time (solo, basic quality):
   Sprites: ~[X] hours
   Audio: can source from freesound.org / itch.io for a solo project

🎨 Recommended free asset sources for prototyping:
   - itch.io (search your game genre + "free")
   - kenney.nl (high quality, public domain)
   - freesound.org (audio)
   - opengameart.org

➡️ Next step: Use unity-scene skill to start building the scene structure,
   then use unity-script to generate player and enemy scripts.
```
