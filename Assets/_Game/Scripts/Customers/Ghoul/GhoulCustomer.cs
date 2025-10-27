using UnityEngine;
using Ouiki.Items;
using System.Collections;
using Ouiki.FPS;
using DG.Tweening;

namespace Ouiki.Restaurant
{
    public class GhoulCustomer : BaseCustomer
    {
        [Header("Form GameObjects")]
        public GameObject humanFormGO;
        public GameObject monsterFormGO;

        [Header("Animators")]
        public Animator humanAnimator;
        public Animator monsterAnimator;

        [Header("Ghoul Settings")]
        public float morphDelay = 0.6f;
        public float shoutCooldown = 20f;

        [Header("Grab/Jump Scare")]
        public float grabDuration = 0.65f;
        public float holdPlayerDuration = 1.2f;

        [Header("Scream Settings")]
        public AudioSource screamAudioSource;
        public AudioClip screamClip;

        [Header("Grabbed Player Body Rotation")]
        public Vector3 playerGrabbedLocalEulerAngles = new Vector3(0f, 0f, 0f);

        [Header("Grabbed Player Arm Offset")]
        public Vector3 playerGrabbedArmOffset = Vector3.zero;

        public bool hasMorphed = false;
        private float morphTimer = 0f;
        private float lastShoutTime = -999f;
        private bool isTaunting = false;
        private int coffeeOrderCount = 0;
        private bool hasGrabbedPlayer = false;
        private Transform rightHandIK;
        private bool searchingForPlayer = false;

        // DOTween sit tween reference
        private Tween _sitTween;

        protected override void Start()
        {
            base.Start();
        }

        protected override void Update()
        {
            base.Update();

            // If morphing sequence started and not morphed, morph and chase after delay
            if (state == CustomerState.AngrySitting && !hasMorphed)
            {
                morphTimer += Time.deltaTime;
                if (morphTimer >= morphDelay)
                {
                    MorphToMonster();
                    TauntAndChase();
                }
            }

            // --- CRITICAL: Always run chase/try grab while chasing ---
            if (state == CustomerState.Chasing && hasMorphed)
            {
                HandleChasePlayer();

                // Scream periodically while chasing in monster form
                if (Time.time - lastShoutTime > shoutCooldown)
                {
                    lastShoutTime = Time.time;
                    PlayScream();
                }

                // If lost player during chase, start "infinite search" mode
                if (PlayerLost() && !searchingForPlayer)
                {
                    Debug.Log($"[GhoulCustomer] Lost player during chase. Starting search mode.");
                    StartCoroutine(InfinitePlayerSearch());
                }
            }
        }

        bool PlayerLost()
        {
            if (!baristaTarget) return true;
            float dist = Vector3.Distance(transform.position, baristaTarget.position);
            return dist > visionRadius;
        }

        void SwitchToHumanForm()
        {
            KillSitTween();
            if (humanFormGO) humanFormGO.SetActive(true);
            if (monsterFormGO) monsterFormGO.SetActive(false);
            animationController.SetAnimator(humanAnimator);
            hasMorphed = false;
            Debug.Log("[GhoulCustomer] Switched to human form.");
        }

        public void SwitchToMonsterForm()
        {
            KillSitTween();
            if (humanFormGO) humanFormGO.SetActive(false);
            if (monsterFormGO) monsterFormGO.SetActive(true);
            animationController.SetAnimator(monsterAnimator);
            hasMorphed = true;
            Debug.Log("[GhoulCustomer] Switched to monster form.");
        }

        void MorphToMonster()
        {
            morphTimer = 0f;
            KillSitTween();
            SwitchToMonsterForm();
            animationController?.PlayMorph();
            MakeAllHumansFlee();
            Debug.Log("[GhoulCustomer] Morphed to monster.");
        }

        void MorphToHuman()
        {
            KillSitTween();
            SwitchToHumanForm();
            animationController?.PlayMorph();
            Debug.Log("[GhoulCustomer] Morphed to human.");
        }

        void TauntAndChase()
        {
            KillSitTween();
            animationController?.PlayTaunt();
            isTaunting = true;
            Invoke(nameof(BeginChase), 1.5f);
            Debug.Log("[GhoulCustomer] Taunting and will begin chase.");
        }

        public void BeginChase()
        {
            KillSitTween();
            SetState(CustomerState.Chasing);
            MusicManager.Instance?.PlayChaseMusic();
            isTaunting = false;
            animationController?.PlayAlert();
            if (baristaTarget)
            {
                agent.enabled = true; 
                agent.speed = chaseSpeed * 1.3f;
                agent.stoppingDistance = 0f;
                agent.SetDestination(baristaTarget.position);
                animationController?.PlayRun();
                Debug.Log("[GhoulCustomer] Began chasing player.");
            }
        }

        void MakeAllHumansFlee()
        {
            if (customerSpawner == null) return;
            foreach (var cust in customerSpawner.ActiveCustomers)
            {
                if (cust != this && cust is HumanCustomer)
                {
                    cust.FleeFromGhoul();
                }
            }
        }

        protected override void StartOrder()
        {
            if (!hasMorphed && state != CustomerState.AngrySitting && state != CustomerState.Chasing && state != CustomerState.Drinking)
            {
                SetState(CustomerState.Ordering);
                patienceRemaining = patienceTime;
                coffeeOrderCount++;
                assignedSeat?.ShowServiceIndicator();
                hasBecomeAngrySitting = false;
            }
        }

        protected override void CheckForServedCoffee()
        {
            if (state != CustomerState.Ordering && state != CustomerState.Waiting)
                return;

            var slot = assignedSeat.serviceSlot;
            if (slot == null || !slot.IsOccupied) return;
            var cup = slot.GetComponentInChildren<CupItem>();
            if (cup != null && cup.IsFilled && cup.isActiveAndEnabled)
            {
                ServeCoffee(cup);
            }
        }

        public override void ServeCoffee(CupItem cup)
        {
            if (state != CustomerState.Ordering && state != CustomerState.Waiting)
                return;

            coffeeServed = true;
            isDrinking = true;
            cup.SetInteractable(false);
            StartDrinking(cup);
        }

        protected override void StartDrinking(CupItem cup)
        {
            SetState(CustomerState.Drinking);
            animationController?.PlaySitDrink();
            StartCoroutine(FinishDrinkingRoutine(cup, 2.5f));
        }

        protected override IEnumerator FinishDrinkingRoutine(CupItem cup, float delay)
        {
            yield return new WaitForSeconds(delay);
            FinishDrinking(cup);
        }

        protected override void FinishDrinking(CupItem cup)
        {
            SetState(CustomerState.Happy);
            animationController?.PlaySitWohoo();
            cup.SetInteractable(true);
            cup.SetFilled(false);
            assignedSeat.serviceSlot.Remove();

            if (OtherHumansExist())
            {
                Invoke(nameof(StartOrder), 1.5f);
            }
            else
            {
                Invoke(nameof(StartMorphSequence), 1.5f);
            }
        }

        bool OtherHumansExist()
        {
            if (customerSpawner == null) return false;
            foreach (var cust in customerSpawner.ActiveCustomers)
            {
                if (cust != this && cust is HumanCustomer && cust.state != CustomerState.Left && cust.state != CustomerState.Leaving && cust.state != CustomerState.Fleeing)
                    return true;
            }
            return false;
        }

        void StartMorphSequence()
        {
            SetState(CustomerState.AngrySitting);
            patienceRemaining = 9999f;
            morphTimer = 0;
        }

        protected override void BecomeAngrySitting()
        {
            if (!hasMorphed && OtherHumansExist())
            {
                SetState(CustomerState.AngrySitting);
                animationController.PlaySitMad();
                hasBecomeAngrySitting = true;
            }
        }

        protected override void BecomeImpatient()
        {
            patienceRemaining = 0;
            if (!hasMorphed)
            {
                MorphToMonster();
            }
            TauntAndChase();
        }

        public override void FleeFromGhoul() { }

        public override void HandleChasePlayer()
        {
            // Prevent movement/animation overwrite when grabbing
            if (hasGrabbedPlayer)
                return;

            if (baristaTarget == null)
            {
                return;
            }

            float dist = Vector3.Distance(transform.position, baristaTarget.position);

            agent.stoppingDistance = 0f; 

            if (dist <= attackDistance * 1.2f)
            {
                Debug.Log("[GhoulCustomer] In range to grab player. Attempting to grab.");
                TryGrabPlayer();
                return;
            }

            SetState(CustomerState.Chasing);
            agent.enabled = true; 
            agent.speed = chaseSpeed * 1.3f;
            agent.SetDestination(baristaTarget.position);
            animationController?.PlayRun();
            lostPlayer = false;
            lastKnownPlayerPos = baristaTarget.position;
        }

        private void TryGrabPlayer()
        {
            if (hasGrabbedPlayer)
            {
                Debug.Log("[GhoulCustomer] Already grabbed player, returning.");
                return;
            }

            if (baristaTarget != null && baristaTarget.CompareTag("Player"))
            {
                Debug.Log("[GhoulCustomer] baristaTarget is Player, starting grab sequence.");
                hasGrabbedPlayer = true;
                var player = baristaTarget.GetComponent<PlayerInteractionController>();
                if (player != null)
                {
                    StartCoroutine(GrabSequence(player));
                }
                else
                {
                    Debug.LogWarning("[GhoulCustomer] baristaTarget does not have PlayerInteractionController.");
                }
            }
            else
            {
                Debug.LogWarning("[GhoulCustomer] baristaTarget is null or not tagged Player.");
            }
        }

        private System.Collections.IEnumerator GrabSequence(PlayerInteractionController player)
        {
            animationController?.PlayGrab();

            if (rightHandIK == null && monsterAnimator != null)
                rightHandIK = monsterAnimator.GetBoneTransform(HumanBodyBones.RightHand);

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
                AnimatorStateInfo stateInfo = monsterAnimator.GetCurrentAnimatorStateInfo(0);
                if (stateInfo.IsName("grab"))
                {
                    grabAnimDuration = stateInfo.length;
                }
                else
                {
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

            if (player.stateManager != null)
                player.stateManager.Die();

            if (player.transform.parent == rightHandIK)
                player.transform.SetParent(null);

            StartCoroutine(GhoulScreamLoop());
        }

        private IEnumerator GhoulScreamLoop()
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

        private IEnumerator InfinitePlayerSearch()
        {
            searchingForPlayer = true;
            int searchAttempts = 0;
            int guaranteedFind = Random.Range(2, 6);

            while (!hasGrabbedPlayer)
            {
                Vector3 searchPos = lastKnownPlayerPos + Random.insideUnitSphere * searchRadius;
                searchPos.y = transform.position.y;
                agent.enabled = true; 
                agent.SetDestination(searchPos);
                SetState(CustomerState.Searching);
                animationController?.PlayAlert();
                Debug.Log($"[GhoulCustomer] Searching for player. Attempt: {searchAttempts + 1}");

                while (agent.pathPending || agent.remainingDistance > 0.15f)
                {
                    yield return null;
                }

                yield return new WaitForSeconds(Random.Range(0.4f, 1.1f));
                searchAttempts++;

                if (searchAttempts >= guaranteedFind)
                {
                    Debug.Log("[GhoulCustomer] Found player during search! Returning to chase.");
                    SetState(CustomerState.Chasing);
                    agent.enabled = true; 
                    animationController?.PlayRun();
                    agent.SetDestination(baristaTarget.position);
                    searchingForPlayer = false;
                    yield break;
                }
            }
        }

        protected override void StandAndLeave()
        {
            KillSitTween();
            if (hasMorphed)
                return;

            SetState(CustomerState.Leaving);
            agent.enabled = true; 
            SwitchToHumanForm();
            animationController?.PlayIdle();
            assignedSeat?.Release();
            agent.SetDestination(assignedSeat.StandPointWorld);
        }

        private void KillSitTween()
        {
            if (_sitTween != null && _sitTween.IsActive())
            {
                _sitTween.Kill();
                _sitTween = null;
                Debug.Log("[GhoulCustomer] Killed DOTween sit tween.");
            }
        }

#if UNITY_EDITOR
        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();
            Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.18f);
            Gizmos.DrawWireSphere(transform.position, visionRadius);
        }
#endif
    }
}