using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DungeonDelve.UI
{
    /// <summary>
    /// Controls the Main Menu scene: handles Play and Quit button clicks,
    /// communicates the selected class to GameManager, and loads the Gameplay scene.
    ///
    /// ⚠️ Assumption: Class selection UI is not implemented in Sprint 1.
    /// The Warrior class is hardcoded as the default for MVP. Class select
    /// UI will be added in Sprint 3.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        // ── Inspector Fields ─────────────────────────────────────────────
        [Header("UI References")]
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _quitButton;

        [Tooltip("Text shown when returning from a failed run. Leave empty to hide.")]
        [SerializeField] private TextMeshProUGUI _lastRunResultText;

        [Header("Class Selection (MVP: hardcoded to Warrior)")]
        [Tooltip("Warrior ClassDefinition ScriptableObject — drag in from Assets/ScriptableObjects/.")]
        [SerializeField] private Core.ClassDefinition _defaultClass;

        // ── Unity Callbacks ──────────────────────────────────────────────
        private void Start()
        {
            if (_playButton == null || _quitButton == null)
            {
                Debug.LogError("[MainMenuController] Play or Quit button not assigned!", this);
                return;
            }

            _playButton.onClick.AddListener(OnPlayClicked);
            _quitButton.onClick.AddListener(OnQuitClicked);

            ShowLastRunResult();
        }

        private void OnDestroy()
        {
            // Always clean up listeners to avoid GC issues if the scene reloads.
            _playButton?.onClick.RemoveListener(OnPlayClicked);
            _quitButton?.onClick.RemoveListener(OnQuitClicked);
        }

        // ── Private Methods ──────────────────────────────────────────────
        private void OnPlayClicked()
        {
            if (_defaultClass == null)
            {
                Debug.LogError("[MainMenuController] No ClassDefinition assigned! " +
                               "Drag the Warrior ClassDefinition SO into the _defaultClass field.", this);
                return;
            }

            if (Core.GameManager.Instance == null)
            {
                Debug.LogError("[MainMenuController] GameManager.Instance is null. " +
                               "Is the Bootstrap scene missing from Build Settings?", this);
                return;
            }

            Core.GameManager.Instance.StartRun(_defaultClass);
        }

        private void OnQuitClicked()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void ShowLastRunResult()
        {
            if (_lastRunResultText == null) return;

            // ⚠️ Assumption: last run stats stored in GameManager.
            // For now just show a placeholder or hide the text.
            // Sprint 5 will wire in real run stats (floors reached, enemies killed).
            _lastRunResultText.gameObject.SetActive(false);
        }
    }
}
