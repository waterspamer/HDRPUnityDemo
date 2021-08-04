using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Serialization;

namespace Nettle {
    public class AmbientOcclusionScaler : MonoBehaviour {
        public PostProcessVolume _postProcessVolume;
        [Obsolete] //May remove after uptade Ispanskie_Kyartaly 
        [HideInInspector]
        public Transform GeneralPlan;
        [SerializeField]
        [Tooltip("Starts from this height AO intensity and Radius will be minimal")]
        [FormerlySerializedAs("_applyMinAOSettingsHeight")]
        private float _minHeight;
        [SerializeField]
        private float _maxHeight;
        [SerializeField]
        private float _minRadius;
        [SerializeField]
        private float _maxRadius;
        [SerializeField]
        private float _minIntensity;
        [SerializeField]
        private float _maxIntensity;
        private AmbientOcclusion _ambientOcclusionLayer;

        public void OnValidate() {
            if (GeneralPlan && _maxHeight == 0) {
                _maxHeight = GeneralPlan.localScale.y;
            }
        }

        void Start() {
            _postProcessVolume.profile.TryGetSettings(out _ambientOcclusionLayer);
        }

        void Update() {
            ScaleAmbientOcclusion();
        }

        private void ScaleAmbientOcclusion() {
            _ambientOcclusionLayer.radius.value = Mathf.Lerp(_minRadius, _maxRadius,
                Mathf.Max(0.01f, (MotionParallaxDisplay.Instance.transform.localScale.y - _minHeight) / _maxHeight));
            _ambientOcclusionLayer.intensity.value = Mathf.Lerp(_minIntensity, _maxIntensity,
                Mathf.Max(0.01f, (MotionParallaxDisplay.Instance.transform.localScale.y - _minHeight) / _maxHeight));
        }
    }
}