using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using Ouiki.Interfaces;

namespace Ouiki.FPS
{
    public class PlayerUIController : MonoBehaviour
    {
        public Image crosshair;
        public Image staminaBar;
        public Image staminaBarBG;
        public CanvasGroup staminaBarCG;
        public GameObject knockedOutOverlay;

        [Header("Item Interaction UI")]
        public TextMeshProUGUI itemLabelText;
        public TextMeshProUGUI itemActionText;

        public bool useStaminaBar = true;
        public bool hideBarWhenFull = true;
        public float barShowTimeAfterFull = 1.5f;

        [Header("Crosshair Interact Colors")]
        public Color crosshairDefaultColor = Color.white;
        public Color crosshairInteractColor = Color.yellow;
        public float crosshairColorTweenTime = 0.15f;

        private PlayerManager manager;
        private PlayerStateManager state;
        private CooldownManager cooldown;
        private PlayerInteractionController interactionController;
        private bool _initialized = false;
        private float staminaBarWidth;
        private float staminaBarHeight;
        private float lastBarPercent = 1f;
        private bool barJustFilled = false;
        private float barFillHideTimer = 0f;

        public Color fullColor = Color.green;
        public Color regenColor = new Color(0f, 1f, 0f, 0.3f);

        void Start()
        {
            // Optionally clear item texts at start
            if (itemLabelText) itemLabelText.text = "";
            if (itemActionText) itemActionText.text = "";
        }

        public void Init(PlayerManager mgr)
        {
            manager = mgr;
            state = manager.stateManager;
            cooldown = manager.cooldownManager;
            interactionController = mgr.interactionController;
            _initialized = true;

            if (staminaBar != null)
            {
                staminaBarWidth = staminaBar.rectTransform.sizeDelta.x;
                staminaBarHeight = staminaBar.rectTransform.sizeDelta.y;
            }
        }

        void Update()
        {
            if (!_initialized) return;
            UpdateCrosshair();
            UpdateStaminaBar();
            UpdateKnockedOutOverlay();
            UpdateItemLabels();
        }

        void UpdateCrosshair()
        {
            if (crosshair == null) return;
            crosshair.enabled = !state.IsKnockedOut;

            bool isLookingAtInteractable = interactionController != null && interactionController.IsLookingAtInteractable;
            Color targetColor = isLookingAtInteractable ? crosshairInteractColor : crosshairDefaultColor;
            if (crosshair.color != targetColor)
            {
                crosshair.DOColor(targetColor, crosshairColorTweenTime);
            }
        }

        void UpdateStaminaBar()
        {
            if (!useStaminaBar || staminaBar == null || cooldown == null) return;

            float staminaDuration = cooldown.staminaDuration;
            float staminaRemaining = cooldown.staminaRemaining;
            bool isOnCooldown = cooldown.sprintOnCooldown;
            bool isSprinting = cooldown.isSprinting;

            float percent = Mathf.Clamp01(staminaRemaining / staminaDuration);

            if (Mathf.Abs(percent - lastBarPercent) > 0.01f)
            {
                lastBarPercent = percent;
                float barCurrentWidth = staminaBarWidth * percent;
                staminaBar.rectTransform.DOSizeDelta(new Vector2(barCurrentWidth, staminaBarHeight), 0.2f);
            }

            bool shouldShow = !hideBarWhenFull || percent < 1f || isOnCooldown;
            if (hideBarWhenFull && percent >= 1f)
            {
                if (!barJustFilled)
                {
                    barJustFilled = true;
                    barFillHideTimer = barShowTimeAfterFull;
                }
                shouldShow = barFillHideTimer > 0f;
                if (barFillHideTimer > 0f)
                    barFillHideTimer -= Time.deltaTime;
            }
            else
            {
                barJustFilled = false;
                barFillHideTimer = 0f;
            }

            staminaBar.gameObject.SetActive(shouldShow);
            if (staminaBarCG) staminaBarCG.alpha = shouldShow ? 1f : 0f;

            Color targetColor = percent < 1f ? regenColor : fullColor;
            if (staminaBar.color != targetColor)
            {
                staminaBar.DOColor(targetColor, 0.2f);
            }
        }

        void UpdateKnockedOutOverlay()
        {
            if (knockedOutOverlay == null) return;
            knockedOutOverlay.SetActive(state.IsKnockedOut);
        }

        void UpdateItemLabels()
        {
            if (itemLabelText == null || itemActionText == null || interactionController == null)
                return;

            // Default: Hide texts
            itemLabelText.text = "";
            itemActionText.text = "";

            if (!interactionController.IsLookingAtInteractable || state.IsKnockedOut)
                return;

            var lookedAt = interactionController.GetLookedAtInteractable();

            if (lookedAt is ILabel labelProvider)
            {
                itemLabelText.text = labelProvider.Label;
            }
            else
            {
                itemLabelText.text = lookedAt?.GetType().Name ?? "";
            }

            if (lookedAt is ICommand commandProvider && !string.IsNullOrEmpty(commandProvider.ActionName))
            {
                itemActionText.text = commandProvider.ActionName;
            }
            else if (lookedAt is PickableItem)
            {
                itemActionText.text = "[E] Grab";
            }
            else if (lookedAt != null)
            {
                itemActionText.text = "[E] Interact";
            }
        }
    }
}