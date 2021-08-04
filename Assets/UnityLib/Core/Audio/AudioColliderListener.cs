using UnityEngine;

namespace Nettle {

    public class AudioColliderListener : MonoBehaviour {

        public Transform DisplayTransform;
        public MotionParallaxDisplay Display;
        public bool InitialState = true;
        private static bool _muted = false;
        private bool _mutedBeforeDisable = false;
        private static AudioColliderListener _instance;
        public static bool Muted {
            get {
                return _muted || _instance == null || !_instance.gameObject.activeInHierarchy || !_instance.enabled;
            }
        }

        private void Awake() {
            SetMuted(InitialState);
            _instance = this;
        }

        void Update() {
            transform.position = new Vector3(DisplayTransform.position.x, DisplayTransform.localScale.y, DisplayTransform.position.z);
            transform.localScale = new Vector3(DisplayTransform.localScale.x, 1, DisplayTransform.localScale.z);
            transform.localRotation = DisplayTransform.localRotation;
        }

        public void SetMuted(bool value) {
            _muted = value;
        }

    }
}
