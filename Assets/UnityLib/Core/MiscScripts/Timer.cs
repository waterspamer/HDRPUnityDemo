using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Nettle {

    public class Timer : MonoBehaviour {

        public float Time = 60f;
        [Tooltip("Time line direction")]
        public bool Forward;
        public string TextAfterComplete = "Time over";
        public string TimeFormat = "{0:D2}h:{1:D2}m:{2:D2}s.{3:D3}ms";
        public Color FlashingColor = Color.red;
        public float FlashingTime = 5;
        public float FlashingInterval = 1;
        public Text Text;
        public UnityEvent TimerCompleteEvent;

        private float _startTimer;
        private Color _defaultColor;
        private string _defaultText;
        private Coroutine _timerCoroutine;

        void Reset() {
            if (Text == null) {
                Text = GetComponent<Text>();
            }
        }

        void Awake() {
            _defaultColor = Text.color;
            _defaultText = Text.text;
        }

        void OnDisable() {
            AbortTimer();
        }

        public void ToggleTimer() {
            if (_timerCoroutine == null) {
                StartTimer();
            } else {
                AbortTimer();
            }
        }

        public void StartTimer() {
            _timerCoroutine = StartCoroutine(TimerCoroutine());
        }

        public void StopTimer() {
            if (_timerCoroutine != null) {
                StopCoroutine(_timerCoroutine);
            }
        }

        public void AbortTimer() {
            Text.color = _defaultColor;
            Text.text = _defaultText;
            if (_timerCoroutine != null) {
                StopCoroutine(_timerCoroutine);
                _timerCoroutine = null;
            }
        }

        IEnumerator TimerCoroutine() {
            _startTimer = UnityEngine.Time.time;
            while (true) {
                float timePassed = UnityEngine.Time.time - _startTimer;
                float timeLeft = Time - timePassed;
                bool timerComplete = timeLeft <= 0;
                string timeString;
                if (timerComplete) {
                    if (TextAfterComplete.Length == 0) {
                        timeString = TimeToString(Forward ? 0 : Time);
                    } else {
                        timeString = TextAfterComplete;
                    }
                    if (FlashingTime > 0) {
                        Text.color = _defaultColor;
                    }
                    StopCoroutine(_timerCoroutine);
                    _timerCoroutine = null;
                    TimerCompleteEvent.Invoke();
                } else {
                    if (timeLeft < FlashingTime) {
                        float t = ((FlashingTime - timeLeft) % FlashingInterval) / FlashingInterval;
                        Color fromColor;
                        Color toColor;
                        if (t < 0.5) {
                            fromColor = _defaultColor;
                            toColor = FlashingColor;
                        } else {
                            fromColor = FlashingColor;
                            toColor = _defaultColor;
                        }
                        Text.color = Color.Lerp(fromColor, toColor, t);
                    }
                    timeString = TimeToString(Forward ? timePassed : timeLeft);
                }
                Text.text = timeString;
                yield return null;
            }
        }

        string TimeToString(float time) {
            TimeSpan t = TimeSpan.FromSeconds(time);
            return string.Format(TimeFormat, t.Hours, t.Minutes, t.Seconds, t.Milliseconds);
        }
    }
}
