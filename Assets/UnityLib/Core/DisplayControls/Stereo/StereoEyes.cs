using UnityEngine;
using System;
using System.Collections;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Nettle {
    public enum EyesCameraMode {
        Stereo,
        Mono,
        Left,
        Right
    }

    public enum StereoType {
        NVidia3DVision,
        FramePacking,
        RemoteDisplay
    }
    [ExecuteAfter(typeof(NettleBoxTracking))]
    [ExecuteBefore(typeof(MotionParallax3D2))]
    public sealed class StereoEyes : MonoBehaviour {
        public NettleBoxTracking NettleBoxTracking;
        public MotionParallax3D2 MotionParallax3D2;
        public Camera EyeCamera;
        [SerializeField]
        private bool _renderTwoEyesPerFrame = true;
        [SerializeField]
        private StereoType _stereoType;
        /// <summary>
        /// 
        /// </summary>
        private const uint _frameSeparatorHD = 30;
        private const uint _frameSeparatorFHD = 45;
        private IntPtr _renderEventHandler;
        private bool _cameraModeGUI = false;
        [SerializeField]
        private float _distanceBetweenEyes = 0.065f;
        [SerializeField]
        private bool _flipEyes = false;
        [SerializeField]
        private EyesCameraMode _cameraMode;

        public Action BeforeRenderEvent;

        public Vector3 LeftEyeLocal {
            get { return Vector3.right * DistanceBetweenEyes * -0.5f; }
        }
        public Vector3 RightEyeLocal {
            get { return Vector3.right * DistanceBetweenEyes * 0.5f; }
        }
        public Vector3 LeftEyeWorld {
            get { return transform.TransformPoint(LeftEyeLocal); }
        }
        public Vector3 RightEyeWorld {
            get { return transform.TransformPoint(RightEyeLocal); }
        }
        [ConfigField]
        public bool RenderTwoEyesPerFrame { get => _renderTwoEyesPerFrame; set => _renderTwoEyesPerFrame = value; }
        [ConfigField]
        public StereoType StereoType { get => _stereoType; set => _stereoType = value; }
        [ConfigField]
        public float DistanceBetweenEyes { get => _distanceBetweenEyes; set => _distanceBetweenEyes = value; }
        [ConfigField]
        public bool FlipEyes { get => _flipEyes; set => _flipEyes = value; }
        [ConfigField]
        public EyesCameraMode CameraMode { get => _cameraMode; set => _cameraMode = value; }
        [ConfigField]
        public bool LeftEyeActive { get; private set; }


        private static StereoEyes _instance;
        public static StereoEyes Instance{
            get
            {
                if (_instance==null)
                {
                    _instance = SceneUtils.FindObjectsOfType<StereoEyes>(true).FirstOrDefault();
                }
                return _instance;
            }
        }

        private void OnValidate() {
            EditorInit();
            UpdateCameraTransform();
        }

        private void Reset() {
            EditorInit();
        }

        private void EditorInit() {
            if (!NettleBoxTracking) {
                NettleBoxTracking = FindObjectOfType<NettleBoxTracking>();
            }
            if (!MotionParallax3D2) {
                MotionParallax3D2 = FindObjectOfType<MotionParallax3D2>();
            }
        }

        private void Start() {
#if UNITY_WEBGL
            StereoType = StereoType.RemoteDisplay;
            RenderTwoEyesPerFrame = false;
            CameraMode = EyesCameraMode.Mono;
#endif
            if (EyeCamera == null) {
                EyeCamera = GetComponentInChildren<Camera>();
            }
            
            Application.onBeforeRender += OnBeforeRender;
#if !UNITY_EDITOR
        if (_stereoType == StereoType.NVidia3DVision) {
            StartCoroutine(DelayedSetRenderEventHandler());
        }
#endif
        }

        private IEnumerator DelayedSetRenderEventHandler() {
            yield return null;
            SetRenderEventHandler();
        }

        private void SetRenderEventHandler() {
            _renderEventHandler = UnityStereoDll.GetRenderEventFunc();
        }

        private void OnDestroy() {
            Application.onBeforeRender -= OnBeforeRender;
        }

        public void ToggleTwoEyesPerFrame() {
            RenderTwoEyesPerFrame = !RenderTwoEyesPerFrame;
        }

        private void OnBeforeRender() {
            if (RenderTwoEyesPerFrame) {
                PrepareCameraRender();
                if (BeforeRenderEvent != null) {
                    BeforeRenderEvent.Invoke();
                }
                Debug.Log("Rendered");
                EyeCamera.Render();
            }
            PrepareCameraRender();
            if (BeforeRenderEvent != null) {
                BeforeRenderEvent.Invoke();
            }
        }

        private void PrepareCameraRender() {
            Toggle3DVisionEye();
            if (NettleBoxTracking != null) {
                NettleBoxTracking.UpdateTracking();
            }
            UpdateCameraTransform();
            if (MotionParallax3D2 != null) {
                MotionParallax3D2.LateUpdateManual();
            }
        }

        private void UpdateCameraTransform() {
            if (EyeCamera != null) {
                switch (CameraMode) {
                    case EyesCameraMode.Stereo:
                        EyeCamera.transform.position = LeftEyeActive ? LeftEyeWorld : RightEyeWorld;
                        break;
                    case EyesCameraMode.Mono:
                        EyeCamera.transform.position = transform.position;
                        break;
                    case EyesCameraMode.Left:
                        EyeCamera.transform.position = LeftEyeWorld;
                        break;
                    case EyesCameraMode.Right:
                        EyeCamera.transform.position = RightEyeWorld;
                        break;
                }
                EyeCamera.transform.rotation = transform.rotation;
            }
        }

        public Vector2 GetEyeRTSize() {
            if (StereoType == StereoType.NVidia3DVision || StereoType == StereoType.RemoteDisplay) {
                return new Vector2(Screen.width, Screen.height);
            } else if (StereoType == StereoType.FramePacking) {
                var frameSeparator = Screen.width == 1280 ? _frameSeparatorHD : _frameSeparatorFHD;
                int eyeTargetHeight = (int)(Screen.height - frameSeparator) / 2;
                return new Vector2(Screen.width, eyeTargetHeight);
            } else {
                throw new Exception("Unknown stereo mode");
            }
        }

        public Rect GetCameraViewportRect() {
            return GetCameraViewportRect(LeftEyeActive);
        }

        public Rect GetCameraViewportRect(bool leftEye) {
            if (StereoType == StereoType.NVidia3DVision || StereoType == StereoType.RemoteDisplay) {
                return EyeCamera.rect;
            } else {
                float normalizedEyeHeight = GetEyeRTSize().y / (float)Screen.height;
                if (FlipEyes) {
                    leftEye = !leftEye;
                }
                return new Rect(0, leftEye ? (1.0f - normalizedEyeHeight) : 0.0f, 1.0f, normalizedEyeHeight);
            }
        }

        public void Toggle3DVisionEye() {
            try {
                if (_renderEventHandler == IntPtr.Zero) {
                    Debug.Log("Render event handler was null");
                    SetRenderEventHandler();
                }
                LeftEyeActive = !LeftEyeActive;
                if (StereoType == StereoType.NVidia3DVision) {
#if !UNITY_EDITOR
            if(!FlipEyes){
                GL.IssuePluginEvent(_renderEventHandler, LeftEyeActive ? (int)UnityStereoDll.GraphicsEvent.SetLeftEye : (int)UnityStereoDll.GraphicsEvent.SetRightEye);
            }else{
                GL.IssuePluginEvent(_renderEventHandler, LeftEyeActive ? (int)UnityStereoDll.GraphicsEvent.SetRightEye : (int)UnityStereoDll.GraphicsEvent.SetLeftEye);
            }
#endif
                }
                EyeCamera.rect = GetCameraViewportRect();
            } catch (Exception ex) {
                Debug.LogError(ex.Message + "\n" + ex.StackTrace);
            }
        }

        private void OnDrawGizmos() {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawLine(LeftEyeLocal, RightEyeLocal);
            Gizmos.DrawLine(Vector3.zero, Vector3.forward * DistanceBetweenEyes * 0.2f);
#if UNITY_EDITOR
            Handles.Label(LeftEyeWorld, "L");
            Handles.Label(RightEyeWorld, "R");
#endif
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(LeftEyeLocal, 0.01f);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(RightEyeLocal, 0.01f);
        }

        private void Update() {
            if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKey(KeyCode.LeftControl) && Input.GetKeyUp(KeyCode.C)) {
                _cameraModeGUI = !_cameraModeGUI;
            }
        }

        public void SetMonoMode(bool mono) {
            CameraMode = mono ? EyesCameraMode.Mono : EyesCameraMode.Stereo;
        }

        private void OnGUI() {
            if (!_cameraModeGUI) { return; }

            var btnWidth = 50.0f;
            var btnHeigth = 20.0f;

            GUI.Label(new Rect(25, 25, 200, 25), "Select screen mode:");

            if (GUI.Button(new Rect(25, 50, btnWidth, btnHeigth), "Stereo")) {
                CameraMode = EyesCameraMode.Stereo;
                _cameraModeGUI = false;
            }
            if (GUI.Button(new Rect(25, 75, btnWidth, btnHeigth), "Mono")) {
                CameraMode = EyesCameraMode.Mono;
                _cameraModeGUI = false;
            }
            if (GUI.Button(new Rect(25, 100, btnWidth, btnHeigth), "Left")) {
                CameraMode = EyesCameraMode.Left;
                _cameraModeGUI = false;
            }
            if (GUI.Button(new Rect(25, 125, btnWidth, btnHeigth), "Right")) {
                CameraMode = EyesCameraMode.Right;
                _cameraModeGUI = false;
            }
        }
    }
}
