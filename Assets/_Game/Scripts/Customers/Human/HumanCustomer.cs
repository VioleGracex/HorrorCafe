using Ouiki.Items;
using Ouiki.FPS;
using UnityEngine;

namespace Ouiki.Restaurant
{
    public class HumanCustomer : BaseCustomer
    {
        private bool hasKnockedOutPlayer = false;

        protected override void Update()
        {
            base.Update();

            // When waiting or ordering, check for patience thresholds
            if ((state == CustomerState.Waiting || state == CustomerState.Ordering) && !isDrinking)
            {
                // If patience <= 50% and not yet angry sitting
                if (!hasBecomeAngrySitting && patienceRemaining <= (patienceTime * 0.5f) && patienceRemaining > 0f)
                {
                    SetState(CustomerState.Angry);
                    animationController.PlaySitMad();
                    hasBecomeAngrySitting = true;
                }
                // If patience is 0, stand up and start chasing
                else if (patienceRemaining <= 0f)
                {
                    BecomeImpatient();
                }
            }

            if (state == CustomerState.Angry || state == CustomerState.Chasing)
                HandleChasePlayer();
        }

        protected override void BecomeImpatient()
        {
            assignedSeat?.HideServiceIndicator();
            SnapToStandPoint();
            SetState(CustomerState.Chasing);
            animationController.PlayRun();
            chaseTimer = chaseTime;
        }

        public override void HandleChasePlayer()
        {
            if (baristaTarget == null)
            {
                StandAndLeave();
                return;
            }

            float dist = Vector3.Distance(transform.position, baristaTarget.position);

            if (dist <= attackDistance)
            {
                if (!hasKnockedOutPlayer)
                {
                    KnockOutBarista();
                    hasKnockedOutPlayer = true;
                }
                StandAndLeave();
            }
            else
            {
                SetState(CustomerState.Chasing);
                MusicManager.Instance?.PlayChaseMusic();
                chaseTimer -= Time.deltaTime;
                agent.speed = chaseSpeed;
                agent.stoppingDistance = attackDistance * 0.9f;
                agent.SetDestination(baristaTarget.position);

                if (chaseTimer <= 0f)
                {
                    StandAndLeave();
                    MusicManager.Instance?.StopChaseMusic();
                }
            }
        }

        private void KnockOutBarista()
        {
            var player = baristaTarget?.GetComponent<PlayerInteractionController>();
            if (player != null)
                player.KnockOutPlayer();

            MusicManager.Instance?.StopChaseMusic();                
        }
    }
}