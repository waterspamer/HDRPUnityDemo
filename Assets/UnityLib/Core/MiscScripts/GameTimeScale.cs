using System;
using UnityEngine;
using System.Collections;
using UnityEngine.Events;

namespace Nettle {

    public class GameTimeScale : MonoBehaviour {

        [Serializable]
        public class UnityFloatEvent : UnityEvent<float> { }

        public bool GuiMode = false;
        public bool ChangeFixedTime = true;
        public float Scale = 1.0f;
        public float TransitionDuration = 0.5f;
        public UnityFloatEvent ScaleChanged = new UnityFloatEvent();

        private bool _isTransiting = false;
        private float _TransitionTimeLeft;


        private bool _paused;
        private float DefaultFixedDeltaTime;


        private void Awake() {
            DefaultFixedDeltaTime = Time.fixedDeltaTime;
            Set();
        }

        private void Set() {
            Time.timeScale = Scale;
            Time.fixedDeltaTime = DefaultFixedDeltaTime * Scale;
            ScaleChanged.Invoke(Scale);
        }

        public void SetScale(float scale) {
            _isTransiting = false;
            Scale = scale;
            if (!_paused) {
                Set();
            }
        }

        public void SetScaleWithTransition(float scale) {
            _isTransiting = true;
            StartCoroutine(SetScaleTransitionCoroutine(Time.timeScale, scale));
        }

        public IEnumerator SetScaleTransitionCoroutine(float fromScale, float toScale) {
            _TransitionTimeLeft = TransitionDuration;
            while (_TransitionTimeLeft > 0 && _isTransiting) {
                if (!_paused) {
                    _TransitionTimeLeft -= Time.unscaledDeltaTime;
                    Scale = Mathf.Lerp(fromScale, toScale, Mathf.InverseLerp(TransitionDuration, 0, _TransitionTimeLeft));
                    Set();
                    yield return null;
                }
            }
        }


        public void TogglePause() {
            if (_paused) {
                Unpause();
            } else {
                Pause();
            }
        }

        public void Pause() {
            _paused = true;
            Time.timeScale = 0;
            ScaleChanged.Invoke(0);
        }

        public void Unpause() {
            _paused = false;
            SetScale(Scale);
        }

        public void ResetScale() {
            Scale = 1.0f;
            SetScale(Scale);
        }

        void OnGUI() {
            float minTimeScale = 0;
            float maxTimeScale = 30;
            if (GuiMode) {
                int hOffset = 50;
                int vOffset = 50;
                int height = 50;
                var sliderRect = new Rect(hOffset, vOffset, Screen.width - hOffset * 2, height);
                Scale = GUI.HorizontalSlider(sliderRect, Time.timeScale, minTimeScale, maxTimeScale);

                Scale = Mathf.Clamp(Scale, minTimeScale, maxTimeScale);

                GUI.Label(new Rect(sliderRect.x, sliderRect.y + sliderRect.height, sliderRect.width, sliderRect.height), "Current scale: " + Time.timeScale);

                int buttonWidth = 100;
                int buttonHeight = 30;

                if (GUI.Button(new Rect((Screen.width - buttonWidth) * 0.5f, sliderRect.y + sliderRect.height, buttonWidth, buttonHeight), "Reset")) {
                    Scale = 1.0f;
                }
                Time.timeScale = Scale;
            }

        }
    }
}