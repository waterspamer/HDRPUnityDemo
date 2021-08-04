using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nettle {
    public class AdaptiveShadowBias : MonoBehaviour {
        [SerializeField]
        private Transform _displayTransform;
        [SerializeField]
        private Light _light;
        public float MinBias = 0.05f;
        public float MaxBias = 0.6f;
        public float MinScale = 30;
        public float MaxScale = 500;

        private void Reset() {
            _light = GetComponent<Light>();
            MotionParallaxDisplay mpd = FindObjectOfType<MotionParallaxDisplay>();
            if (mpd != null) {
                _displayTransform = mpd.transform;
            }
        }

        void Update() {           
            float t = Mathf.InverseLerp(MinScale, MaxScale, _displayTransform.localScale.x);
            _light.shadowBias = Mathf.Lerp(MinBias, MaxBias, t);
        }
    }
}