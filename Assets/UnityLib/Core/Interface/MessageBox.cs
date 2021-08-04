using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Nettle {
    public class MessageBox : MonoSingleton<MessageBox> {
        [SerializeField]
        private Text _text;
        [SerializeField]
        private CanvasGroup _canvasGroup;
        [SerializeField]
        private float _fadeoutDuration = 1;
        [SerializeField]
        private float _fadeoutDelay = 5;

        public float Alpha {
            get {
                return _alpha;
            }
            set {
                _alpha = Mathf.Clamp01 (value);
                _canvasGroup.alpha = _alpha;
            }
        }
        private float _alpha = 0;
        private float _fadeoutTimer = 0;

        private void Update () {
            if (_fadeoutTimer > 0) {
                _fadeoutTimer -= Time.deltaTime;
            } else {
                if (_alpha > 0) {
                    _alpha -= Time.deltaTime/_fadeoutDuration;
                }
            }
            _canvasGroup.alpha = _alpha;
        }

        public void ShowMessage (string message, float duration = 0) {
            _fadeoutTimer = duration>0 ? duration: _fadeoutDelay;
            Alpha = 1;
            _text.text = message;          
        }
    }
}