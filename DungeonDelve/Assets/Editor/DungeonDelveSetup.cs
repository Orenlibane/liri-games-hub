// ╔══════════════════════════════════════════════════════════════════╗
// ║  DungeonDelve — Project Setup Script                            ║
// ║  Menu: DungeonDelve → 🚀 Setup Project                         ║
// ║                                                                  ║
// ║  Run this ONCE after importing all scripts.                     ║
// ║  It creates all scenes, GameObjects, and wires everything up.   ║
// ╚══════════════════════════════════════════════════════════════════╝

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DungeonDelve.Core;
using DungeonDelve.UI;

public static class DungeonDelveSetup
{
    private const string BootstrapPath = "Assets/Scenes/Bootstrap.unity";
    private const string MainMenuPath  = "Assets/Scenes/MainMenu.unity";
    private const string GameplayPath  = "Assets/Scenes/Gameplay.unity";

    // ── Entry point ──────────────────────────────────────────────────
    [MenuItem("DungeonDelve/🚀 Setup Project")]
    public static void SetupProject()
    {
        if (!EditorUtility.DisplayDialog(
            "DungeonDelve Setup",
            "This will create all scenes and project structure.\n\nAny existing scenes with these names will be overwritten.\n\nProceed?",
            "Yes, set it up!", "Cancel"))
            return;

        Debug.Log("=== DungeonDelve Setup Starting ===");

        EnsureFolders();
        CreateScriptableObjects();
        CreateBootstrapScene();
        CreateMainMenuScene();
        CreateGameplayScene();
        SetBuildSettings();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog(
            "Setup Complete! 🎉",
            "All scenes created and wired up.\n\n" +
            "Next: Press Play from the Bootstrap scene to see the main menu.\n\n" +
            "Find your ScriptableObjects in:\n" +
            "Assets/ScriptableObjects/Classes/\n" +
            "Assets/ScriptableObjects/Enemies/",
            "Let's go!");

        // Open Bootstrap so user is ready to hit Play.
        EditorSceneManager.OpenScene(BootstrapPath);
        Debug.Log("=== DungeonDelve Setup Complete ===");
    }

    // ── Folder creation ──────────────────────────────────────────────
    static void EnsureFolders()
    {
        string[] folders = {
            "Assets/Scripts/Core",
            "Assets/Scripts/Player",
            "Assets/Scripts/Enemies",
            "Assets/Scripts/Systems",
            "Assets/Scripts/UI",
            "Assets/Scripts/Utilities",
            "Assets/ScriptableObjects",
            "Assets/ScriptableObjects/Classes",
            "Assets/ScriptableObjects/Items",
            "Assets/ScriptableObjects/Enemies",
            "Assets/ScriptableObjects/MetaUnlocks",
            "Assets/Prefabs",
            "Assets/Scenes",
            "Assets/Art",
            "Assets/Audio",
        };

        foreach (var path in folders)
        {
            var parts = path.Split('/');
            var current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                var next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
        Debug.Log("[Setup] Folders ready.");
    }

    // ── ScriptableObject creation ────────────────────────────────────
    static void CreateScriptableObjects()
    {
        // Warrior class
        CreateSO<ClassDefinition>(
            "Assets/ScriptableObjects/Classes/Warrior.asset",
            so => {
                so.SetEditorValues(
                    className:   "Warrior",
                    description: "A sturdy fighter who excels in close combat and outlasting enemies.",
                    maxHP: 30, attack: 5, defense: 2, speed: 5, mana: 0,
                    abilityName: "Block",
                    abilityDesc: "Once per turn, reduce incoming damage by 50%.",
                    abilityCooldown: 3
                );
            });

        // Rogue class
        CreateSO<ClassDefinition>(
            "Assets/ScriptableObjects/Classes/Rogue.asset",
            so => {
                so.SetEditorValues(
                    className:   "Rogue",
                    description: "A nimble thief who deals heavy damage but can't take many hits.",
                    maxHP: 20, attack: 8, defense: 0, speed: 8, mana: 0,
                    abilityName: "Backstab",
                    abilityDesc: "Deal double damage when attacking an enemy that hasn't acted this turn.",
                    abilityCooldown: 0
                );
            });

        // Mage class
        CreateSO<ClassDefinition>(
            "Assets/ScriptableObjects/Classes/Mage.asset",
            so => {
                so.SetEditorValues(
                    className:   "Mage",
                    description: "A glass cannon who devastates enemies with spells but crumbles in melee.",
                    maxHP: 15, attack: 3, defense: 0, speed: 5, mana: 20,
                    abilityName: "Fireball",
                    abilityDesc: "Deal 12 damage to all enemies in a 3-tile radius. Costs 8 mana.",
                    abilityCooldown: 0
                );
            });

        // Goblin enemy
        CreateSO<EnemyConfig>(
            "Assets/ScriptableObjects/Enemies/Goblin.asset",
            so => {
                so.SetEditorValues(
                    enemyName: "Goblin",
                    maxHP: 8, attack: 3, defense: 0, speed: 4,
                    goldDrop: 2,
                    behaviour: EnemyBehaviourType.ChaseAndAttack,
                    detectionRange: 5, attackRange: 1
                );
            });

        // Skeleton enemy
        CreateSO<EnemyConfig>(
            "Assets/ScriptableObjects/Enemies/Skeleton.asset",
            so => {
                so.SetEditorValues(
                    enemyName: "Skeleton",
                    maxHP: 12, attack: 4, defense: 2, speed: 3,
                    goldDrop: 3,
                    behaviour: EnemyBehaviourType.ChaseAndAttack,
                    detectionRange: 4, attackRange: 1
                );
            });

        // Health Potion item
        CreateSO<ItemData>(
            "Assets/ScriptableObjects/Items/HealthPotion.asset",
            so => {
                so.SetEditorValues(
                    itemName: "Health Potion",
                    description: "Restores 15 HP when consumed.",
                    type: ItemType.Consumable,
                    rarity: ItemRarity.Common,
                    healAmount: 15
                );
            });

        // Iron Sword item
        CreateSO<ItemData>(
            "Assets/ScriptableObjects/Items/IronSword.asset",
            so => {
                so.SetEditorValues(
                    itemName: "Iron Sword",
                    description: "A dependable blade. +3 Attack.",
                    type: ItemType.Weapon,
                    rarity: ItemRarity.Common,
                    attackBonus: 3
                );
            });

        Debug.Log("[Setup] ScriptableObjects created.");
    }

    // ── Bootstrap scene ──────────────────────────────────────────────
    static void CreateBootstrapScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var gmGO = new GameObject("GameManager");
        gmGO.AddComponent<GameManager>();

        var tmGO = new GameObject("TurnManager");
        tmGO.AddComponent<TurnManager>();

        EditorSceneManager.SaveScene(scene, BootstrapPath);
        Debug.Log("[Setup] Bootstrap scene created.");
    }

    // ── MainMenu scene ───────────────────────────────────────────────
    static void CreateMainMenuScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // ── Canvas ───────────────────────────────────────────────────
        var canvasGO = new GameObject("Canvas");
        var canvas   = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight  = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();

        // ── Background panel ─────────────────────────────────────────
        var bgGO = CreateUIImage(canvasGO, "Background", new Color(0.05f, 0.05f, 0.1f, 1f));
        SetRect(bgGO, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero); // stretch full screen

        // ── Title ────────────────────────────────────────────────────
        var titleGO   = CreateUIText(canvasGO, "Title", "DUNGEON DELVE", 72, Color.white);
        SetRect(titleGO, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-400, -100), new Vector2(800, 100), new Vector2(0, 200));

        // ── Subtitle ──────────────────────────────────────────────────
        var subGO = CreateUIText(canvasGO, "Subtitle", "A Roguelike Dungeon Crawler", 28, new Color(0.7f, 0.7f, 0.7f));
        SetRect(subGO, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-300, -30), new Vector2(600, 50), new Vector2(0, 120));

        // ── Play button ───────────────────────────────────────────────
        var playGO  = CreateUIButton(canvasGO, "PlayButton", "▶  PLAY",
                                     new Color(0.2f, 0.6f, 0.2f), Color.white, 32);
        SetRect(playGO, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-150, -35), new Vector2(300, 70), new Vector2(0, 0));

        // ── Quit button ───────────────────────────────────────────────
        var quitGO  = CreateUIButton(canvasGO, "QuitButton", "✕  QUIT",
                                     new Color(0.5f, 0.1f, 0.1f), Color.white, 28);
        SetRect(quitGO, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-120, -30), new Vector2(240, 60), new Vector2(0, -90));

        // ── Version text ───────────────────────────────────────────────
        var verGO = CreateUIText(canvasGO, "Version", "v0.1 — Sprint 1", 18, new Color(0.4f, 0.4f, 0.4f));
        SetRect(verGO, new Vector2(0f, 0f), new Vector2(0f, 0f),
                new Vector2(0, 0), new Vector2(200, 30), new Vector2(100, 20));

        // ── MenuController ────────────────────────────────────────────
        var controllerGO = new GameObject("MenuController");
        controllerGO.transform.SetParent(canvasGO.transform);
        var controller = controllerGO.AddComponent<MainMenuController>();

        // Wire button references via SerializedObject
        var so = new SerializedObject(controller);
        so.FindProperty("_playButton").objectReferenceValue = playGO.GetComponent<Button>();
        so.FindProperty("_quitButton").objectReferenceValue = quitGO.GetComponent<Button>();

        // Wire default class (Warrior)
        var warrior = AssetDatabase.LoadAssetAtPath<ClassDefinition>(
            "Assets/ScriptableObjects/Classes/Warrior.asset");
        if (warrior != null)
            so.FindProperty("_defaultClass").objectReferenceValue = warrior;
        else
            Debug.LogWarning("[Setup] Warrior SO not found — assign it manually in MainMenuController.");

        so.ApplyModifiedProperties();

        // EventSystem
        var esGO = new GameObject("EventSystem");
        esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
        esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

        EditorSceneManager.SaveScene(scene, MainMenuPath);
        Debug.Log("[Setup] MainMenu scene created.");
    }

    // ── Gameplay scene ───────────────────────────────────────────────
    static void CreateGameplayScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // Managers (in this scene so they're accessible to dungeon objects)
        var managersGO = new GameObject("_Managers");
        var tmGO = new GameObject("TurnManager");
        tmGO.transform.SetParent(managersGO.transform);
        tmGO.AddComponent<TurnManager>();

        // Placeholder camera setup
        var cam = GameObject.Find("Main Camera");
        if (cam == null)
        {
            cam = new GameObject("Main Camera");
            cam.AddComponent<Camera>();
            cam.tag = "MainCamera";
        }
        cam.GetComponent<Camera>().orthographic     = true;
        cam.GetComponent<Camera>().orthographicSize = 7f;
        cam.GetComponent<Camera>().backgroundColor  = new Color(0.04f, 0.04f, 0.08f);
        cam.transform.position = new Vector3(0, 0, -10);

        // Placeholder floor label
        var placeholderGO = new GameObject("_PLACEHOLDER");
        var text = CreateUIText(placeholderGO, "Label",
            "Sprint 2 will generate the dungeon here.\nRun 'DungeonDelve → Generate Sprint 2' next.",
            24, Color.yellow);

        EditorSceneManager.SaveScene(scene, GameplayPath);
        Debug.Log("[Setup] Gameplay scene created.");
    }

    // ── Build settings ───────────────────────────────────────────────
    static void SetBuildSettings()
    {
        EditorBuildSettings.scenes = new[] {
            new EditorBuildSettingsScene(BootstrapPath, true),
            new EditorBuildSettingsScene(MainMenuPath,  true),
            new EditorBuildSettingsScene(GameplayPath,  true),
        };
        Debug.Log("[Setup] Build settings configured: Bootstrap(0) → MainMenu(1) → Gameplay(2)");
    }

    // ── UI Helpers ───────────────────────────────────────────────────
    static GameObject CreateUIImage(GameObject parent, string name, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var img = go.AddComponent<Image>();
        img.color = color;
        go.AddComponent<RectTransform>();
        return go;
    }

    static GameObject CreateUIText(GameObject parent, string name, string text, int fontSize, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var txt = go.AddComponent<Text>();
        txt.text      = text;
        txt.fontSize  = fontSize;
        txt.color     = color;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        go.AddComponent<RectTransform>();
        return go;
    }

    static GameObject CreateUIButton(GameObject parent, string name, string label,
                                     Color bgColor, Color textColor, int fontSize)
    {
        var go  = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var img = go.AddComponent<Image>();
        img.color = bgColor;
        var btn = go.AddComponent<Button>();
        var colors = btn.colors;
        colors.highlightedColor = Color.Lerp(bgColor, Color.white, 0.2f);
        colors.pressedColor     = Color.Lerp(bgColor, Color.black, 0.2f);
        btn.colors = colors;

        var textGO = new GameObject("Text");
        textGO.transform.SetParent(go.transform, false);
        var txt = textGO.AddComponent<Text>();
        txt.text      = label;
        txt.fontSize  = fontSize;
        txt.color     = textColor;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        var txtRect   = textGO.GetComponent<RectTransform>();
        txtRect.anchorMin = Vector2.zero;
        txtRect.anchorMax = Vector2.one;
        txtRect.offsetMin = Vector2.zero;
        txtRect.offsetMax = Vector2.zero;

        return go;
    }

    static void SetRect(GameObject go, Vector2 anchorMin, Vector2 anchorMax,
                        Vector2 offsetMin, Vector2 sizeDelta, Vector2 anchoredPos = default)
    {
        var rt         = go.GetComponent<RectTransform>();
        if (rt == null) rt = go.AddComponent<RectTransform>();
        rt.anchorMin      = anchorMin;
        rt.anchorMax      = anchorMax;
        rt.offsetMin      = offsetMin;
        rt.sizeDelta      = sizeDelta;
        rt.anchoredPosition = anchoredPos;
    }

    // ── Generic SO creation helper ───────────────────────────────────
    static void CreateSO<T>(string path, System.Action<T> configure) where T : ScriptableObject
    {
        var existing = AssetDatabase.LoadAssetAtPath<T>(path);
        if (existing != null)
        {
            configure(existing);
            EditorUtility.SetDirty(existing);
            return;
        }
        var so = ScriptableObject.CreateInstance<T>();
        configure(so);
        AssetDatabase.CreateAsset(so, path);
    }
}
