using UnityEngine;
using UnityEngine.AI;
using DG.Tweening;
using System;
using Ouiki.Items;
using System.Collections;
using Ouiki.FPS;

namespace Ouiki.Restaurant
{
    public enum CustomerState
    {
        WalkingToSeat, Sitting, Ordering, Waiting,AngrySitting, Chasing, Drinking, Happy, Leaving, Left, Fleeing, Searching, none
    }

    [RequireComponent(typeof(NavMeshAgent))]
    public class BaseCustomer : MonoBehaviour
    {
        public CustomerAnimationController animationController;
        [SerializeField] public CustomerState state = CustomerState.none;
        [HideInInspector] public CustomerSeat assignedSeat;
        [SerializeField] public float patienceRemaining;
        [HideInInspector] public CustomerSpawner customerSpawner;
        protected NavMeshAgent agent;
        protected bool coffeeServed = false;
        protected bool isDrinking = false;
        protected bool isFleeing = false;
        protected Transform baristaTarget;
        protected float chaseTimer = 0f;
        protected bool hasHitBarista = false;
        protected float patienceTime;
        protected float chaseTime;
        protected float attackDistance;
        protected float chaseSpeed;
        protected float walkSpeed;
        protected float fleeSpeed;

        [Header("Vision/Detection")]
        public float visionRadius = 12f;
        public float visionFOV = 90f;
        public float searchRadius = 4f;
        public float searchTime = 4f;
        public int maxSearchAttempts = 5;

        protected bool lostPlayer = false;
        protected float searchTimer = 0f;
        protected Vector3 lastKnownPlayerPos;
        protected int searchAttempts = 0;

        [HideInInspector] public Vector3 exitPoint;

        private Tween _sitTween;
        private bool _isSittingInProgress = false;
        protected bool hasBecomeAngrySitting = false;

        public event Action<BaseCustomer> OnLeave;

        public virtual void Initialize(CustomerTypeSO typeSO, CustomerSpawner spawner)
        {
            customerSpawner = spawner;
            patienceTime = typeSO.patienceTime;
            chaseTime = typeSO.chaseTime;
            attackDistance = typeSO.attackDistance;
            chaseSpeed = typeSO.chaseSpeed;
            walkSpeed = typeSO.walkSpeed;
            fleeSpeed = typeSO.fleeSpeed;
        }

        protected virtual void Awake() => agent = GetComponent<NavMeshAgent>();

        protected virtual void Start()
        {
            baristaTarget = FindFirstObjectByType<PlayerInteractionController>()?.transform;
        }

        protected virtual void Update()
        {
            switch (state)
            {
                case CustomerState.WalkingToSeat:
                    if (!_isSittingInProgress && agent.remainingDistance < 0.05f && !agent.pathPending)
                        StartSitSequence();
                    break;
                case CustomerState.Ordering:
                case CustomerState.Waiting:
                case CustomerState.AngrySitting:
                    patienceRemaining -= Time.deltaTime;

                    if ((state == CustomerState.Ordering || state == CustomerState.Waiting) && !hasBecomeAngrySitting && patienceRemaining <= patienceTime * 0.5f && patienceRemaining > 0f && !isDrinking)
                    {
                        BecomeAngrySitting();
                    }

                    if (patienceRemaining <= 0f && !isDrinking)
                        BecomeImpatient();
                    else
                        CheckForServedCoffee();
                    break;
                case CustomerState.Leaving:
                case CustomerState.Fleeing:
                    if (agent.remainingDistance < 0.05f && !agent.pathPending)
                        MarkLeft();
                    break;
                case CustomerState.Chasing:
                    if (lostPlayer && !agent.pathPending && agent.remainingDistance < 0.1f)
                        BeginSearch();
                    break;
                case CustomerState.Searching:
                    UpdateSearching();
                    break;
            }
        }

        public virtual void AssignSeat(CustomerSeat seat)
        {
            assignedSeat = seat;
            assignedSeat.Reserve();
            agent.speed = walkSpeed;
            agent.stoppingDistance = 0.01f;
            state = CustomerState.none; // force state change
            SetState(CustomerState.WalkingToSeat);
            agent.SetDestination(seat.StandPointWorld);
            _isSittingInProgress = false;
        }

        protected virtual void StartSitSequence()
        {
            if (_isSittingInProgress) return;
            _isSittingInProgress = true;
            agent.ResetPath();
            _sitTween?.Kill();

            Sequence sitSequence = DOTween.Sequence();
            sitSequence.Append(transform.DOMove(assignedSeat.SitPointWorld, 0.5f).SetEase(Ease.InOutSine));
            sitSequence.Join(transform.DORotateQuaternion(assignedSeat.SitRotationWorld, 0.5f).SetEase(Ease.InOutSine));
            sitSequence.OnComplete(() =>
            {
                transform.position = assignedSeat.SitPointWorld;
                transform.rotation = assignedSeat.SitRotationWorld;
                SetState(CustomerState.Sitting);
                StartOrder();
                _sitTween = null;
                _isSittingInProgress = false;
            });

            _sitTween = sitSequence;
        }

        protected virtual void StartOrder()
        {
            SetState(CustomerState.Ordering);
            patienceRemaining = patienceTime;
            assignedSeat?.ShowServiceIndicator();
            hasBecomeAngrySitting = false;
        }

        /// <summary>
        /// Called when patience reaches half but not zero: sit mad, but don't chase yet
        /// </summary>
        protected virtual void BecomeAngrySitting()
        {
            SetState(CustomerState.AngrySitting);
            animationController.PlaySitMad();
            hasBecomeAngrySitting = true;
        }

        /// <summary>
        /// Called when patience runs out completely: stand up and chase
        /// </summary>
        protected virtual void BecomeImpatient()
        {
            assignedSeat?.HideServiceIndicator();
            SnapToStandPoint();
            SetState(CustomerState.Chasing);
            animationController.PlayRun();
            chaseTimer = chaseTime;
        }

        protected virtual void CheckForServedCoffee()
        {
            var slot = assignedSeat.serviceSlot;
            if (slot == null || !slot.IsOccupied) return;
            var cup = slot.GetComponentInChildren<CupItem>();
            if (cup != null && cup.IsFilled && cup.isActiveAndEnabled)
            {
                ServeCoffee(cup);
            }
        }

        public virtual void ServeCoffee(CupItem cup)
        {
            // Can serve coffee if not standing up and chasing
            if (state != CustomerState.Ordering && state != CustomerState.Waiting && state != CustomerState.AngrySitting) return;
            coffeeServed = true;
            isDrinking = true;
            cup.SetInteractable(false);
            StartDrinking(cup);
        }

        protected virtual void StartDrinking(CupItem cup)
        {
            assignedSeat?.HideServiceIndicator();
            SetState(CustomerState.Drinking);
            animationController?.PlaySitDrink();
            StartCoroutine(FinishDrinkingRoutine(cup, 2.5f));
        }

        protected virtual IEnumerator FinishDrinkingRoutine(CupItem cup, float delay)
        {
            yield return new WaitForSeconds(delay);
            FinishDrinking(cup);
        }

        protected virtual void FinishDrinking(CupItem cup)
        {
            SetState(CustomerState.Happy);
            cup.SetInteractable(true);
            cup.SetFilled(false);
            assignedSeat.serviceSlot.Remove();
            assignedSeat.HideServiceIndicator();
            Invoke(nameof(StandAndLeave), 1.5f);
        }

        protected virtual void StandAndLeave()
        {
            _sitTween?.Kill();
            _sitTween = null;
            SetState(CustomerState.Leaving);
            assignedSeat?.HideServiceIndicator();
            assignedSeat?.Release();
            agent.SetDestination(exitPoint);
        }

        protected virtual void MarkLeft()
        {
            _sitTween?.Kill();
            _sitTween = null;
            SetState(CustomerState.Left);
            assignedSeat?.HideServiceIndicator();
            OnLeave?.Invoke(this);
            Destroy(gameObject, 1.0f);
        }

        protected virtual void OnDestroy()
        {
            _sitTween?.Kill();
            _sitTween = null;
        }

        public virtual void SetState(CustomerState newState)
        {
            if (state == newState) return;
            state = newState;

            switch (state)
            {
                case CustomerState.WalkingToSeat:
                    animationController.PlayWalk();
                    break;
                case CustomerState.Sitting:
                    animationController.PlaySitIdle();
                    break;
                case CustomerState.Ordering:
                case CustomerState.Waiting:
                    animationController.PlaySitIdle();
                    break;
                case CustomerState.AngrySitting:
                    animationController.PlaySitMad();
                    break;
                case CustomerState.Chasing:
                    animationController.PlayRun();
                    break;
                case CustomerState.Searching:
                    animationController.PlayAlert();
                    break;
                case CustomerState.Drinking:
                    animationController.PlaySitDrink();
                    break;
                case CustomerState.Leaving:
                    animationController.PlayWalk();
                    break;
                case CustomerState.Fleeing:
                    animationController.PlayRun();
                    break;
                case CustomerState.Happy:
                    animationController.PlaySitWohoo();
                    break;
            }
        }

        public virtual void FleeFromGhoul()
        {
            if (state == CustomerState.Left || state == CustomerState.Leaving || state == CustomerState.Fleeing || state == CustomerState.Chasing) return;
            isFleeing = true;
            _sitTween?.Kill();
            _sitTween = null;
            SetState(CustomerState.Fleeing);
            animationController.PlayRun();
            assignedSeat?.HideServiceIndicator();
            assignedSeat?.Release();
            agent.speed = fleeSpeed;
            agent.stoppingDistance = 0.01f;
            agent.SetDestination(exitPoint); 
        }

        public virtual void HandleChasePlayer()
        {
            if (baristaTarget == null)
            {
                StandAndLeave();
                return;
            }

            float dist = Vector3.Distance(transform.position, baristaTarget.position);

            if (dist <= attackDistance * 1.2f)
            {
                StandAndLeave();
            }
            else
            {
                if (!agent.pathPending)
                {
                    agent.SetDestination(baristaTarget.position);
                    animationController.PlayRun();
                    lostPlayer = false;
                    lastKnownPlayerPos = baristaTarget.position;
                }
            }
        }

        protected virtual void SnapToStandPoint()
        {
            if (assignedSeat != null)
            {
                transform.position = assignedSeat.StandPointWorld;
                agent.Warp(assignedSeat.StandPointWorld);
                transform.rotation = assignedSeat.SitRotationWorld;
            }
        }

        protected void BeginSearch()
        {
            SetState(CustomerState.Searching);
            searchTimer = 0f;
            searchAttempts = 0;
        }

        protected void UpdateSearching()
        {
            searchTimer += Time.deltaTime;
            if (searchTimer > searchTime)
            {
                StandAndLeave();
                return;
            }

            if (!agent.pathPending && agent.remainingDistance < 0.15f)
            {
                if (searchAttempts < maxSearchAttempts)
                {
                    Vector3 rnd = UnityEngine.Random.insideUnitSphere * searchRadius;
                    rnd.y = 0;
                    Vector3 searchPos = lastKnownPlayerPos + rnd;

                    NavMeshHit hit;
                    if (NavMesh.SamplePosition(searchPos, out hit, 1.2f, NavMesh.AllAreas))
                    {
                        agent.SetDestination(hit.position);
                        animationController.PlayAlert();
                        searchAttempts++;
                    }
                }
                else
                {
                    StandAndLeave();
                }
            }
        }

        protected virtual void HitBarista()
        {
            if (hasHitBarista) return;
            hasHitBarista = true;
            baristaTarget?.GetComponent<PlayerInteractionController>()?.SendMessage("TakeDamage", SendMessageOptions.DontRequireReceiver);
        }

#if UNITY_EDITOR
        protected virtual void OnDrawGizmosSelected()
        {
            // Draw vision cone from head
            Transform head = null;
            Animator anim = (animationController != null) ? animationController.CurrentAnimator : null;
            if (anim != null && anim.isHuman)
                head = anim.GetBoneTransform(HumanBodyBones.Head);

            if (head == null)
                head = this.transform; // fallback

            float fovAngle = visionFOV;
            float fovRange = visionRadius;
            int segments = 32;
            Vector3 headPos = head.position;
            Vector3 forward = head.forward;

            // Draw cone lines
            Gizmos.color = new Color(1f, 0.8f, 0.2f, 0.38f);
            for (int i = 0; i <= segments; i++)
            {
                float angle = -fovAngle / 2 + (fovAngle * i / segments);
                Quaternion rot = Quaternion.AngleAxis(angle, head.up);
                Vector3 dir = rot * forward;
                Gizmos.DrawLine(headPos, headPos + dir * fovRange);
            }

            // Draw arc between cone lines
            Vector3 prevPoint = Vector3.zero;
            for (int i = 0; i <= segments; i++)
            {
                float angle = -fovAngle / 2 + (fovAngle * i / segments);
                Quaternion rot = Quaternion.AngleAxis(angle, head.up);
                Vector3 dir = rot * forward;
                Vector3 point = headPos + dir * fovRange;
                if (i > 0) Gizmos.DrawLine(prevPoint, point);
                prevPoint = point;
            }

            // Draw search radius as transparent blue sphere
            Gizmos.color = new Color(0.4f, 0.9f, 1f, 0.14f);
            Gizmos.DrawWireSphere(transform.position, searchRadius);
        }
#endif
    }
}