---
name: unity-build
description: >
  Configures Unity build settings, generates a platform-specific build checklist,
  optimizes project for release, and guides through the publishing process for PC
  (Steam/itch.io), mobile (iOS/Android), or WebGL (itch.io/Newgrounds). Covers Player
  Settings, quality settings, compression, IL2CPP vs Mono, stripping, addressables,
  and platform-specific gotchas. Use this skill when the user is ready to build their
  game, wants to ship or publish, needs to configure Unity Player Settings, wants
  to optimize build size or performance, needs to set up for a specific platform,
  asks "how do I build my Unity game", "how to publish to Steam/itch.io",
  "reduce my build size", "optimize for mobile", "set up WebGL build",
  "release checklist", "build settings", or "how do I export my Unity game".
---

# Unity Build & Release Skill

You guide the developer through preparing a Unity project for release — from build configuration to platform-specific publishing. The goal is a correctly configured, optimized build with no last-minute surprises.

## Step 1: Determine target

Before generating any guidance, confirm:
1. **Primary platform**: PC (Windows/Mac/Linux), Mobile (iOS/Android), WebGL, or Console?
2. **Distribution channel**: Steam, itch.io, App Store, Google Play, or direct download?
3. **Unity version**: some options differ between LTS versions
4. **Scripting backend**: Mono (faster iteration) vs IL2CPP (smaller build, better performance, required for iOS)

---

## PC Build (Windows / Mac / Linux)

### Player Settings (Edit → Project Settings → Player)

```
Company Name: [YourStudio]
Product Name: [Game Title]
Version: 1.0.0  ← use semantic versioning
Default Icon: [assign 512×512 PNG]

Resolution and Presentation:
  Default Screen Width: 1920
  Default Screen Height: 1080
  Fullscreen Mode: Fullscreen Window (preferred over Exclusive Fullscreen)
  Resizable Window: true (if your UI supports it)
  Run in Background: true (for windowed mode)

Other Settings:
  Scripting Backend: IL2CPP (smaller, faster builds — worth the longer compile time)
  Api Compatibility Level: .NET Standard 2.1
  Allow 'unsafe' Code: only if needed
  Strip Engine Code: true (Managed Stripping Level: Medium to start)
  Optimization → Use incremental GC: true
```

### Quality Settings (Edit → Project Settings → Quality)
```
For a solo indie game, typically 2-3 quality levels:
  Low:    VSync: Off, Shadow Distance: 0, Texture Quality: Half Res
  Medium: VSync: Count 1, Shadow Distance: 40, Texture Quality: Full
  High:   VSync: Count 1, Shadow Distance: 80, Texture Quality: Full

Set default quality per platform:
  Windows: Medium or High
  Mac: High (Macs are generally powerful relative to target users)
```

### Build steps
```
1. File → Build Settings
2. Add all scenes in the correct order:
   - Index 0: Bootstrap (loads first)
   - Index 1: MainMenu
   - Index 2+: Gameplay scenes
3. Platform: PC, Mac & Linux Standalone
4. Target Platform: Windows (or select per build)
5. Architecture: x86_64
6. Create button: click "Build" (not "Build and Run" for release)
   → Output to: Builds/Windows/v1.0.0/
```

### PC optimization checklist
```
□ All textures: Max Size appropriate (2048 for most, 512 for small UI)
□ All audio: Compression format set (Vorbis for music, ADPCM for short SFX)
□ Texture atlases: Use Sprite Atlas for 2D games (reduces draw calls)
□ No "Resources" folder (prefer Addressables or direct references)
□ Physics layers: collision matrix configured (disable unnecessary layer pairs)
□ Shadow casting: disabled on sprites and 2D elements
□ Batching: static batching enabled for non-moving world objects (3D)
□ Profiler: run a profile pass and address any > 16ms frame issues
□ No Debug.Log calls in release (wrap in #if UNITY_EDITOR or use a log manager)
```

---

## Mobile Build (Android / iOS)

### Android Player Settings
```
Other Settings:
  Package Name: com.yourstudio.gametitle  ← reverse domain format
  Version: 1.0.0
  Bundle Version Code: 1  ← integer, increment with every upload
  Minimum API Level: Android 8.0 (API 26)  ← good coverage
  Target API Level: Latest installed
  Scripting Backend: IL2CPP (required for 64-bit)
  Target Architectures: ARM64 (required for Play Store), optionally also ARMv7
  Internet Access: Not Required (unless using online features)

Publishing Settings:
  Keystore: Create a keystore file! Store it safely outside the project.
  (Without a keystore you can't update your app on the Play Store)
```

### iOS Player Settings
```
Bundle Identifier: com.yourstudio.gametitle
Version: 1.0.0
Build: 1  ← increment with every TestFlight upload
Minimum iOS Version: 14.0 (good coverage as of 2024)
Scripting Backend: IL2CPP (required)
Architecture: ARM64

Signing (in Xcode after Unity build):
  Team: [Your Apple Developer Team]
  Provisioning Profile: App Store distribution profile
```

### Mobile-specific optimization
```
□ Texture compression:
   Android: ASTC (modern devices) — set in Build Settings → Texture Compression
   iOS: ASTC (all modern iOS devices support it)

□ Reduce build size:
   - Strip unused Unity features in Player Settings → Other Settings → Stripping
   - Remove unused packages from Package Manager
   - Use texture compression aggressively

□ Target frame rate:
   Application.targetFrameRate = 60;  ← set in Bootstrap
   QualitySettings.vSyncCount = 0;    ← disable VSync (let targetFrameRate control it)

□ Battery: disable features when app is backgrounded
   (subscribe to Application.onBeforeRender and Application.focusChanged)

□ Screen sleep: Screen.sleepTimeout = SleepTimeout.NeverSleep;
   (for games — re-enable for casual games to respect OS settings)

□ Touch input: Use Unity new Input System with touch action maps,
   or use Input.GetTouch() for simple cases
```

---

## WebGL Build

### WebGL Player Settings
```
Resolution and Presentation:
  Default Canvas Width: 960
  Default Canvas Height: 540
  Run in Background: true (important — WebGL pauses by default when tab loses focus)
  WebGL Template: Default or Minimal
    (Minimal recommended for itch.io embedding)

Publishing Settings:
  Compression Format: Brotli (best compression, requires server support)
                   or Gzip (wider server compatibility)
                   or Disabled (if hosting on itch.io — it handles compression)
  Data Caching: true

Other Settings:
  Scripting Backend: IL2CPP (WebGL only supports this)
  Strip Engine Code: true
  Exception Support: None (for release — smaller build, faster; use Explicitly Thrown for debugging)
```

### WebGL build size reduction
WebGL builds are large. Common culprits and fixes:
```
□ Audio: switch to Vorbis compression, reduce quality to 70%
□ Textures: use texture compression (ASTC/DXT)
□ Strip unused packages from Package Manager
□ Use Managed Stripping Level: High (test carefully — can break reflection-based code)
□ Target build size goal: < 30MB for good load times on itch.io
```

### Hosting on itch.io
```
1. Build to: Builds/WebGL/v1.0.0/
2. Zip the Build folder contents (NOT the folder itself — zip index.html and the Build/ subfolder)
3. itch.io → Create project → Kind of project: HTML
4. Upload the .zip
5. Set embed size to match your canvas (960×540 or 1920×1080)
6. Check "This file will be played in the browser"
7. Disable "SharedArrayBuffer" requirement if possible (limits COOP/COEP headers)
```

### Hosting on Steam (PC)
```
Prerequisites: Steamworks SDK, Steamworks account, $100 app fee

Steps:
1. Create app on Steamworks (store.steampowered.com/steamworks/)
2. Download Steamworks SDK
3. For Unity: use Steamworks.NET plugin (github.com/rlabrecque/Steamworks.NET)
4. Add Steam app ID:
   - Create steam_appid.txt in project root with your App ID
5. Upload builds via SteamPipe (Steamworks → steamcmd or Steamworks SDK)
6. Set up store page, screenshots, capsule art
7. Achievements: implement via Steamworks.NET SteamUserStats
8. Cloud saves: implement via ISteamRemoteStorage
```

---

## Pre-release checklist (all platforms)

```
## Pre-Release Checklist

### Functionality
□ Complete the game from start to finish in a single sitting (find the critical path bugs)
□ Test with a fresh save (delete all PlayerPrefs/save data before testing)
□ Test on minimum spec hardware
□ All scenes are in Build Settings
□ No "development build" checkbox left on accidentally
□ No debug logging in release (use a conditional log wrapper)
□ No test scenes or test GameObjects left in production builds

### Performance
□ Profile with Profiler — no single frame > 33ms (30fps) or > 16ms (60fps)
□ Memory profiler — no obvious leaks over a 10-minute play session
□ Build size is reasonable for platform

### Polish
□ Application icon set (all required sizes per platform)
□ Splash screen configured (or Unity splash removed if you have a Pro license)
□ Loading screens/transitions between scenes
□ Quit to desktop works (Application.Quit() on PC, back button on Android)
□ Game pauses when app loses focus

### Platform requirements
□ PC: Installer or zip with README
□ Android: 64-bit builds, keystore secured, target API level current
□ iOS: Privacy usage descriptions in Info.plist (camera, mic, photos if used)
□ WebGL: Tested in Chrome and Firefox, tested on mobile browser

### First 10 minutes experience
□ First run experience is clear — player knows what to do
□ Controls are explained or intuitive
□ First 3 minutes are polished and representative
□ Game saves early so players don't lose progress
```

---

## Output format

After generating build guidance, summarize:

1. **Platform-specific commands** — key settings to change
2. **Build output location** — where files land
3. **Distribution steps** — how to upload/submit
4. **Common pitfalls** — the things that always cause last-minute scrambles
5. **Version management** — how to track versions going forward

Example:
```
📦 Your Windows build will output to: Builds/Windows/v1.0.0/
   Zip the entire folder → upload to itch.io or SteamPipe

⚠️ Common pitfalls:
   - Forgetting to set scripting backend to IL2CPP before final build
   - Not testing a completely fresh installation (install from zip on a different machine)
   - Missing the keystore file when updating Android app (keep it backed up!)
   - WebGL audio doesn't play until user interaction (browser security policy)

📋 Version scheme going forward:
   v1.0.0 = launch
   v1.0.1 = hotfixes
   v1.1.0 = content updates
   Bundle Version Code: increment by 1 with every upload (Android/iOS)
```
