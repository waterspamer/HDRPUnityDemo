using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nettle {

    [ExecuteAfter(typeof(VisibilityZoneViewer))]
    public class Rotate360 : MonoBehaviour {

        public Transform Target;
        [Tooltip("Speed in degrees per second. Sign controls direction")]
        public float RotationSpeed = 20;
        public Vector3 RotationAxis = Vector3.up;
        private Quaternion _oldRotation;
        private Quaternion _defaultRotation;
        private bool _active = true;

        private void Awake() {
            if (Target == null) {
                Target = transform;
            }
        }

        private void Start() {
            _defaultRotation = Target.localRotation;
        }

        private void OnEnable() {
            _oldRotation = Target.localRotation;
        }

        private void LateUpdate() {
            if (_active) {
                Target.localRotation = _oldRotation;
                Target.Rotate(RotationAxis.normalized * Time.deltaTime * RotationSpeed);
                _oldRotation = Target.localRotation;
            }
        }

        public void SetRotation(float angle) {
            Target.localRotation = _defaultRotation;
            Target.Rotate(RotationAxis.normalized * angle);
            _oldRotation = Target.localRotation;
        }
    }
}
