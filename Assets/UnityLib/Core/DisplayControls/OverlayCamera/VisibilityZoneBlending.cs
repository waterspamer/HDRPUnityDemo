using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nettle {
    public class VisibilityZoneBlending : MonoBehaviour {
        private enum TransitionListBehaviour {
            Exclude, Include
        }
        [System.Serializable]
        private class ZoneTransition {
            public VisibilityZone From;
            public VisibilityZone To;
        }
        [SerializeField]
        private OverlayCameraEffect _overlayCamera;
        [SerializeField]
        private float _fadeDuration = 1;
        [Tooltip("Array to specify zone transitions to be handled. Null means any zone.")]
        [SerializeField]
        private ZoneTransition[] _zoneTransitions;
        [SerializeField]
        private TransitionListBehaviour _transitionListBehaviour;
        private Transform _cameraParent;
        private MotionParallaxDisplay _display;
        private StereoEyes _eyes;
        private MotionParallax3D2 _mp3d;
        private Camera _mainCamera;
        private float _oldZoneEyesScale;
        private float _fadeDurationLeft = 0;

        void  Awake()
        {
            if (Camera.main == null) {
                Debug.LogError("Cannot find main camera");
                return;
            }
            _overlayCamera.SetNoOverlay(0);
            _cameraParent = new GameObject("ZoneBlendingCameraParent").transform;
            _overlayCamera.transform.SetParent(_cameraParent);            
            _display = FindObjectOfType<MotionParallaxDisplay>();
            _eyes = _display.GetComponentInChildren<StereoEyes>();
            _mainCamera = Camera.main;
        }

        private void Start() {
            VisibilityZoneViewer.Instance.OnShowZone.AddListener(OnShowZone);
            _mp3d = FindObjectOfType<MotionParallax3D2>();
        }

        private void OnShowZone(VisibilityZone zone) {
            VisibilityZone oldZone = VisibilityZoneViewer.Instance.PreviousZone;
            if (!zone.FastSwitchFromPrevious() || zone == oldZone) {
                //We want to blend zones only if there is fast switch enabled between them
                return;
            }
            if (zone.Group != null && zone.Group.SwitchWithoutTransition && oldZone == zone.Group) {
                return;
            }
            if (_zoneTransitions.Length > 0) {
                bool transitionIsInList = false;
                foreach (ZoneTransition t in _zoneTransitions) {
                    if ((t.From == null || t.From == oldZone) && (t.To == null || t.To == zone)) {
                        transitionIsInList = true;
                        break;
                    } 
                }
                if ((_transitionListBehaviour == TransitionListBehaviour.Exclude) == transitionIsInList) {
                    return;
                }
            }
            _cameraParent.position = _display.transform.position;
            _cameraParent.rotation = _display.transform.rotation;
            _cameraParent.localScale = _display.transform.localScale;
            _overlayCamera.SetFullOverlay(0);
            _overlayCamera.SetNoOverlay(_fadeDuration);
            _fadeDurationLeft = _fadeDuration;
            _oldZoneEyesScale = _display.Width / 2;
            MatchCamera();
        }

        private void Update() {
            if (_fadeDurationLeft > 0) {
                _fadeDurationLeft -= Time.deltaTime;
            }
        }

        private void OnPreRender() {
            if (_fadeDurationLeft > 0) {
                MatchCamera();
            }
        }

        private void MatchCamera() {
            Vector3 eyePosition = _display.transform.InverseTransformPoint(_mainCamera.transform.position);
            _overlayCamera.transform.localPosition = eyePosition;
            _overlayCamera.transform.rotation = Quaternion.LookRotation(_cameraParent.TransformDirection(_display.transform.InverseTransformDirection(_mainCamera.transform.forward)), _cameraParent.TransformDirection(_display.transform.InverseTransformDirection(_mainCamera.transform.up)));
            if (_mp3d.UseStereoProjection) {
                MotionParallax3D2.NettleProjectionMatrix(_overlayCamera.OverlayCamera, eyePosition, _oldZoneEyesScale, _eyes.GetEyeRTSize());
            }
            else {
                _overlayCamera.OverlayCamera.fieldOfView = Camera.main.fieldOfView;
            }
        }
    }
}
