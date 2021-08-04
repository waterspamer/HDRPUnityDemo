using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine.SceneManagement;

namespace Nettle {
    [ExecuteAfter(typeof(DefaultTime))]
    public class FlatReflectionManager : MonoBehaviour {
        [SerializeField]
        private bool _generateMaps = true;
        private Texture2D _flatReflectionTex;
        [SerializeField]
        private Texture2D _flatReflectionBlurTex;
        private RenderTexture _flatReflectionTexRt;
        [SerializeField]
        private Texture2D _sphereReflectionTexture;
        [SerializeField]
        private bool _setSphereMap = true;
        [SerializeField]
        private int _texSize = 512;
        [SerializeField]
        private bool _forceMapGeneration = false;
        [SerializeField]
        private int _mapBlurIterations = 10;
        [SerializeField]
        private bool _createBlurryReflection = false;
        [SerializeField]
        private bool _useBackgroundColor = true;
        [SerializeField]
        [ConditionalHide("_useBackgroundColor", true)]
        private Color _backgroundColor;
        [SerializeField]
        private LayerMask _renderMask;
        [SerializeField]
        private bool _bakePartOfScene = false;
        [ConditionalHide("BakePartOfScene", true)]
        [SerializeField]
        private GameObject _rootOfIncludedGameObjects;

        private bool _sceneRender = false;
        private Camera _cam;
        private Bounds _sceneBounds;
        private Renderer[] _renderers;
        private float _areaSize = 0.0f;

        void Awake() {
            PrepareTextures();
            DrawReflection();
        }

        public void PrepareTextures() {
            if (!_generateMaps) return;
            List<Renderer> r = new List<Renderer>();
            if (_bakePartOfScene) {
                r = _rootOfIncludedGameObjects.GetComponentsInChildren<Renderer>().ToList();
            } else {
                GameObject[] obj = (GameObject[])FindObjectsOfType(typeof(GameObject));
                if (obj != null) {
                    for (int i = 0; i < obj.Length; i++) {
                        if ((_renderMask.value & (1 << obj[i].layer)) != 0 && obj[i].GetComponent<Renderer>() != null) {
                            r.Add(obj[i].GetComponent<Renderer>());
                        }
                    }
                }
            }
            if (r.Count > 0) {
                _renderers = new Renderer[r.Count];
                for (int i = 0; i < r.Count; i++) {
                    _renderers[i] = r[i];
                }
            } else {
                _renderers = null;
                return;
            }

            _sceneRender = true;
            _cam = gameObject.GetComponent<Camera>();
            if (_cam == null) {
                _cam = gameObject.AddComponent<Camera>();
                _cam.backgroundColor = new Color(0, 0, 0, 0);
            }

            _flatReflectionTexRt = new RenderTexture(_texSize, _texSize, 24, RenderTextureFormat.ARGB32);
            Texture2D texture2D = new Texture2D(_texSize, _texSize, TextureFormat.ARGB32, true) {
                filterMode = FilterMode.Trilinear
            };
            _flatReflectionTex = texture2D;

            if (_createBlurryReflection) {
                _flatReflectionBlurTex = new Texture2D(_texSize, _texSize, TextureFormat.ARGB32, true) {
                    filterMode = FilterMode.Trilinear
                };
            }

            if (_renderers.Length > 0) {
                //compute scene bounds
                _sceneBounds = _renderers[0].bounds;
                for (int i = 1; i < _renderers.Length; ++i) {
                    _sceneBounds.Encapsulate(_renderers[i].bounds);
                }
                //check if scene has really visible objects with not null size
                if (_sceneBounds.size == Vector3.zero) {
                    _sceneRender = false;
                    Destroy(_cam);
                    _flatReflectionTex = null;
                    return;
                }
            }

            _areaSize = Mathf.Max(_sceneBounds.size.x, _sceneBounds.size.z);
            Shader.SetGlobalTexture("_ReflectionFlatMap", _flatReflectionTex);
            Shader.SetGlobalTexture("_ReflectionFlatBlurryMap", _flatReflectionBlurTex);
            Shader.SetGlobalInt("mip_count", _flatReflectionTex.mipmapCount);
            Shader.SetGlobalVector("_ReflectionFlatParams", new Vector4(_sceneBounds.center.x, _sceneBounds.center.z, _sceneBounds.size.x, _sceneBounds.size.z));
            if (_useBackgroundColor) {
                Shader.EnableKeyword("FLAT_REFLECTION_BACKGROUND_COLOR");
                Shader.SetGlobalColor("_ReflectionFlatBackgroundColor", _backgroundColor);
            } else {
                Shader.DisableKeyword("FLAT_REFLECTION_BACKGROUND_COLOR");
            }
            if (_setSphereMap) {
                Shader.SetGlobalTexture("_ReflectionSphereMap", _sphereReflectionTexture);
            }
        }

        public void DrawReflection() {
            if (_sceneRender && _generateMaps) {
                SetCamera();
                bool rerender = false;
                if (_forceMapGeneration)
                    rerender = true;
                else if (!LoadReflection(_flatReflectionTex, "NotBlurryReflection.png") ||
                    (_createBlurryReflection && !LoadReflection(_flatReflectionBlurTex, "BlurryReflection.png"))) {
                    rerender = true;
                }
                Rerender(rerender);
                if (_cam != null) {
                    _cam.transform.Rotate(-90, 0, 0);
                    Destroy(_cam);
                }
                _renderers = null;
                _sceneRender = false;
            }
        }

        private void SetCamera() {
            _cam.orthographic = true;
            _cam.orthographicSize = _areaSize / 2;
            _cam.enabled = false;
            _cam.aspect = 1.0f;
            _cam.name = "FlatReflectionCamera";
            _cam.transform.parent = null;
            _cam.transform.Rotate(90, 0, 0);
            _cam.transform.localPosition = new Vector3(_sceneBounds.center.x, _sceneBounds.max.y, _sceneBounds.center.z);
            _cam.farClipPlane = _sceneBounds.size.y * 2.0f;

            _cam.targetTexture = _flatReflectionTexRt;
            _cam.cullingMask = _renderMask;
            _cam.backgroundColor = _backgroundColor;
            _cam.clearFlags = CameraClearFlags.SolidColor;
        }

        private void ApplyBloor() {
            if (_createBlurryReflection) {
                Vector2[] blurKernel = new Vector2[]{
                        new Vector2(-1,-1),
                        new Vector2( 0,-1),
                        new Vector2( 1,-1),
                        new Vector2(-1, 0),
                        new Vector2( 0, 0),
                        new Vector2( 1, 0),
                        new Vector2(-1, 1),
                        new Vector2( 0, 1),
                        new Vector2( 1, 1),
                    };

                for (int i = 0; i < _flatReflectionBlurTex.mipmapCount; i++) {
                    Color[] pixels = _flatReflectionBlurTex.GetPixels(i);
                    int width = (int)Mathf.Sqrt(pixels.Length);
                    int blurIterCount = _mapBlurIterations;
                    //blur iterations
                    for (int iter = 0; iter < blurIterCount; iter++) {
                        //blur each pixel
                        for (int y = 0; y < width; y++) {
                            for (int x = 0; x < width; x++) {
                                Color sumColor = new Color(0, 0, 0, 0);
                                //sample 9 pixels
                                foreach (var dir in blurKernel) {
                                    int sx = x + (int)dir.x;
                                    int sy = y + (int)dir.y;
                                    sx = sx >= 0 ? sx : 0;
                                    sx = sx < width ? sx : (width - 1);
                                    sy = sy >= 0 ? sy : 0;
                                    sy = sy < width ? sy : (width - 1);
                                    sumColor += pixels[sy * width + sx];
                                }
                                sumColor /= blurKernel.Length;
                                pixels[y * width + x] = sumColor;
                            }
                        }
                    }
                    _flatReflectionBlurTex.SetPixels(pixels, i);
                    pixels = null;
                }
                _flatReflectionBlurTex.Apply();
                SaveReflection(_flatReflectionBlurTex, "BlurryReflection.png");
            }
        }

        private void Rerender(bool needRerend) {
            if (needRerend) {
                _cam.Render();
                RenderTexture prev = RenderTexture.active;
                RenderTexture.active = _flatReflectionTexRt;
                _flatReflectionTex.ReadPixels(new Rect(0, 0, _texSize, _texSize), 0, 0, true);
                _flatReflectionTex.Apply();
                if (_createBlurryReflection) {
                    _flatReflectionBlurTex.ReadPixels(new Rect(0, 0, _texSize, _texSize), 0, 0, true);
                    _flatReflectionBlurTex.Apply();
                }
                RenderTexture.active = prev;
                ApplyBloor();
                SaveReflection(_flatReflectionTex, "NotBlurryReflection.png");

            }
        }

        private bool LoadReflection(Texture2D tex, string filename) {
            string load_dir = GetMapsDir();
            string info_dir = load_dir + "info_" + filename + ".bin";

            if (!File.Exists(info_dir)) {
                load_dir = Application.dataPath + "/" + SceneManager.GetActiveScene().name + "/Maps/GeneratedReflection/";  //Deprecated path
                info_dir = load_dir + "info_" + filename + ".bin";
            }

            if (File.Exists(info_dir)) {
                byte[] info = File.ReadAllBytes(info_dir);
                if (info == null || info.Length < 1)
                    return false;
                int mip_count = info[0];
                if (mip_count != tex.mipmapCount) {
                    return false;
                }

                for (int i = 0; i < mip_count; i++) {
                    string mip_name = "mip_" + i.ToString() + filename;
                    byte[] mip_data = File.ReadAllBytes(load_dir + mip_name);
                    if (mip_data == null || mip_data.Length < 1) {
                        return false;
                    }
                    Texture2D mip = new Texture2D(1, 1, TextureFormat.ARGB32, false);
                    mip.LoadImage(mip_data);
                    if (mip == null) {
                        return false;
                    }
                    Color[] mip_pixels = mip.GetPixels(0);
                    tex.SetPixels(mip_pixels, i);
                }
                tex.Apply();
                return true;
            }
            return false;
        }

        private void SaveReflection(Texture2D tex, string filename) {
            if (tex != null && tex.mipmapCount > 0) {
                string save_dir = GetMapsDir();
                Directory.CreateDirectory(save_dir);
                for (int i = 0; i < tex.mipmapCount; i++) {
                    Color[] mipData = tex.GetPixels(i);
                    int width = (int)Mathf.Sqrt(mipData.Length);
                    Texture2D mip = new Texture2D(width, width, TextureFormat.ARGB32, false);
                    mip.SetPixels(mipData);
                    mip.Apply();
                    byte[] png_data = mip.EncodeToPNG();
                    File.WriteAllBytes(save_dir + "mip_" + i.ToString() + filename, png_data);
                }
                byte[] texInfo = new byte[1];
                texInfo[0] = (byte)tex.mipmapCount;
                File.WriteAllBytes(save_dir + "info_" + filename + ".bin", texInfo);
            }
        }

        public void RenderFlatReflectionMap() {
            _sceneRender = true;
            _cam = gameObject.GetComponent<Camera>();
            if (_cam == null) {
                _cam = gameObject.AddComponent<Camera>();
                _cam.backgroundColor = new Color(0, 0, 0, 0);
            }
        }

        private string GetMapsDir() {
            return Application.dataPath + "/FlatReflectionMaps/" + SceneManager.GetActiveScene().name + "/";
        }

    }
}
