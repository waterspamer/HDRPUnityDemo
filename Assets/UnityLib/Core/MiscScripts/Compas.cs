using UnityEngine;

namespace Nettle {

    public class Compas : MonoBehaviour {
        [SerializeField]
        [Tooltip("If the letters must always face user, put them into this array")]
        private Transform[] _letters;
        [SerializeField]
        private Vector3 _localUpAxis = new Vector3(0,0,1);
        [SerializeField]
        private Vector3 _localForwardAxis = new Vector3(0,-1,0);


        private Quaternion _defaultRotation;
        private NettleBoxTracking _tracking;

        void Awake() {
            _defaultRotation = transform.rotation;
        }

        void Start() {
            if (_letters.Length > 0) {
                _tracking = FindObjectOfType<NettleBoxTracking>();
            }
        }

        void LateUpdate() {
            transform.rotation = _defaultRotation;
            if (_letters.Length > 0 && _tracking.Active) {
                // Vector3 eyesOffset =transform.InverseTransformPoint(_eyes.transform.position);
                // eyesOffset = Vector3.ProjectOnPlane(eyesOffset, _localUpAxis);
                Vector3 eyesOffset = _tracking.transform.position-transform.position;
                Vector3 globalUpAxis = transform.TransformDirection(_localUpAxis);
                eyesOffset = Vector3.ProjectOnPlane(eyesOffset, globalUpAxis);
                Quaternion lettersRotation = Quaternion.LookRotation(eyesOffset, globalUpAxis);
                lettersRotation *= Quaternion.FromToRotation(_localForwardAxis, Vector3.forward);
                foreach (Transform t in _letters) {
                    t.rotation = lettersRotation;
                }
            }
        }
    }
}
