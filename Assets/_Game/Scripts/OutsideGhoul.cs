using UnityEngine;
using Ouiki.FPS;

namespace Ouiki.Restaurant
{
    public class OutsideGhoul : MonoBehaviour
    {
        [Header("Ghoul Appearance")]
        public GameObject monsterFormGO;
        public CustomerAnimationController animationController;
        public Animator monsterAnimator;

        [Header("Chase Settings")]
        public float chaseSpeed = 8f;
        public float attackDistance = 2f;
        public float visionRadius = 15f;

        [Header("Scream Settings")]
        public AudioSource screamAudioSource;
        public AudioClip screamClip;

        [Header("Grab Settings")]
        public float grabDuration = 0.65f; // fallback duration if animation not found
        public float holdPlayerDuration = 1.2f;

        [Header("Grabbed Player Body Rotation")]
        public Vector3 playerGrabbedLocalEulerAngles = new Vector3(0f, 0f, 0f);

        [Header("Grabbed Player Arm Offset")]
        public Vector3 playerGrabbedArmOffset = Vector3.zero;

        private Transform playerTarget;
        private UnityEngine.AI.NavMeshAgent agent;
        private bool isChasing = false;
        private float lastScreamTime = -999f;
        public float screamCooldown = 12f;

        private bool hasGrabbedPlayer = false;
        private Transform rightHandIK; // for snapping player

        private void Awake()
        {
            agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        }

        private void Start()
        {
            if (monsterFormGO) monsterFormGO.SetActive(true);

            var player = FindFirstObjectByType<PlayerInteractionController>();
            if (player != null)
                playerTarget = player.transform;
        }

        private void Update()
        {
            if (!isChasing && playerTarget != null && CanSeePlayer())
            {
                BeginChase();
            }

            if (isChasing && playerTarget != null)
            {
                HandleChasePlayer();
                if (Time.time - lastScreamTime > screamCooldown)
                {
                    lastScreamTime = Time.time;
                    PlayScream();
                }
            }
        }

        public void BeginChase()
        {
            isChasing = true;
            MusicManager.Instance?.PlayChaseMusic();
            agent.speed = chaseSpeed;
            agent.stoppingDistance = attackDistance * 0.8f;
            animationController?.PlayRun();
        }

        private void HandleChasePlayer()
        {
            if (playerTarget == null) return;
            agent.SetDestination(playerTarget.position);

            float dist = Vector3.Distance(transform.position, playerTarget.position);
            if (dist <= attackDistance)
            {
                TryGrabPlayer();
            }
        }

        private bool CanSeePlayer()
        {
            if (playerTarget == null) return false;
            float dist = Vector3.Distance(transform.position, playerTarget.position);
            return dist < visionRadius;
        }

        private void TryGrabPlayer()
        {
            if (hasGrabbedPlayer) return;

            if (playerTarget != null && playerTarget.CompareTag("Player"))
            {
                hasGrabbedPlayer = true;
                var player = playerTarget.GetComponent<PlayerInteractionController>();
                if (player != null)
                {
                    StartCoroutine(GrabSequence(player));
                }
            }
        }

        private System.Collections.IEnumerator GrabSequence(PlayerInteractionController player)
        {
            animationController?.PlayGrab();

            // Use Animator IK to get right hand if possible
            if (rightHandIK == null && monsterAnimator != null)
                rightHandIK = monsterAnimator.GetBoneTransform(HumanBodyBones.RightHand);

            // Snap player to right hand position for the grab, set local rotation and offset
            if (rightHandIK != null)
            {
                player.transform.SetParent(rightHandIK);
                player.transform.localPosition = playerGrabbedArmOffset;
                player.transform.localRotation = Quaternion.Euler(playerGrabbedLocalEulerAngles);
            }

            if (player.stateManager != null)
                player.stateManager.GetGrabbed();

            // Wait for grab animation duration + hold
            float grabAnimDuration = grabDuration;
            if (monsterAnimator != null)
            {
                // Assumes "grab" is the animation state name for the grab
                AnimatorStateInfo stateInfo = monsterAnimator.GetCurrentAnimatorStateInfo(0);
                if (stateInfo.IsName("grab"))
                {
                    grabAnimDuration = stateInfo.length;
                }
                else
                {
                    // Try to get clip by name
                    foreach (var clip in monsterAnimator.runtimeAnimatorController.animationClips)
                    {
                        if (clip.name.ToLower().Contains("grab"))
                        {
                            grabAnimDuration = clip.length;
                            break;
                        }
                    }
                }
            }

            yield return new WaitForSeconds(grabAnimDuration + holdPlayerDuration);

            // === KILL PLAYER HERE ===
            if (player.stateManager != null)
                player.stateManager.Die();

            // (Optional) Detach player from hand after grab/scare
            if (player.transform.parent == rightHandIK)
                player.transform.SetParent(null);

            // (Optional) Play looped scream or finish the encounter here
            StartCoroutine(GhoulScreamLoop());
        }

        private System.Collections.IEnumerator GhoulScreamLoop()
        {
            while (true)
            {
                PlayScream();
                yield return new WaitForSeconds(2.5f);
            }
        }

        public void PlayScream()
        {
            animationController?.PlayScream();
            if (screamAudioSource != null && screamClip != null)
            {
                screamAudioSource.clip = screamClip;
                screamAudioSource.Play();
            }
        }
    }
}