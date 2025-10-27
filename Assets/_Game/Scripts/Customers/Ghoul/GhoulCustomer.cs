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

        public bool hasMorphed = false;
        private float morphTimer = 0f;
        private float lastShoutTime = -999f;
        private bool isTaunting = false;
        private int coffeeOrderCount = 0;

        private bool hasGrabbedPlayer = false;
        private Transform rightHandIK; 

        [Header("Grabbed Player Body Rotation")]
        public Vector3 playerGrabbedLocalEulerAngles = new Vector3(0f, 0f, 0f);

        [Header("Grabbed Player Arm Offset")]
        public Vector3 playerGrabbedArmOffset = Vector3.zero;

        protected override void Start()
        {
            base.Start();
            //SwitchToHumanForm();
        }

        protected override void Update()
        {
            base.Update();

            if (state == CustomerState.Angry && !hasMorphed)
            {
                morphTimer += Time.deltaTime;
                if (morphTimer >= morphDelay)
                {
                    MorphToMonster();
                    TauntAndChase();
                }
            }

            if (state == CustomerState.Chasing && hasMorphed && Time.time - lastShoutTime > shoutCooldown)
            {
                lastShoutTime = Time.time;
                PlayScream();
            }

            if (state == CustomerState.Chasing && hasMorphed && PlayerLost())
            {
                lostPlayer = true;
                lastKnownPlayerPos = baristaTarget ? baristaTarget.position : transform.position;
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
            if (humanFormGO) humanFormGO.SetActive(true);
            if (monsterFormGO) monsterFormGO.SetActive(false);
            animationController.SetAnimator(humanAnimator);
            hasMorphed = false;
        }

        public void SwitchToMonsterForm()
        {
            if (humanFormGO) humanFormGO.SetActive(false);
            if (monsterFormGO) monsterFormGO.SetActive(true);
            animationController.SetAnimator(monsterAnimator);
            hasMorphed = true;
        }

        void MorphToMonster()
        {
            morphTimer = 0f;
            SwitchToMonsterForm();
            animationController?.PlayMorph();
            MakeAllHumansFlee();
        }

        void MorphToHuman()
        {
            SwitchToHumanForm();
            animationController?.PlayMorph();
        }

        void TauntAndChase()
        {
            animationController?.PlayTaunt();
            isTaunting = true;
            Invoke(nameof(BeginChase), 1.5f);
        }

        public void BeginChase()
        {
            SetState(CustomerState.Chasing);
            MusicManager.Instance?.PlayChaseMusic();
            isTaunting = false;
            animationController?.PlayAlert();
            if (baristaTarget)
            {
                agent.speed = chaseSpeed * 1.3f;
                agent.stoppingDistance = attackDistance * 0.8f;
                agent.SetDestination(baristaTarget.position);
                animationController?.PlayRun();
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
            SetState(CustomerState.Ordering);
            patienceRemaining = patienceTime;
            coffeeOrderCount++;
        }

        protected override void CheckForServedCoffee()
        {
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
            if (state != CustomerState.Ordering && state != CustomerState.Waiting) return;
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
            SetState(CustomerState.Angry);
            patienceRemaining = 0;
            morphTimer = 0;
        }

        protected override void BecomeImpatient()
        {
            SetState(CustomerState.Angry);
            morphTimer = 0;
            patienceRemaining = 0;
        }

        public override void FleeFromGhoul() { }

        public override void HandleChasePlayer()
        {
            if (baristaTarget == null)
            {
                StandAndLeave();
                return;
            }

            float dist = Vector3.Distance(transform.position, baristaTarget.position);

            if (dist <= attackDistance * 1.2f)
            {
                TryGrabPlayer();
            }
            else
            {
                if (!agent.pathPending)
                {
                    agent.SetDestination(baristaTarget.position);
                    animationController?.PlayRun();
                    lostPlayer = false;
                    lastKnownPlayerPos = baristaTarget.position;
                }
            }
        }

        private void TryGrabPlayer()
        {
            if (hasGrabbedPlayer) return;

            // Confirm it's the player by tag (must set "Player" tag on player GameObject)
            if (baristaTarget != null && baristaTarget.CompareTag("Player"))
            {
                hasGrabbedPlayer = true;
                var player = baristaTarget.GetComponent<PlayerInteractionController>();
                if (player != null)
                {
                    StartCoroutine(GrabSequence(player));
                }
            }
        }

        private IEnumerator GrabSequence(PlayerInteractionController player)
        {
            animationController.PlayGrab();

            // Use Animator IK to get right hand if possible
            if (rightHandIK == null && monsterAnimator != null)
            {
                rightHandIK = monsterAnimator.GetBoneTransform(HumanBodyBones.RightHand);
            }

            // Snap player to right hand position for the grab, set local rotation and offset
            if (rightHandIK != null)
            {
                player.transform.SetParent(rightHandIK);
                player.transform.localPosition = playerGrabbedArmOffset;
                player.transform.localRotation = Quaternion.Euler(playerGrabbedLocalEulerAngles);
            }

            if (player.stateManager != null)
                player.stateManager.GetGrabbed();

            yield return new WaitForSeconds(grabDuration + holdPlayerDuration);

            // === KILL PLAYER HERE ===
            if (player.stateManager != null)
                player.stateManager.Die();

            // Optional: Detach the player from hand after
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

        protected override void StandAndLeave()
        {
            SetState(CustomerState.Leaving);
            SwitchToHumanForm();
            animationController?.PlayIdle();
            assignedSeat?.Release();
            agent.SetDestination(assignedSeat.StandPointWorld);
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