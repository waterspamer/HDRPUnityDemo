using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Nettle {

    public class EventAtAnimationTime : MonoBehaviour {
        public Animator Animator;
        public string TargetStateName = "";

        public bool UseNormalizedTime = true;
        public bool UseMaxTime = false;
        [FormerlySerializedAs("TargetNormalizedTime")]
        public float TargetTime = 0;
        [ConditionalHide("UseMaxTime", true)]
        public float MaxTime = 1;

        public UnityEvent OnTargetTimeReached;
        private bool _fired = false;
        
        private void Update() {
            if (Animator == null) {
                return;
            }
            AnimatorStateInfo state = Animator.GetCurrentAnimatorStateInfo(0);
            if (TargetStateName != "" && !state.IsName(TargetStateName)) {
                return;
            }
            float totalLength = UseNormalizedTime ? 1 : state.length;
            float currentTime = state.normalizedTime * totalLength;
            
            if (state.loop)
            {
                currentTime %= totalLength;
            }

            bool maxTimeCheck = UseMaxTime && currentTime > MaxTime;

            if (currentTime  >= TargetTime &&!maxTimeCheck) {
                if (!_fired) {
                    _fired = true;
                    FireEvent();
                }
            }
            else {
                _fired = false;
            }
        }
        public void FireEvent()
        {
            if (OnTargetTimeReached != null)
            {
                OnTargetTimeReached.Invoke();
            }
        }
    }
}
