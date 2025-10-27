using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

namespace Ouiki.UI
{
    public class GameOverCanvasManager : MonoBehaviour
    {
        public static GameOverCanvasManager Instance { get; private set; }

        [Header("UI References")]
        [SerializeField] private Canvas gameOverCanvas;
        [SerializeField] private TMP_Text gameOverTitle;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button quitButton;

        public event Action OnRestartClicked;
        public event Action OnQuitClicked;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else if (Instance != this)
                Destroy(gameObject);

            if (gameOverCanvas != null)
                gameOverCanvas.gameObject.SetActive(false);

            if (restartButton != null)
                restartButton.onClick.AddListener(HandleRestartClicked);

            if (quitButton != null)
                quitButton.onClick.AddListener(HandleQuitClicked);
        }

        /// <summary>
        /// Shows the game over screen with custom messages.
        /// </summary>
        public void ShowGameOver(string title = "Game Over")
        {
            if (gameOverCanvas != null)
                gameOverCanvas.gameObject.SetActive(true);

            if (gameOverTitle != null)
                gameOverTitle.text = title;

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        public void HideGameOver()
        {
            if (gameOverCanvas != null)
                gameOverCanvas.gameObject.SetActive(false);

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void HandleRestartClicked()
        {
            OnRestartClicked?.Invoke();
            RestartGame();
        }

        private void HandleQuitClicked()
        {
            OnQuitClicked?.Invoke();
            QuitGame();
        }
        public void RestartGame()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void QuitGame()
        {
            Application.Quit();
        }
    }
}