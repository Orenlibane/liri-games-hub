using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DungeonDelve.Core
{
    /// <summary>
    /// Persistent singleton that owns run state: current floor, player alive status,
    /// selected class, and scene transitions. Lives in the Bootstrap scene and survives
    /// all scene loads via DontDestroyOnLoad.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        // ── Singleton ────────────────────────────────────────────────────
        public static GameManager Instance { get; private set; }

        // ── Inspector Fields ─────────────────────────────────────────────
        [Header("Scene Names")]
        [SerializeField] private string _mainMenuSceneName  = "MainMenu";
        [SerializeField] private string _gameplaySceneName  = "Gameplay";
        [SerializeField] private string _bootstrapSceneName = "Bootstrap";

        [Header("Debug")]
        [SerializeField] private bool _debugLogging = true;

        // ── Events ───────────────────────────────────────────────────────
        /// <summary>Fired when the current floor number changes.</summary>
        public event Action<int> OnFloorChanged;

        /// <summary>Fired when a new run starts (player just entered the dungeon).</summary>
        public event Action OnRunStarted;

        /// <summary>Fired when the current run ends — either by death or escape.</summary>
        public event Action<bool> OnRunEnded; // bool: playerWon

        // ── Public State ─────────────────────────────────────────────────
        public int          CurrentFloor    { get; private set; }
        public bool         IsRunActive     { get; private set; }
        public GameState    CurrentState    { get; private set; }
        public ClassDefinition SelectedClass { get; private set; }

        // ── Unity Callbacks ──────────────────────────────────────────────
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SetState(GameState.Initializing);
        }

        private void Start()
        {
            // Bootstrap scene's only job: initialize managers, then go to MainMenu.
            SceneManager.LoadScene(_mainMenuSceneName);
            SetState(GameState.MainMenu);
        }

        // ── Public API ───────────────────────────────────────────────────

        /// <summary>
        /// Start a new run with the given class. Called by MainMenuController when
        /// the player clicks Play.
        /// </summary>
        public void StartRun(ClassDefinition selectedClass)
        {
            if (selectedClass == null)
            {
                Debug.LogError("[GameManager] StartRun called with null ClassDefinition!");
                return;
            }

            SelectedClass  = selectedClass;
            CurrentFloor   = 1;
            IsRunActive    = true;
            SetState(GameState.InDungeon);

            Log($"Run started — Class: {selectedClass.ClassName}, Floor: {CurrentFloor}");
            OnRunStarted?.Invoke();
            SceneManager.LoadScene(_gameplaySceneName);
        }

        /// <summary>
        /// Advance to the next dungeon floor. Called by the dungeon system when the
        /// player reaches the stairs.
        /// </summary>
        public void AdvanceFloor()
        {
            if (!IsRunActive)
            {
                Debug.LogWarning("[GameManager] AdvanceFloor called but no run is active.");
                return;
            }

            CurrentFloor++;
            Log($"Advanced to floor {CurrentFloor}");
            OnFloorChanged?.Invoke(CurrentFloor);

            // Reload the Gameplay scene to regenerate the dungeon for the new floor.
            SceneManager.LoadScene(_gameplaySceneName);
        }

        /// <summary>
        /// End the current run. Called when the player dies or (eventually) wins.
        /// </summary>
        /// <param name="playerWon">True if the player completed the dungeon.</param>
        public void EndRun(bool playerWon)
        {
            if (!IsRunActive)
            {
                Debug.LogWarning("[GameManager] EndRun called but no run is active.");
                return;
            }

            IsRunActive = false;
            Log($"Run ended — Won: {playerWon}, Reached floor: {CurrentFloor}");

            OnRunEnded?.Invoke(playerWon);

            // ⚠️ Assumption: post-run screen is handled in MainMenu for MVP.
            // A dedicated GameOver scene can replace this in Sprint 5.
            SetState(GameState.MainMenu);
            SceneManager.LoadScene(_mainMenuSceneName);
        }

        /// <summary>
        /// Return to the main menu without ending a run (e.g. quit from pause menu).
        /// </summary>
        public void ReturnToMainMenu()
        {
            if (IsRunActive)
                EndRun(playerWon: false);
            else
            {
                SetState(GameState.MainMenu);
                SceneManager.LoadScene(_mainMenuSceneName);
            }
        }

        // ── Private Helpers ──────────────────────────────────────────────
        private void SetState(GameState newState)
        {
            CurrentState = newState;
            Log($"State → {newState}");
        }

        private void Log(string message)
        {
            if (_debugLogging)
                Debug.Log($"[GameManager] {message}");
        }
    }

    /// <summary>Top-level game states that drive scene and system behavior.</summary>
    public enum GameState
    {
        Initializing,
        MainMenu,
        InDungeon,
        Paused,
        GameOver
    }
}
