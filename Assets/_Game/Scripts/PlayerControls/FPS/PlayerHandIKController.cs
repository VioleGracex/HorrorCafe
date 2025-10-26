using UnityEngine;

namespace Ouiki.FPS
{
    public class PlayerHandIKController : MonoBehaviour
    {
        [Header("References")]
        public Animator animator; // Assign in inspector (same as your player mesh)
        public Transform rightHandTarget; // Empty GameObject in front of camera where hand should go
        public Transform rightHandHint;   // (Optional) For elbow, assign if needed
        public Transform rightHandHoldPoint; // Child of hand bone for precise item placement

        [Header("IK Settings")]
        [Range(0, 1)] public float ikWeight = 1f; // 0 = no IK, 1 = full IK, can be animated
        public float ikLerpSpeed = 5f;

        private bool isHolding = false;
        private Transform heldItem;

        void LateUpdate()
        {
            float targetWeight = isHolding ? 1f : 0f;
            ikWeight = Mathf.MoveTowards(ikWeight, targetWeight, ikLerpSpeed * Time.deltaTime);
        }

        public void SetHeldItem(Transform item)
        {
            heldItem = item;
            isHolding = item != null;
            if (heldItem && rightHandHoldPoint)
            {
                heldItem.SetParent(rightHandHoldPoint);
                heldItem.localPosition = Vector3.zero;
                heldItem.localRotation = Quaternion.identity;
            }
            else if (!heldItem)
            {
                // Optionally: reset parent if needed
            }
        }

        void OnAnimatorIK(int layerIndex)
        {
            if (animator == null) return;
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, ikWeight);
            animator.SetIKRotationWeight(AvatarIKGoal.RightHand, ikWeight);

            if (rightHandTarget)
            {
                animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandTarget.position);
                animator.SetIKRotation(AvatarIKGoal.RightHand, rightHandTarget.rotation);
            }

            if (rightHandHint)
            {
                animator.SetIKHintPositionWeight(AvatarIKHint.RightElbow, ikWeight);
                animator.SetIKHintPosition(AvatarIKHint.RightElbow, rightHandHint.position);
            }
        }
    }
}