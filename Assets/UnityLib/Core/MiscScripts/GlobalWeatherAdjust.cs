using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Nettle {
    public class GlobalWeatherAdjust : MonoBehaviour {
        [SerializeField]
        private float _hardness = 1;
        private MotionParallaxDisplay _display;
        private bool _switchThisFrame = false;

        void Start() {
            _display = SceneUtils.FindObjectsOfType<MotionParallaxDisplay>(true).First();
            InstantMatch(_display.transform);
            VisibilityZoneViewer.Instance.OnShowZone.AddListener(OnShowZone);
        }

        private void OnShowZone(VisibilityZone zone) {
            InstantMatch(zone.transform);
        }

        void Update() {
            if (_display == null) {
                return;
            }
            if (_switchThisFrame) {
                _switchThisFrame = false;
                return;
            }
            transform.position = Vector3.Lerp(transform.position, _display.transform.position, Time.deltaTime * _hardness);
            transform.rotation = Quaternion.Lerp(transform.rotation, _display.transform.rotation, Time.deltaTime * _hardness);
            if (transform.localScale.magnitude > _display.transform.localScale.magnitude) {
                transform.localScale = Vector3.Lerp(transform.localScale, _display.transform.localScale, Time.deltaTime * _hardness);
            }
            else {
                transform.localScale = _display.transform.localScale;
            }
        }

        private void InstantMatch(Transform t) {
            Debug.Log("matchSnow");
            transform.position = t.position;
            transform.rotation = t.rotation;
            transform.localScale = t.localScale;
            _switchThisFrame = true;
        }
    }
}