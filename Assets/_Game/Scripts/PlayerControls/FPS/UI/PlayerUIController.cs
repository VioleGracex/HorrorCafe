using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using Ouiki.Interfaces;
using Ouiki.Items;

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

            SetTextAndActive(itemLabelText, "");
            SetTextAndActive(itemActionText, "");
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

            string label = "";
            string action = "";

            if (interactionController.IsLookingAtInteractable && !state.IsKnockedOut)
            {
                var lookedAt = interactionController.GetLookedAtInteractable();

                if (lookedAt is ILabel labelProvider)
                {
                    label = labelProvider.Label;
                }
                else
                {
                    label = lookedAt?.GetType().Name ?? "";
                }

                if (lookedAt is ICommand commandProvider && !string.IsNullOrEmpty(commandProvider.ActionName))
                {
                    action = commandProvider.ActionName;
                }
                else if (lookedAt is PickableItem)
                {
                    action = "[E] Grab";
                }
                else if (lookedAt != null)
                {
                    action = "[E] Interact";
                }
            }

            SetTextAndActive(itemLabelText, label);
            SetTextAndActive(itemActionText, action);
        }

        private void SetTextAndActive(TMP_Text txt, string value)
        {
            if (txt == null) return;
            txt.text = value;
            if (txt.gameObject.activeSelf != !string.IsNullOrEmpty(value))
                txt.gameObject.SetActive(!string.IsNullOrEmpty(value));
        }
    }
}