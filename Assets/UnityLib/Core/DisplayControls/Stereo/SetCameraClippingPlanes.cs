using System.Collections;
using UnityEngine;

namespace Nettle {

    public class SetCameraClippingPlanes : MonoBehaviour {
        private bool _enabled = true;
        [SerializeField]
        private float _minNearClip = 1;
        [SerializeField]
        private float _maxSceneDepth = 0.0f;

        private const float _magicValue = 100.0f;

        public Camera Cam;
        public StereoEyes Eyes;

        [ConfigField]
        public bool Enabled {
            get {
                return _enabled;
            }

            set {
                _enabled = value;
            }
        }

        [ConfigField]
        public float MinNearClip {
            get {
                return _minNearClip;
            }

            private set {
                _minNearClip = value;
            }
        }

        [ConfigField]
        public float MaxSceneDepth {
            get {
                if (VisibilityZoneViewer.Instance != null && VisibilityZoneViewer.Instance.ActiveZone != null && VisibilityZoneViewer.Instance.ActiveZone.OverrideMaxSceneDepth) {
                    return VisibilityZoneViewer.Instance.ActiveZone.MaxSceneDepth;
                }
                return _maxSceneDepth;
            }

            set {
                _maxSceneDepth = value;
            }
        }

        private static SetCameraClippingPlanes _instance;

        public static SetCameraClippingPlanes Instance {
            get {
                return _instance;
            }
        }

        private void Reset () {
            if (!Cam) {
                Cam = GetComponent<Camera> ();
            }
            if (!Eyes) {
                SceneUtils.FindObjectIfSingle (ref Eyes);
            }
        }

        private void Awake () {
            if (_instance == null) {
                _instance = this;
            }
#if UNITY_WEBGL
            Enabled = false;
#endif
        }

        private void Start () {
            if (!Cam) {
                Cam = GetComponent<Camera> ();
            }
        }

        private void Update () {
            CalculateClips ();
        }

        public void CalculateClips () {
            if (!Enabled || !Cam || !Eyes) {
                return;
            }
            Cam.farClipPlane = Eyes.transform.position.y - MaxSceneDepth;
            Cam.farClipPlane += Cam.farClipPlane / 3;
            Cam.nearClipPlane = Cam.farClipPlane / _magicValue * MinNearClip;
        }
    }
}