using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Ouiki.UI
{
    public class PauseMenuManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Canvas pauseMenuCanvas;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button quitButton;

        private bool isPaused = false;
        private InputAction pauseAction;

        private void Awake()
        {
            if (pauseMenuCanvas != null)
                pauseMenuCanvas.gameObject.SetActive(false);

            if (resumeButton != null)
                resumeButton.onClick.AddListener(ResumeGame);

            if (restartButton != null)
                restartButton.onClick.AddListener(RestartGame);

            if (quitButton != null)
                quitButton.onClick.AddListener(QuitGame);

            // Create and enable an InputAction for ESC key (stand-alone, no PlayerInput)
            pauseAction = new InputAction(type: InputActionType.Button, binding: "<Keyboard>/escape");
            pauseAction.performed += OnPausePerformed;
            pauseAction.Enable();
        }

        private void OnDestroy()
        {
            if (pauseAction != null)
            {
                pauseAction.performed -= OnPausePerformed;
                pauseAction.Disable();
            }
        }

        private void OnPausePerformed(InputAction.CallbackContext ctx)
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }

        public void PauseGame()
        {
            if (isPaused) return;
            isPaused = true;
            Time.timeScale = 0f;
            if (pauseMenuCanvas != null)
                pauseMenuCanvas.gameObject.SetActive(true);

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        public void ResumeGame()
        {
            if (!isPaused) return;
            isPaused = false;
            Time.timeScale = 1f;
            if (pauseMenuCanvas != null)
                pauseMenuCanvas.gameObject.SetActive(false);

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        public void RestartGame()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void QuitGame()
        {
            Time.timeScale = 1f;
            Application.Quit();
        }
    }
}