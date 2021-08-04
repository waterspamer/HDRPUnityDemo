using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nettle {

    public class AnimationRandomizer : MonoBehaviour {

        private Animator[] _animators;

        // Use this for initialization
        void Awake() {
            _animators = GetComponentsInChildren<Animator>(true);
        }
        private void OnEnable() {
            Randomize();
        }

        public void Randomize() {
            foreach (Animator animator in _animators) {
                animator.Play(animator.GetCurrentAnimatorStateInfo(0).fullPathHash, 0, Random.Range(0, 1.0f));
            }
        }
    }
}
