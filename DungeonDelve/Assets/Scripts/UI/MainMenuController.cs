using UnityEngine;
using UnityEngine.UI;
using DungeonDelve.Core;

namespace DungeonDelve.UI
{
    /// <summary>
    /// Drives the Main Menu: Play and Quit buttons.
    /// Warrior class is hardcoded for MVP — Sprint 3 adds class selection UI.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private Button         _playButton;
        [SerializeField] private Button         _quitButton;
        [SerializeField] private ClassDefinition _defaultClass;

        private void Start()
        {
            if (_playButton == null || _quitButton == null)
            {
                Debug.LogError("[MainMenuController] Buttons not assigned!", this);
                return;
            }
            _playButton.onClick.AddListener(OnPlayClicked);
            _quitButton.onClick.AddListener(OnQuitClicked);
        }

        private void OnDestroy()
        {
            _playButton?.onClick.RemoveListener(OnPlayClicked);
            _quitButton?.onClick.RemoveListener(OnQuitClicked);
        }

        private void OnPlayClicked()
        {
            if (_defaultClass == null)
            {
                Debug.LogError("[MainMenuController] No ClassDefinition assigned!", this);
                return;
            }
            if (GameManager.Instance == null)
            {
                Debug.LogError("[MainMenuController] GameManager not found. Did Bootstrap load first?", this);
                return;
            }
            GameManager.Instance.StartRun(_defaultClass);
        }

        private void OnQuitClicked()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
