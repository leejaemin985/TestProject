using UnityEngine;

namespace Unit
{
    public class PlayerFSMAnimController : MonoBehaviour
    {
        [SerializeField] private Animator anim;
        private PlayerFSM fsm;
        public void SetFloat(string key, float value) => anim.SetFloat(key, value);

        public void SetTrigger(string key) => anim.SetTrigger(key);

        public void Play(string animName)
        {
            Play(animName, fsm.animTransitionDuration, 0);
        }

        public void Play(string animName, float transitionDuration, float offsetTime)
        {
            anim.CrossFadeInFixedTime(animName, transitionDuration, 0, offsetTime);
        }
    }
}