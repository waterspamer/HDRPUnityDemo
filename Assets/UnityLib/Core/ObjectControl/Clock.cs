using System;
using UnityEngine;
using System.Collections;

namespace Nettle {

    public class Clock : MonoBehaviour {

        public Transform HourHand;
        public Transform MinuteHand;
        public Transform SecondHand;

        public float TimeScale = 1;
        public float RotationZOffset = 0.0f;
        public bool ResetOnEnable = true;

        [SerializeField]
        private int HoursInCircle = 12;
        [SerializeField]
        private int MinutesInCircle = 60;
        [SerializeField]
        private int SecondsInCircle = 60;

        private Vector3 _hourHandStartRotation;
        private Vector3 _minuteHandStartRotation;
        private Vector3 _secondHandStartRotation;
        [SerializeField]
        private bool _ticking = false;
        public bool Ticking {
            get {
                return _ticking;
            }
            set {
                _ticking = value;
            }
        }
        [SerializeField]
        private bool _showRealTime = true;
        private IEnumerator _coroutine;

        private DateTime _currentTime = new DateTime(0);

        private void Start() {
            _coroutine = UpdateClocks();
            StartCoroutine(_coroutine);


            if (HourHand != null) {
                _hourHandStartRotation = HourHand.transform.localEulerAngles;
            }
            if (MinuteHand != null) {
                _minuteHandStartRotation = MinuteHand.transform.localEulerAngles;
            }
            if (SecondHand != null) {
                _secondHandStartRotation = SecondHand.transform.localEulerAngles;
            }
        }

        private void OnEnable() {
            if (ResetOnEnable) {
                ResetTime();
            }
            if (_coroutine != null) {
                StartCoroutine(_coroutine);
            }
        }

        private void OnDisable() {
            if (_coroutine != null) {
                StopCoroutine(_coroutine);
            }
        }

        public void ResetTime() {
            _ticking = true;
            _currentTime = new DateTime(0);
        }

        public void Pause() {
            _ticking = false;
        }


        private IEnumerator UpdateClocks() {
            while (true) {
                if (!_ticking) {
                    yield return null;
                    continue;
                }
                yield return null;
                if (_showRealTime) {
                    _currentTime = DateTime.Now;
                } else {
                    _currentTime = _currentTime.AddSeconds(Time.deltaTime * TimeScale);
                }
                var hourHandRotation = (_currentTime.Hour % (float)HoursInCircle) / HoursInCircle * 360.0f + _currentTime.Minute / MinutesInCircle * (360.0f / HoursInCircle) + RotationZOffset;
                var minuteHandRotation = _currentTime.Minute / (float)MinutesInCircle * 360.0f + RotationZOffset;
                var secondHandRotation = _currentTime.Second / (float)SecondsInCircle * 360.0f + RotationZOffset;

                if (HourHand != null) {
                    HourHand.localRotation = Quaternion.Euler(_hourHandStartRotation.x, _hourHandStartRotation.y, hourHandRotation);
                }

                if (MinuteHand != null) {
                    MinuteHand.localRotation = Quaternion.Euler(_minuteHandStartRotation.x, _minuteHandStartRotation.y, minuteHandRotation);
                }

                if (SecondHand != null) {
                    SecondHand.localRotation = Quaternion.Euler(_secondHandStartRotation.x, _secondHandStartRotation.y, secondHandRotation);
                }
            }
        }
    }
}
