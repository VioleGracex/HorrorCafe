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

            // Only handle chase logic if actually chasing
            if (state == CustomerState.Chasing)
                HandleChasePlayer();
        }

        protected override void BecomeAngrySitting()
        {
            SetState(CustomerState.AngrySitting);
            animationController.PlaySitMad();
            hasBecomeAngrySitting = true;
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