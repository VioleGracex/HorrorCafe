using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Ouiki.UI
{
    public class ReadableCanvasManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Canvas instructionsCanvas;
        [SerializeField] private TMP_Text uiTitle;
        [SerializeField] private TMP_Text uiText;
        [SerializeField] private Button closeButton;

        private System.Action onCloseCallback;

        private void Awake()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(CloseReadable);
        }

        public void ShowReadable(string title, string text, System.Action onClose = null)
        {
            if (instructionsCanvas != null)
                instructionsCanvas.gameObject.SetActive(true);
            if (uiTitle != null)
                uiTitle.text = title;
            if (uiText != null)
                uiText.text = text;
            onCloseCallback = onClose;

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        public void CloseReadable()
        {
            if (instructionsCanvas != null)
                instructionsCanvas.gameObject.SetActive(false);
            onCloseCallback?.Invoke();
            onCloseCallback = null;

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}