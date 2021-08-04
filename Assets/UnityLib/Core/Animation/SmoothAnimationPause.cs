using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nettle {
    public class SmoothAnimationPause : MonoBehaviour {
        [SerializeField]
        private float _startState = 1;
        [SerializeField]
        private float _maxSpeed = 1;
        [SerializeField]
        private float _minSpeed = 0;
        [SerializeField]
        private float _lerpSpeed = 1;
        [SerializeField]
        private Animator _animator;

        private float _targetSpeed;

        private void Awake () {
            SetState (_startState);
        }

        // Update is called once per frame
        void Update () {
            _animator.speed = Mathf.Lerp (_animator.speed, _targetSpeed, Time.deltaTime * _lerpSpeed);
        }

        public void SetState (float state) {
            _targetSpeed = Mathf.Lerp (_minSpeed, _maxSpeed, state);
        }
    }
}