using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;

namespace Nettle {

[ExecuteBefore(typeof(VisibilityManagerStreaming))]
public class TexturesStreaming : MonoBehaviour {
    public StreamingTexturePathUtils PathUtils;

    private List<VisibilityControl> visibilityControls = new List<VisibilityControl>();
    private VisibilityManagerStreaming _visibilityManagerStreaming;

    [Serializable]
    public class StreamTextureType {
        [HideInInspector]
        public string JustForInspector = "StreamTextureType";
        public string Folder;
        public string FilenameFormat = "" + StreamingTexturePathUtils.ObjectNamePlaceholder + "_[" + StreamingTexturePathUtils.VisibilityTagPlaceholder + "]_LightMap" + "_" + StreamingTexturePathUtils.DayNightTagPlaceholder + ".dds";
        public string ShaderParamName;

        public bool FlipVertical = false;
        public bool IsUnityLightmap = true;

        public Texture2D[] defaultTextures;

        public Texture2D GetDefaultTexture(int id) {
            if ((defaultTextures == null) || (defaultTextures.Length <= id)) {
                return null;
            }
            return defaultTextures[id];
        }
    }

    public class StreamingContext {
        public MeshRenderer Renderer;
        public List<StreamingTextureSlot> StreamSlots = new List<StreamingTextureSlot>();
    }

    public enum StreamStage {
        Idle,
        LoResStreaming,
        HiResStreaming
    }

    public StreamTextureType[] StreamTextures = {
        new StreamTextureType{ShaderParamName = "_LightMap", Folder = @"Lightmaps/", FlipVertical = false},
    };

    private readonly Dictionary<MeshRenderer, StreamingContext> _streamContexts
        = new Dictionary<MeshRenderer, StreamingContext>();

    private readonly StreamingTexturesCache _texturesCache = new StreamingTexturesCache();
    private readonly List<StreamingTexture2D> _requests = new List<StreamingTexture2D>();

    public StreamingTexturesCache TexturesCache {
        get { return _texturesCache; }
    }

    private StreamStage _stage;
    public event Action<TexturesStreaming> StageChanged;
    private List<MeshRenderer> _hiresList;

    public bool LoadLoRes = true;
    public bool LoadHiRes = true;
    public bool LoadAtStart = true;

    [SerializeField]
    private bool _createCacheOnInit = true;

    void Start() {
        if (LoadAtStart) {
            Initialize();
        }
    }

    public StreamStage Stage {
        get { return _stage; }
        private set {
            if (_stage != value) {
                _stage = value;
                if (StageChanged != null)
                    StageChanged.Invoke(this);
            }
        }
    }

    void OnLevelWasLoaded(int level) {
        //if (!LoadAtStart)
        //    Initialize();
    }

    void Awake() {
        _visibilityManagerStreaming = FindObjectOfType<VisibilityManagerStreaming>();
        if (!LoadAtStart) {
            Initialize();
        }
    }

    protected void Initialize() {
        //print("Initialization!");

        TextureLoaderDll.OnStart();

        if (StreamTextures != null) {
            foreach (var folder in StreamTextures.Select(v => v.Folder)) {
                TextureLoader.PrecacheFormats(PathUtils.GetAbsoluteFolderPath(folder, false), true);
            }

            TexturesCache.Clean();
            if (_createCacheOnInit) {
                VisibilityStreamingUtils.BuildTexturesCache(StreamTextures, this, PathUtils);
            }
            Debug.Log("Textures in cache: " + TexturesCache.Textures.Count);
        }

        Stage = StreamStage.Idle;

        _streamContexts.Clear();
        visibilityControls.Clear();

        visibilityControls = FindObjectsOfType<VisibilityControl>().Where(v => v.GetComponent<MeshRenderer>() != null).ToList();

        foreach (var visibilityControl in visibilityControls) {
                if (!(visibilityControl.Renderer is MeshRenderer)) {
                    continue;
                }
            var ctx = new StreamingContext {Renderer = visibilityControl.Renderer as MeshRenderer};
            foreach (var t in StreamTextures) {
                ctx.StreamSlots.Add(new StreamingTextureSlot(visibilityControl.Renderer as MeshRenderer, t));
            }

            if (!_streamContexts.ContainsKey(visibilityControl.Renderer as MeshRenderer)) _streamContexts.Add(visibilityControl.Renderer as MeshRenderer, ctx);
        }

        foreach (var ctx in _streamContexts) {
            foreach (var slot in ctx.Value.StreamSlots) {
                slot.SetLoadingTexture(null);
                slot.ApplyTexture(this, null); //force apply default texture
            }
        }
    }

    private bool IsStreamingSomething() {
        return _requests.Count != 0;
        /*return _streamContexts
            .SelectMany(k => k.Value.StreamSlots)
            .Any(v => v.texture != null && v.texture.IsLoading());*/
    }

    public void ReloadTextures(bool preview) {
        Stage = preview ? StreamStage.LoResStreaming : StreamStage.HiResStreaming;
        CancelAllStreaming();
#if UNITY_EDITOR
        if (!UnityEditor.EditorApplication.isPlaying) {
            Stage = StreamStage.Idle;
            return;
        }
#endif
        if (!LoadLoRes && Stage == StreamStage.LoResStreaming) {
            return;
        }

        if (!LoadHiRes && Stage == StreamStage.HiResStreaming) {
            return;
        }

        //Release(objectsList);

        foreach (var control in _visibilityManagerStreaming.ShowList) {            
                if (!(control.Renderer is MeshRenderer)) {
                    continue;
                }
            if (control.Renderer==null || !_streamContexts.ContainsKey(control.Renderer as MeshRenderer)) {
                continue;
            }
            var ctx = _streamContexts[control.Renderer as MeshRenderer];
            UnityEngine.Profiling.Profiler.BeginSample("ctx.StreamSlot loop");
            foreach (var slot in ctx.StreamSlots) {
                if (preview && control.gameObject.GetComponent<VisibilityControlAlways>()) {
                    continue;
                }
                UnityEngine.Profiling.Profiler.BeginSample("ctx.StreamSlot.Part1");
                var fileName = PathUtils.GetFileName(control.gameObject, slot.StreamDesc.FilenameFormat);
                UnityEngine.Profiling.Profiler.EndSample();
                UnityEngine.Profiling.Profiler.BeginSample("ctx.StreamSlot.Part2");
                var filePath = PathUtils.GetTexturePath(control.gameObject, fileName, slot.StreamDesc, preview);
                UnityEngine.Profiling.Profiler.EndSample();

                if (filePath == "default") {
                    UnityEngine.Profiling.Profiler.BeginSample("ctx.StreamSlot.Part3");
                    slot.SetLoadingTexture(null);
                    slot.ApplyTexture(this, null); //force apply default texture
                    UnityEngine.Profiling.Profiler.EndSample();
                } else {
                    if (slot.ActiveTexture != null) {
                        if (slot.ActiveTexture.SourcePath == filePath) {
                            continue;
                        }
                    }
                    bool loaded;
                    UnityEngine.Profiling.Profiler.BeginSample("ctx.StreamSlot.Part4");
                    var newSlotTex = QueryStreamingTexture(filePath, true, out loaded);
                    UnityEngine.Profiling.Profiler.EndSample();
                    
                    slot.SetLoadingTexture(newSlotTex);

                    if (newSlotTex != null) {
                        if (loaded) {
                            ApplyStreamedTexture(slot);
                        } else {
                            var s = slot;
                            newSlotTex.StreamingComplete = null;
                            newSlotTex.StreamingComplete = (tex => {
                                ApplyStreamedTexture(s);

                            });
                            BeginTextureStreaming(newSlotTex);
                        }
                    }
                }
            }
            UnityEngine.Profiling.Profiler.EndSample();
        }
    }

    public void Release(List<MeshRenderer> items) {
        if (items != null && _streamContexts.Count > 0) {
            foreach (var meshRenderer in items) {
                if (meshRenderer != null && _streamContexts.ContainsKey(meshRenderer)) {
                    var slots = _streamContexts[meshRenderer].StreamSlots;
                    if (slots != null) {
                        foreach (var slot in slots) {
                            slot.SetLoadingTexture(null);
                            slot.SetActiveTexture(null);
                        }
                    }
                }
            }
        }
    }

    public void Update() {
        TextureLoaderDll.ProcessDeviceObjects();

        if (_stage != StreamStage.Idle) {
            if (_stage == StreamStage.LoResStreaming) {
                if (!IsStreamingSomething()) {
                    //Stage = StreamStage.HiResStreaming;
                    ReloadTextures(false);
                }
            } else if (_stage == StreamStage.HiResStreaming) {
                if (!IsStreamingSomething()) {
                    Stage = StreamStage.Idle;
                }
            }
        }

        //complete loaded textures
        var tmpRequests = _requests.ToArray();
        foreach (var r in tmpRequests) {
            if (!r.IsLoading()) {
                _requests.Remove(r);
                r.OnStreamingComplete(true);
            }
        }
    }

    private void ApplyStreamedTexture(StreamingTextureSlot slot) {
        //if (slot.ActiveTexture != null)
        //{
        //    Debug.Log("Apply streaming texture " + slot.ActiveTexture.SourcePath);
        //}
      
        slot.ApplyTexture(this);
    }

    /*   public void CancelTextureStreaming(StreamingTexture2D tex) {
   #if UNITY_EDITOR
           if (!UnityEditor.EditorApplication.isPlaying){
               return;
           }
   #endif
           if (_requests.Contains(tex)) {
               _requests.Remove(tex);
           }
           if (tex.IsLoading()) {
               tex.CancelStreaming();
           }
       }*/

    public void BeginTextureStreaming(StreamingTexture2D tex) {
#if UNITY_EDITOR
        if (!UnityEditor.EditorApplication.isPlaying) {
            return;
        }
#endif
        if (tex != null) {
            _requests.RemoveAll((a) => a == tex);
            //if (tex.Texture.name.Contains("building_a_roof")) {
            //    Debug.Log("building_a_roof ADD REQUEST " + tex.SourcePath);
            //}
            _requests.Add(tex);
            tex.BeginStreaming();
        }
    }

    private void CancelAllStreaming() {
        foreach (var r in _requests.Where(r => r.Texture != null)) {
            r.CancelStreaming();
        }
        _requests.Clear();
    }

    public StreamingTexture2D QueryStreamingTexture(string texturePath, bool mips, out bool loaded) {
        loaded = false;
#if UNITY_EDITOR
        if (!UnityEditor.EditorApplication.isPlaying) {
            return null;
        }
#endif
        
        Profiler.BeginSample("File.Exists");
        if (!System.IO.File.Exists(texturePath)) {
            Profiler.EndSample();
            return null;
        } else {
            Profiler.EndSample();
        }

        //TODO: get back that code, BUT!!! make sure that "loaded" state detection is 100% correct!!!
        /* var tex = _texturesCache.GetLoadedTexture(texturePath);
         if (tex != null) {
             Debug.Log("Found loaded texture! " + texturePath + " --- " + tex.Texture.name);
             loaded = true;
             return tex;
         }*/

        NTextureInfo fmt;
        Profiler.BeginSample("GetTextureInfo");
        TextureLoader.GetTextureInfo(texturePath, out fmt);
        Profiler.EndSample();
        if (!mips) {
            Profiler.BeginSample("fmt.MipLevels = 1");
            fmt.MipLevels = 1;
            Profiler.EndSample();
        }
        Profiler.BeginSample("CreateStreamingTexture");
        var tex = _texturesCache.GetFitTexture(fmt, true, false) ??
              _texturesCache.CreateStreamingTexture(texturePath, fmt);
        Profiler.EndSample();
        if (tex == null) {
            return null;
        }

        tex.SourcePath = texturePath;
        return tex;
    }

    private void OnDestroy() {
        CancelAllStreaming();
        TextureLoaderDll.OnDestroy();
        _texturesCache.Clean();
    }
}
}
