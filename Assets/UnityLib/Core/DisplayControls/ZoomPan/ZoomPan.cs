using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace Nettle {
    [ExecuteAfter(typeof(VisibilityZoneViewer))]
    public class ZoomPan : MonoBehaviour {
        [FormerlySerializedAs("Target")]
        public Transform Display;
        public NettleBoxTracking Tracking;

        public event Action<Vector3> OnMoved;
        public event Action<float> OnZoomed;

        private float _currentZoom = 1.0f;
        private Vector3 _startScale;

        [SerializeField]
        [FormerlySerializedAs("MoveEnabled")]
        private bool _moveEnabled = true;
        [SerializeField]
        [FormerlySerializedAs("SwapXZ")]
        private bool _swapXZ = false;
        [SerializeField]
        [FormerlySerializedAs("ZoomEnabled")]
        private bool _zoomEnabled = true;
        [SerializeField]
        [FormerlySerializedAs("ZoomSpeed")]
        private float _zoomSpeed = 0.8f;
        [SerializeField]
        [FormerlySerializedAs("MinZoom")]
        private float _minZoom = 0.1f;
        [SerializeField]
        [FormerlySerializedAs("MaxZoom")]
        private float _maxZoom = 10.0f;
        [SerializeField]
        [FormerlySerializedAs("MoveRelativeToUser")]
        private bool _moveRelativeToUser = false;
        [SerializeField]
        [FormerlySerializedAs("LocalSpace")]
        private bool _localSpace = true;
        [SerializeField]
        [FormerlySerializedAs("ViewUnits")]
        private bool _viewUnits = true;
        [SerializeField]
        [FormerlySerializedAs("Target")]
        private bool _flipXAxis = false;

        public float CurrentZoom { get => _currentZoom; set => _currentZoom = value; }
        public bool MoveEnabled {
            get {
                return _moveEnabled && !IsPanBlocked();
            }
            set {
                _zoomEnabled = value;
            }
        }
        public bool ZoomEnabled {
            get {
                return _zoomEnabled&& !IsZoomBlocked();
            }
            set {
                _zoomEnabled = value;
            }
        }
        public bool SwapXZ { get => _swapXZ; set => _swapXZ = value; }
        public float ZoomSpeed { get => _zoomSpeed; set => _zoomSpeed = value; }
        public float MinZoom { get => _minZoom; set => _minZoom = value; }
        public float MaxZoom { get => _maxZoom; set => _maxZoom = value; }
        public bool MoveRelativeToUser { get => _moveRelativeToUser; set => _moveRelativeToUser = value; }
        public bool LocalSpace { get => _localSpace; set => _localSpace = value; }
        public bool ViewUnits { get => _viewUnits; set => _viewUnits = value; }
        public bool FlipXAxis { get => _flipXAxis; set => _flipXAxis = value; }

        public List<IZoomPanBlocker> Blockers = new List<IZoomPanBlocker>();

        void Start() {
            _startScale = Display.transform.lossyScale;
        }

        void Reset() {
            if (!Display) {
                var mpDisplay = SceneUtils.FindObjectIfSingle<MotionParallaxDisplay>();
                if (mpDisplay) {
                    Display = mpDisplay.transform;
                }
            }
            if (!Tracking) {
                Tracking = SceneUtils.FindObjectIfSingle<NettleBoxTracking>();
            }
        }

        public void SetStartScale(Vector3 startScale, bool resetZoom = true) {
            _startScale = startScale;
            if (resetZoom) {
                CurrentZoom = 1.0f;
            }
        }

        public void ToggleMoveRelativeToUser() {
            MoveRelativeToUser = !MoveRelativeToUser;
        }

        public void Move(float axisX, float axisY) {
            if (MoveEnabled) {
                var offset = new Vector3 {
                    x = (SwapXZ ? axisY : axisX) * (FlipXAxis ? 1.0f : -1.0f),
                    z = (SwapXZ ? axisX : axisY) * (FlipXAxis ? 1.0f : -1.0f)
                };

                if (MoveRelativeToUser) {
                    var right = Vector3.Normalize(Tracking.RightEye - Tracking.LeftEye);
                    var front = Vector3.Normalize(Vector3.Cross(right, Vector3.up));
                    right = Vector3.Normalize(Vector3.Cross(Vector3.up, front));
                    offset = right * offset.x + front * offset.z;
                } else if (LocalSpace) {
                    var right = Display.transform.right;
                    var front = Display.transform.forward;
                    offset = right * offset.x + front * offset.z;
                }

                if (offset != Vector3.zero) {
                    Display.transform.position += offset * Display.transform.localScale.x * 2;
                    OnMoved?.Invoke(Display.transform.position);
                }
            }
        }

        public void DoZoom(float axis) {
            if (ZoomEnabled) {
                CurrentZoom = Mathf.Clamp(CurrentZoom * Mathf.Pow(ZoomSpeed, axis), MinZoom, MaxZoom);
                Display.transform.localScale = _startScale * CurrentZoom;
                OnZoomed?.Invoke(CurrentZoom);
            }
        }

        public void DoZoomDirect(float scale) {
            if (ZoomEnabled) {
                CurrentZoom = Mathf.Clamp(CurrentZoom * scale, MinZoom, MaxZoom);
                Display.transform.localScale = _startScale * CurrentZoom;
                OnZoomed?.Invoke(CurrentZoom);
            }
        }

        private bool IsPanBlocked() {
            if (Blockers == null) {
                return false;
            }
            foreach (IZoomPanBlocker blocker in Blockers) {
                if (blocker != null && blocker.IsPanBlocked()) {
                    return true;
                }
            }
            return false;
        }

        private bool IsZoomBlocked() {
            if (Blockers == null) {
                return false;
            }
            foreach (IZoomPanBlocker blocker in Blockers) {
                if (blocker!=null && blocker.IsZoomBlocked()) {
                    return true;
                }
            }
            return false;
        }
    }
}
