using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nettle {
    public class AnimatorHierarchyRebind : MonoBehaviour {
        public void Rebind() {
            Animator[] animators = GetComponentsInChildren<Animator>();
            foreach (Animator anim in animators) {
                anim.Rebind();
                AnimationRandomizer rnd = anim.GetComponent<AnimationRandomizer>();
                if (rnd != null) {
                    rnd.Randomize();
                }
            }
        }
    }
}