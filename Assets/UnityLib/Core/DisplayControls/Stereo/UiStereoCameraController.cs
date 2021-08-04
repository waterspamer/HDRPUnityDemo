using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Nettle {

[RequireComponent(typeof(Camera))]
public class UiStereoCameraController: MonoBehaviour {
        public Shader UIShader;
    public StereoEyes Eyes;
    private Camera _camera;
        public RenderTexture _uiTexture;
        private Material _uiMAterial;
#if UNITY_EDITOR
        void Reset() {
        HierarchyInit();
            UIShader = Shader.Find("UI/AlphaBlendFixForRT");
    }

    void OnValidate() {
        HierarchyInit();
    }

    void HierarchyInit() {
        if (!GameObjectEx.IsPrefabInAsset(gameObject)) {
            if (!Eyes) {
                Eyes = FindObjectOfType<StereoEyes>();
            }
        }
    }
#endif

    void Start() {
        _camera = GetComponent<Camera>();
            ReplaceUIMaterials();
    }

        private void ReplaceUIMaterials() {
            if (UIShader == null) {
                return;
            }
            if (_uiMAterial == null) {
                _uiMAterial = new Material(UIShader);
            }
            List<Graphic> graphics = SceneUtils.FindObjectsOfType<Graphic>(true);
            foreach (Graphic gr in graphics) {
                if (gr.material == null || gr.material.shader.name == "UI/Default") {
                    gr.material = _uiMAterial;
                }
            }
        }

    void LateUpdate() {
        var rtSize = Eyes.GetEyeRTSize();
        if (_uiTexture == null || (_uiTexture.width != (int)rtSize.x) || (_uiTexture.height != (int)rtSize.y)) {
            _uiTexture = new RenderTexture((int)rtSize.x, (int)rtSize.y, 24);
        }
        _camera.targetTexture = _uiTexture;            
    }

    void OnEnable() {
        StartCoroutine(RenderCoroutine());
    }

    public IEnumerator RenderCoroutine() {
        while (true) {
            yield return new WaitForEndOfFrame();
            UpdateCamera();
        }
    }

    public void UpdateCamera() {
        if (_uiTexture != null && _camera != null && _camera.enabled && _camera.gameObject.activeInHierarchy) {
            var leftEyeRect = Eyes.GetCameraViewportRect(Eyes.LeftEyeActive);
            var rtSize = Eyes.GetEyeRTSize();
            var leftPixelRect = new Rect(leftEyeRect.x * rtSize.x, leftEyeRect.y * Screen.height, leftEyeRect.width * rtSize.x, rtSize.y);
            
            GL.PushMatrix();
            GL.LoadPixelMatrix(0, Screen.width, Screen.height, 0);

            Graphics.DrawTexture(leftPixelRect, _uiTexture);
            if (Eyes.RenderTwoEyesPerFrame) {
                var rightEyeRect = Eyes.GetCameraViewportRect(!Eyes.LeftEyeActive);
                var rightPixelRect = new Rect(rightEyeRect.x * rtSize.x, rightEyeRect.y * Screen.height, rightEyeRect.width * rtSize.x, rtSize.y);
                Graphics.DrawTexture(rightPixelRect, _uiTexture);
            }
            GL.PopMatrix();
        }
    }

    
}
}
