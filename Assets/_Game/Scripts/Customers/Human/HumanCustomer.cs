using UnityEngine;
using Ouiki.Items;
using Ouiki.FPS;

namespace Ouiki.Restaurant
{
    public class HumanCustomer : BaseCustomer
    {
        protected override void BecomeImpatient()
        {
            SnapToStandPoint(); 
            SetState(CustomerState.Angry);
            animationController.PlaySitMad();
            chaseTimer = chaseTime;
        }

        protected override void Update()
        {
            base.Update();
            if (state == CustomerState.Angry || state == CustomerState.Chasing)
                HandleChasePlayer();
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
                HitBarista();
                StandAndLeave();
            }
            else
            {
                if (state != CustomerState.Chasing)
                    SnapToStandPoint(); 
                SetState(CustomerState.Chasing);
                chaseTimer -= Time.deltaTime;
                agent.speed = chaseSpeed;
                agent.stoppingDistance = attackDistance * 0.9f;
                agent.SetDestination(baristaTarget.position);

                if (chaseTimer <= 0f)
                    StandAndLeave();
            }
        }

        protected override void HitBarista()
        {
            if (hasHitBarista) return;
            hasHitBarista = true;
            baristaTarget?.GetComponent<PlayerInteractionController>()?.SendMessage("TakeDamage", SendMessageOptions.DontRequireReceiver);
        }
    }
}