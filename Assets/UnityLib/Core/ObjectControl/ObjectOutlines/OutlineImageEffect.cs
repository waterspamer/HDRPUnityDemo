using UnityEngine;
using System.Collections;
using System.IO;

namespace Nettle {

    public class OutlineImageEffect : MonoBehaviour {
        public const string OutlineLayerName = "Outline";
        [SerializeField]
        private Shader _outlineShader;
        public Shader _outlinedObjectShader;
        [Range(0, 5)]
        public float _outlineSize = 1;
        [Range(0, 20)]
        public float _outlineStrength = 2;
        public Color _outlineColor = Color.white;
        [SerializeField]
        private bool _flipUVs = false;
        [SerializeField]
        private bool _separateLayerForOutlinedObjects = true;
        private Camera _camera;
        private Camera _tempCamera;
        private Material _outlineMaterial;
        private RenderTexture _tempRT;
        public static bool SeparateLayerForOutlinedObjects{ get; private set; }

        private void Reset() {
            _outlinedObjectShader = Shader.Find("Hidden/OutlinedObject");
            _outlineShader = Shader.Find("Hidden/Outline");
        }

        private void Awake()
        {
            SeparateLayerForOutlinedObjects = _separateLayerForOutlinedObjects;
        }

        void Start() {
#if UNITY_WEBGL
            enabled = false;
            return;
#endif
            _camera = GetComponent<Camera>();
            _tempCamera = new GameObject("OutlineCamera").AddComponent<Camera>();
            _tempCamera.transform.parent = transform;
            _tempCamera.enabled = false;
            _outlineMaterial = new Material(_outlineShader);
        }
        private void OnDestroy() {
            _tempRT.Release();
        }

        void OnRenderImage(RenderTexture source, RenderTexture destination) {
            if (_tempRT == null) {
                _tempRT = new RenderTexture(source.width, source.height, 24,RenderTextureFormat.Default);
                _tempRT.filterMode = FilterMode.Trilinear;
                _tempRT.Create();
            }
            _tempCamera.CopyFrom(_camera);
            _tempCamera.renderingPath = RenderingPath.Forward;
            _tempCamera.clearFlags = CameraClearFlags.Color;
            _tempCamera.backgroundColor = Color.black;
            _tempCamera.depthTextureMode = DepthTextureMode.None;
            if(_separateLayerForOutlinedObjects){
                _tempCamera.cullingMask = 1 << LayerMask.NameToLayer(OutlineLayerName);
            }
            _tempCamera.rect = new Rect(0, 0, 1, 1);
            _tempCamera.targetTexture = _tempRT;
            _tempCamera.RenderWithShader(_outlinedObjectShader, "");
            
            _outlineMaterial.SetTexture("_SceneTex", source);
            _outlineMaterial.SetFloat("OutlineSize", _outlineSize);
            _outlineMaterial.SetFloat("OutlineStrength", _outlineStrength);
            _outlineMaterial.SetColor("OutlineColor", _outlineColor);
            if (_flipUVs) {
                _outlineMaterial.EnableKeyword("FLIP_UVS");
            }
            else {
                _outlineMaterial.DisableKeyword("FLIP_UVS");
            }
            Graphics.Blit(_tempRT, destination, _outlineMaterial);
        }
    }
}
