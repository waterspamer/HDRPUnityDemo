using UnityEngine;
using System;
using Object = UnityEngine.Object;

namespace Nettle {

public class StreamingTexture2D : IDisposable {
    public Texture2D Texture;
    public IntPtr StreamingContext;
    public NTextureInfo SrcFormat;
    public NTextureInfo DstFormat;

    //private object _user; 

    public string SourcePath {
        get { return _sourcePath; }
        set {
            if (_sourcePath != value && RefCount != 0) {
#if UNITY_EDITOR
                //Debug.Log("EPIC FAIL! Changing path while refcounted; New path: " + _sourcePath);
#endif
            }
            _sourcePath = value;
        }
    }

    private int refCount = 0;
    private string _sourcePath;

    public Action<StreamingTexture2D> StreamingComplete; 

    public StreamingTexture2D(Texture2D unityTex) {
        Texture = unityTex;
        if (Texture == null) {
            Debug.Log("Fail!!! NULL Texture");
        } else {
            StreamingContext = TextureLoaderDll.CreateStreamingContext(Texture.GetNativeTexturePtr());
        }
    }

    public void OnStreamingComplete(bool clearHandlers) {
        if (StreamingComplete != null) {
            StreamingComplete(this);
            if (clearHandlers) {
                StreamingComplete = null;
            }
        }
    }

    public bool IsLoading() {
        return !TextureLoaderDll.IsStreamingFinished(StreamingContext);
    }

    public bool Streaming{
        get{return !TextureLoaderDll.IsStreamingFinished(StreamingContext);}
    }

    public void CancelStreaming() {
        StreamingComplete = null;
        //Debug.Log("Cancelled texture " + SourcePath);
        SourcePath = "";
        Texture.name = "Cancelled";
        TextureLoaderDll.CancelTextureStreaming(StreamingContext);
    }
    public void BeginStreaming() {
        Texture.name = "Stream: " + System.IO.Path.GetFileName(SourcePath);
        TextureLoader.GetTextureInfo(SourcePath, out SrcFormat);
        TextureLoaderDll.BeginTextureStreaming(StreamingContext, SourcePath, ref SrcFormat, ref DstFormat);
    }
    public int RefCount {
        get { return refCount; }
    }
    public void AddRef(object user)
    {
        //_user = user;
        refCount++;
    }
    public void Release() {
        refCount--;

        if (refCount < 0) {
            Debug.LogError("StreamingTexture2D: RefCount error! RefCount = " + refCount);
        }

        if (refCount == 0 ) {
            CancelStreaming();
            //_user = null;
        }
    }

    public void Dispose() {
        OnDestroy();
    }

    public void OnDestroy(bool destroyTexture = true) {
        if (destroyTexture) {
            Object.Destroy(Texture);
        }
        if (StreamingContext != IntPtr.Zero) {
            TextureLoaderDll.ReleaseStreamingContext(StreamingContext);
            StreamingContext = IntPtr.Zero;
        }
    }
}
}
