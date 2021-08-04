using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Nettle {

    public sealed class NettleBoxTracking : MonoBehaviour {
        [SerializeField]
        private bool _manualMode = false;
        [SerializeField]
        private bool _useTrackingDll = false;
        [Tooltip("If this value is above 0, the camera will rotate at a fixed distance from the target (might be useful for remote display)")]
        [SerializeField]
        private float _fixedDistance = 0;
        [SerializeField]
        private float _fixedAngle = 0;
        [SerializeField]
        private Vector3 _leftEye;
        [SerializeField]
        private Vector3 _rightEye;
        [SerializeField]
        private Vector3 _defaultEyesCenter = new Vector3(0, 0.5f, -0.5f);
        [SerializeField]
        private float _timeBeforeResetEyes = 120f;
        [SerializeField]
        private float _extrapolateByTime = 0.035f;
        private bool _debugMode = false;
        public StereoEyes Eyes;
        public MotionParallaxDisplay Display;


        private string _message;
        private float _lastGetEyeTime;

        [ConfigField]
        public float FixedDistance { get => _fixedDistance; set => _fixedDistance = value; }
        [ConfigField]
        public float FixedAngle { get => _fixedAngle; set => _fixedAngle = value; }
        [ConfigField]
        public Vector3 LeftEye { get => _leftEye; set => _leftEye = value; }
        [ConfigField]
        public Vector3 RightEye { get => _rightEye; set => _rightEye = value; }
        [ConfigField]
        public Vector3 DefaultEyesCenter { get => _defaultEyesCenter; set => _defaultEyesCenter = value; }
        [ConfigField]
        public bool ManualMode { get => _manualMode; set => _manualMode = value; }
        [ConfigField]
        public float TimeBeforeResetEyes { get => _timeBeforeResetEyes; set => _timeBeforeResetEyes = value; }
        [ConfigField]
        public float ExtrapolateByTime { get => _extrapolateByTime; set => _extrapolateByTime = value; }

        public bool Active {get; private set; }

        private Vector3 _lastPosition;

        #region Tracking Dll

        #if !UNITY_WEBGL

        [DllImport("Tracking", CallingConvention = CallingConvention.StdCall)]
        private static extern bool GetEyes(float timeShift, out Vector3 leftEye, out Vector3 rigthEye);

        [DllImport("Tracking", CallingConvention = CallingConvention.StdCall)]
        private static extern void TrackingCreate();

        [DllImport("Tracking", CallingConvention = CallingConvention.StdCall)]
        private static extern void TrackingDestroy();

        [DllImport("Tracking", CallingConvention = CallingConvention.StdCall)]
        private static extern bool IsTrackingCreated();
        #else        
        private static bool GetEyes(float timeShift, out Vector3 leftEye, out Vector3 rigthEye){
            leftEye = Vector3.zero;
            rigthEye = Vector3.zero;
            return false;
        }
        private static void TrackingCreate()
        {
        }
        private static void TrackingDestroy()
        {
        }
        private static bool IsTrackingCreated()
        {
            return false;
        }
#endif

        #endregion

        private void Reset() {
            if (Eyes == null) {
                Eyes = SceneUtils.FindObjectIfSingle<StereoEyes>();
            }
            if (Display == null) {
                Display = SceneUtils.FindObjectIfSingle<MotionParallaxDisplay>();
            }
            ResetEyes();
            UpdateTransform();
        }

        private void Awake() {
            try {
                if (!Application.isEditor || (Application.isEditor && _useTrackingDll)) {
                    Debug.Log("Tracking created. Scene: " + SceneManager.GetActiveScene().name);
                    TrackingCreate();
                    ManualMode = false;
                }
                else {
                    ManualMode = true;
                }
            }
            catch (DllNotFoundException ex) {
                ManualMode = true;
                Debug.Log(ex.Message);
            }
            ResetEyes();
            UpdateTransform();
        }

        private void OnDestroy() {
            try {
                if (!Application.isEditor || (Application.isEditor && _useTrackingDll)) {
                    Debug.Log("Tracking destroyed. Scene: " + SceneManager.GetActiveScene().name);
                    TrackingDestroy();
                }
            }
            catch (DllNotFoundException ex) {
                Debug.Log(ex.Message);
            }
        }

        private void OnGUI() {
            if (_debugMode) {
                GUILayout.Label(string.Format("Left: {0}; Right: {1}", LeftEye, RightEye));
                GUILayout.Label(_message);

                if (Input.GetKeyDown(KeyCode.F7)) {
                    TrackingCreate();
                }

                if (Input.GetKeyDown(KeyCode.F8)) {
                    TrackingDestroy();
                }

                GUILayout.Label("Tracking created: " + IsTrackingCreated().ToString());
                GUILayout.Label("F7 = TrackingCreate\tF8 = TrackingDestroy");
            }
        }

        public void UpdateTracking() {
            if (!ManualMode) {
                UpdateEyes();
                UpdateTransform();
            }
        }

        private void UpdateEyes() {
            float eyesScale = Display.Width / 2;
            try {
                if (!Application.isEditor || (Application.isEditor && _useTrackingDll)) {
                    if (GetEyes(ExtrapolateByTime, out Vector3 left, out Vector3 right)) {
                        Active = true;

                        LeftEye = new Vector3(left.x, left.z, left.y);
                        RightEye = new Vector3(right.x, right.z, right.y);

                        if (Display != null) {
                            LeftEye *= eyesScale;
                            RightEye *= eyesScale;
                        }

                        Eyes.DistanceBetweenEyes = Vector3.Distance(LeftEye, RightEye);
                        _lastGetEyeTime = Time.time;
                    }
                    else {
                        Active = false;
                        _message = "Don't get eyes";
                        if (Eyes.CameraMode == EyesCameraMode.Mono && _lastPosition != transform.localPosition) {
                            _lastPosition = transform.localPosition;
                            _lastGetEyeTime = Time.time;
                        }
                        if (_lastGetEyeTime + TimeBeforeResetEyes < Time.time) {
                            ResetEyes();
                            _lastGetEyeTime = Time.time;
                            _lastPosition = transform.localPosition;
                        }
                    }
                }
                else {
                    ResetEyes();
                }
            }
            catch (DllNotFoundException ex) {
                ManualMode = true;
                ResetEyes();

                Debug.Log(ex.Message);
            }
        }

        private void UpdateTransform() {
            Vector3 nose = (LeftEye + RightEye) * 0.5f;
            if (FixedDistance > 0) {
                nose = nose.normalized * FixedDistance;
            }
            if (FixedAngle > 0) {
                Vector3 proj = Vector3.ProjectOnPlane(nose, Vector3.up).normalized;
                float yAngle = Vector3.Angle(Vector3.forward, proj);
                if (proj.x < 0) {
                    yAngle = -yAngle;
                }
                Vector3 noseRotated = Quaternion.Euler(-Mathf.Abs(FixedAngle), yAngle, 0) *Vector3.forward;
                nose = nose.magnitude * noseRotated;
            }
            transform.localPosition = nose;
            if (Vector3.Distance(LeftEye, RightEye) > 0.0001f) {
                Vector3 forward = (-nose).normalized;
                Vector3 right = (RightEye - LeftEye).normalized;
                Vector3 localUp = Vector3.Cross(forward, right).normalized;
                forward = Vector3.Cross(right, localUp).normalized;
                transform.localRotation = Quaternion.LookRotation(forward, localUp);
            }
        }


        public void ResetEyes() {
            float eyesScale = Display == null ? 1.0f : Display.Width / 2;
            Vector3 eyesCenter = Vector3.up;
            if (!Application.isEditor) {
                eyesCenter = DefaultEyesCenter;
            }
            LeftEye = (eyesCenter + Eyes.LeftEyeLocal) * eyesScale;
            RightEye = (eyesCenter + Eyes.RightEyeLocal) * eyesScale;
        }

        public void ResetTracking() {
            ResetEyes();
            UpdateEyes();
            UpdateTransform();
        }
    }
}
