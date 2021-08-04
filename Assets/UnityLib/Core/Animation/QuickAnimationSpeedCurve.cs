using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuickAnimationSpeedCurve : MonoBehaviour
{
    [SerializeField]
    private AnimationCurve _speed;
    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (_animator.speed != 0)
        {
            float time = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1;
            _animator.speed = _speed.Evaluate(time);
        }
    }
}
