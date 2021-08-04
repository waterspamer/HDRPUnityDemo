using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace Nettle {

    public class NettleBoxStencil : MonoBehaviour {
        private const string _stencilLayerName = "ShowRoomStencil";
        [SerializeField]
        private GameObject _stencilObject;
        private Camera _camera;
        private Camera _tempCamera;
        private RenderTexture _renderTexture;

        void Start() {
            _camera = GetComponent<Camera>();
            _tempCamera = new GameObject("StencilCamera").AddComponent<Camera>();
            _tempCamera.enabled = false;
            _stencilObject.SetActive(false);
            _stencilObject.layer = LayerMask.NameToLayer(_stencilLayerName);
        }

        private void OnDestroy() {
            _renderTexture.Release();
        }

        void OnRenderImage(RenderTexture source, RenderTexture destination) {
            if (_renderTexture == null) {
                _renderTexture = new RenderTexture(source.width, source.height, 0, RenderTextureFormat.R8);
                _renderTexture.Create();
            }

            _stencilObject.SetActive(true);
            _tempCamera.CopyFrom(_camera);
            _tempCamera.clearFlags = CameraClearFlags.SolidColor;
            _tempCamera.backgroundColor = Color.black;
            _tempCamera.cullingMask = 1 << _stencilObject.layer;
            _tempCamera.targetTexture = _renderTexture;
            _tempCamera.Render();
            if (Input.GetKeyDown(KeyCode.R)) {
                Texture2D saveTex = new Texture2D(_renderTexture.width, _renderTexture.height, TextureFormat.R8, false);
                RenderTexture.active = _renderTexture;
                saveTex.ReadPixels(new Rect(0, 0, saveTex.width, saveTex.height), 0, 0);
                RenderTexture.active = null;
                File.WriteAllBytes(Application.dataPath + "\\..\\RenderTex.png", saveTex.EncodeToPNG());
            }
            Shader.SetGlobalTexture("_NettleBoxStencil", _renderTexture);
            _stencilObject.SetActive(false);
            Graphics.Blit(source, destination);
        }
    }
}