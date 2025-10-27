using UnityEngine;

namespace Ouiki.Restaurant
{
    public class CustomerAnimationController : MonoBehaviour
    {
        [SerializeField] private Animator animator;

        public Animator CurrentAnimator => animator;

        public void SetAnimator(Animator newAnimator)
        {
            animator = newAnimator;
        }

        public virtual void PlaySitIdle()    => animator?.Play("chair sit idle");
        public virtual void PlaySitMad()     => animator?.Play("sit mad");
        public virtual void PlaySitShout()   => animator?.Play("sit shout");
        public virtual void PlaySitDrink()   => animator?.Play("Sit drink");
        public virtual void PlaySitClap()    => animator?.Play("sit clap");
        public virtual void PlaySitWohoo()   => animator?.Play("sit wohoo");
        public virtual void PlayIdle()       => animator?.Play("idle");
        public virtual void PlayMorph()      => animator?.Play("morph");
        public virtual void PlayTaunt()      => animator?.Play("taunt");
        public virtual void PlayWalk()       => animator?.Play("walk");
        public virtual void PlayRun()        => animator?.Play("run");
        public virtual void PlayScream()     => animator?.Play("scream");
        public virtual void PlayAlert()      => animator?.Play("alert");
        public virtual void PlayMuscleFlex() => animator?.Play("muscle flex");
        public virtual void PlayGrab() => animator?.Play("grab");
    }
}