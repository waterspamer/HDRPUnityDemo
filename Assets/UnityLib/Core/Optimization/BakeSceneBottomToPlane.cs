using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Nettle {
    public class BakeSceneBottomToPlane : MonoBehaviour {

        private const string _visibilityFactorName = "SceneBottomCulling";
        [Tooltip("Baked texure will be shown whenever display scale is above this value")]
        [SerializeField]
        private float _cullingDisplayScale = 1000;
        [SerializeField]
        [Tooltip("All objects below this level in world space will be baked into textures and culled at large distance")]
        private float _bakedObjectsUpperLimit = 10;
        [SerializeField]
        private float _bakedPlaneY = 0;
        [SerializeField]
        private float _bakingCameraFarClipPlane = 50;
        [SerializeField]
        [Tooltip("Specify root objects, under which the script will search for renderers to be culled")]
        private GameObject[] _culledObjectsRoots;
        [SerializeField]
        /*
        [Tooltip("All culled objects will be placed to a separate level to render them and only them by an orthographic camera. After baking their old layers will be restored")]
        private int _layerForBaking = 31;*/
        private LayerMask _bakingCameraMask;
        [SerializeField]
        private int _bakedTextureSize = 2048;
        [SerializeField]
        private float _tileSize = 500;
        [SerializeField]
        private Material _tileMaterial;
        [SerializeField]
        [ReadOnly()]
        [Tooltip("Objects that will actually be affected by culling. List is filled automatically by pressing the button above")]
        private List<VisibilityControl> _objectsToCull = null;
        [SerializeField]
        [ReadOnly()]
        private Bounds _combinedBounds = new Bounds();
        private List<RenderTexture> renderTextures = new List<RenderTexture>();


        private Transform _bakedPlanesRoot = null;
        private bool _showingFullScene = false;
        private MotionParallaxDisplay _display;
        private bool _initComplete = false;

#if UNITY_EDITOR
        [EasyButtons.Button]
        public void FindObjectsToCull() {
            _objectsToCull = new List<VisibilityControl>();
            bool _startedCombiningBounds = false;
            //List<MeshRenderer> _renderers = new List<MeshRenderer>();
            foreach (GameObject root in _culledObjectsRoots) {
                Renderer[] renderers = root.GetComponentsInChildren<Renderer>();
                foreach (MeshRenderer r in renderers) {
                    if (r.bounds.center.y + r.bounds.extents.y > _bakedObjectsUpperLimit || r.tag == "IgnoreSceneBottomCulling") {
                        continue;
                    }
                    if (_startedCombiningBounds) {
                        _combinedBounds.Encapsulate(r.bounds);
                    }
                    else {
                        _combinedBounds = new Bounds(r.bounds.center, r.bounds.size);
                        _startedCombiningBounds = true;
                    }
                    VisibilityControl v = r.GetComponent<VisibilityControl>();
                    if (v == null) {
                        v = Undo.AddComponent<VisibilityControl>(r.gameObject);
                    }
                    _objectsToCull.Add(v);
                }
            }
        }
#endif

        private void Start() {
            StartCoroutine(DelayedBake());
        }


        private IEnumerator DelayedBake() {
            //Wait one frame, so later when the culled objects are enabled there won't be a huge lag
            //Also helps ensure that all initialization scripts in scene have been executed by this point
            yield return null;
            yield return null;
            Bake();
            _display = FindObjectOfType<MotionParallaxDisplay>();
            UpdateCullingState();
            _initComplete  = true;
        }


        private void Update() {
            UpdateCullingState();
        }

        private void UpdateCullingState(){
        
            if (_display == null) {
                return;
            }
            SetCullingState(_display.transform.localScale.x <= _cullingDisplayScale);        
        }

        private void Bake() {
            if (_objectsToCull == null || _objectsToCull.Count == 0) {
                Debug.LogWarning("Scene bake skipped: nothing to bake");
                return;
            }
            Debug.Log("Started baking");
            //Determine the required number of tiles
            int tileCountX = (int)Mathf.Ceil(_combinedBounds.size.x / _tileSize);
            int tileCountZ = (int)Mathf.Ceil(_combinedBounds.size.z / _tileSize);
            //Create and init baking camera
            GameObject bakingCameraObject = new GameObject();
            Camera bakingCamera = bakingCameraObject.AddComponent<Camera>();
            bakingCamera.orthographic = true;
            bakingCamera.orthographicSize = _tileSize/2;
            bakingCamera.farClipPlane = _bakingCameraFarClipPlane;
            bakingCamera.nearClipPlane = 1;
            bakingCamera.transform.rotation = Quaternion.LookRotation(Vector3.down, Vector3.forward);
            bakingCamera.enabled = false;
            //bakingCamera.cullingMask = 1 <<_layerForBaking;
            bakingCamera.cullingMask = _bakingCameraMask;
            bakingCamera.clearFlags = CameraClearFlags.SolidColor;
            bakingCamera.backgroundColor = Color.clear;


            //Make all lights in the scene affect target layer
            Light[] lights = FindObjectsOfType<Light>();
            Dictionary<Light, LayerMask> lightsOldMasks = new Dictionary<Light, LayerMask>();
            foreach (Light l in lights) {
                lightsOldMasks.Add(l, l.cullingMask);
                // l.cullingMask = 1 << _layerForBaking;
            }

            //Create root object for tiles
            _bakedPlanesRoot = new GameObject("BakedSceneBottom").transform;
            //Create the quad mesh that will be reused for all tiles
            Mesh tileMesh = new Mesh();
            List<Vector3> vertices = new List<Vector3>();
            vertices.Add(new Vector3(-1, 0, 1)* _tileSize / 2);
            vertices.Add(new Vector3(1, 0, 1) * _tileSize / 2);
            vertices.Add(new Vector3(-1, 0, -1) * _tileSize / 2);
            vertices.Add(new Vector3(1, 0, -1) * _tileSize / 2);
            List<Vector2> uvs = new List<Vector2>();
            uvs.Add(new Vector2(0, 1));
            uvs.Add(new Vector2(1, 1));
            uvs.Add(new Vector2(0, 0));
            uvs.Add(new Vector2(1, 0));
            List<Vector3> normals = new List<Vector3>();
            for (int i = 0; i < 4; i++) {
                normals.Add(Vector3.up);
            }
            tileMesh.SetVertices(vertices);
            tileMesh.SetTriangles(new int[] { 1, 2, 0, 3, 2, 1 }, 0);
            tileMesh.SetNormals(normals);
            tileMesh.SetUVs(0, uvs);        
            //Render all tiles
            int mainTexId = Shader.PropertyToID("_MainTex");
            MaterialPropertyBlock materialProperties = new MaterialPropertyBlock();
            for (int i = 0; i < tileCountX; i++) {
                for (int j = 0; j < tileCountZ; j++) {
                    Vector3 tileCenter = new Vector3(_combinedBounds.center.x - _combinedBounds.extents.x + (i + 0.5f) * _tileSize, _bakedPlaneY, _combinedBounds.center.z - _combinedBounds.extents.z + (j + 0.5f) * _tileSize);
                    bakingCamera.transform.position = tileCenter + Vector3.up * (_bakedObjectsUpperLimit+1);
                    RenderTexture tileTexture = new RenderTexture(_bakedTextureSize, _bakedTextureSize, 32);
                    bakingCamera.targetTexture = tileTexture;
                    bakingCamera.Render();
                    GameObject tileObject = new GameObject("Tile" + i + "_" + j);
                    tileObject.transform.SetParent(_bakedPlanesRoot);
                    MeshFilter tileFilter = tileObject.AddComponent<MeshFilter>();
                    tileFilter.sharedMesh = tileMesh;
                    MeshRenderer tileRenderer = tileObject.AddComponent<MeshRenderer>();
                    tileRenderer.receiveShadows = true;
                    tileRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    tileRenderer.sharedMaterial = _tileMaterial;
                    tileRenderer.GetPropertyBlock(materialProperties);
                    materialProperties.SetTexture(mainTexId, tileTexture);
                    tileRenderer.SetPropertyBlock(materialProperties);
                    tileObject.transform.position = new Vector3(tileCenter.x, _bakedPlaneY, tileCenter.z);
                    renderTextures.Add(tileTexture);
                }
            }
            Destroy(bakingCameraObject);
            Debug.Log("Finished baking");
        }

        public void SetCullingState(bool showFull) {
            if (showFull == _showingFullScene && _initComplete) {
                return;
            }
            foreach (VisibilityControl vc in _objectsToCull) {
                if (vc == null)
                {
                    continue;
                }
                vc.SetVisibilityFactor("SceneBottomCulling", showFull);
            }
            _bakedPlanesRoot.gameObject.SetActive(!showFull);
            _showingFullScene = showFull;
        }
    }
}