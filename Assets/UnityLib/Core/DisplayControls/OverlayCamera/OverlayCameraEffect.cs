using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nettle {

    public class OverlayCameraEffect : MonoBehaviour {

        public Camera OverlayCamera;
        public Material BlendMaterial;
        [SerializeField]
        private bool _useBlendMaterialInstance = true;
        [SerializeField]
        private bool _copyMainCamera;
        [Range (0, 1)]
        [SerializeField]
        private float _blendValue = 1;
        public float BlendValue {
            get {
                return _blendValue;
            }
        }

        [SerializeField]
        private bool _flipUnderlyingImage = false;
        private float _blendChangeSpeed = 0;
        private float _targetBlend = 1;
        private bool _blendTransitionRunnung = false;
        private Material _blendMaterial;
        private Camera _mainCamera;
        private RenderTexture _renderTexture;
        private bool _renderingOverlay = false;

        private void Awake () {
            if (OverlayCamera == null || BlendMaterial == null) {
                enabled = false;
                Debug.LogError ("Overlay Camera image effect was not set up properly");
                return;
            }
            if (_useBlendMaterialInstance) {
                _blendMaterial = Instantiate (BlendMaterial);
            } else {
                _blendMaterial = BlendMaterial;
            }
            //We'll render the camera manually
            OverlayCamera.enabled = false;
            if (_mainCamera == null) {
                _mainCamera = GetComponent<Camera> ();
            }
        }

        public void SetMainCamera (Camera camera) {
            _mainCamera = camera;
        }

        public void OnDestroy () {
            if (_renderTexture != null) {
                _renderTexture.Release ();
            }
        }

        private void Update () {
            if (_blendTransitionRunnung) {
                if (Mathf.Abs (_blendValue + _blendChangeSpeed * Time.deltaTime - _targetBlend) > Mathf.Abs (_blendChangeSpeed * Time.deltaTime)) {
                    _blendValue += _blendChangeSpeed * Time.deltaTime;
                } else {
                    _blendValue = _targetBlend;
                    _blendTransitionRunnung = false;
                }
            }
        }

        private void OnRenderImage (RenderTexture source, RenderTexture destination) {
            if (!_renderingOverlay) {
                RenderOverlay (source, destination);
            } else {
                Graphics.Blit (source, destination);
            }
        }

        public void RenderOverlay (RenderTexture source, RenderTexture destination) {
            if (_renderTexture == null) {
                _renderTexture = new RenderTexture (source.width, source.height, 32);
                _renderTexture.filterMode = FilterMode.Trilinear;
            }
            if (_copyMainCamera) {
                OverlayCamera.projectionMatrix = _mainCamera.projectionMatrix;
                OverlayCamera.fieldOfView = _mainCamera.fieldOfView;
            }
            _renderTexture.Create ();
            OverlayCamera.targetTexture = _renderTexture;
            _renderingOverlay = true;
            OverlayCamera.Render ();
            _renderingOverlay = false;
            _blendMaterial.SetTexture ("_OverlayTex", _renderTexture);
            _blendMaterial.SetFloat ("_BlendValue", _blendValue);
            if (_flipUnderlyingImage) {
                _blendMaterial.EnableKeyword ("FLIP_UVS");
            } else {
                _blendMaterial.DisableKeyword ("FLIP_UVS");
            }
            Graphics.Blit (source, destination, _blendMaterial);
            _renderTexture.Release ();
        }

        public void SetBlendValue (float target, float transitionTime) {
            target = Mathf.Clamp01 (target);
            if (transitionTime <= 0) {
                _blendValue = target;
            } else {
                _blendTransitionRunnung = true;
                _blendChangeSpeed = (target - _blendValue) / transitionTime;
                _targetBlend = target;
            }
        }

        public void SetFullOverlay (float transitionTime) {
            SetBlendValue (1, transitionTime);
        }

        public void SetNoOverlay (float transitionTime) {
            SetBlendValue (0, transitionTime);
        }

    }
}