using UnityEngine;

namespace Nettle {

[RequireComponent(typeof(Animator))]
public class MotionParallaxForceLODForAnimator: MotionParallaxForceLOD {


    public Animator Animator;
    public int DisableAnimatorOnLOD = -1;
    private int _lastLODLevel = 0;

    protected override void Reset() {
        base.Reset();
        if (!Animator) {
            Animator = GetComponent<Animator>();
        }
    }

    protected override void OnForceLOD() {
        if (_lastLODLevel != _currentLODLevel) { //Some performance improve
            _lastLODLevel = _currentLODLevel;
            if (_currentLODLevel >= DisableAnimatorOnLOD) {
                if (Animator.enabled) {
                    Animator.enabled = false;
                }
            } else {
                if (!Animator.enabled) {
                    Animator.enabled = true;
                }
            }
        }
    }

}
}
