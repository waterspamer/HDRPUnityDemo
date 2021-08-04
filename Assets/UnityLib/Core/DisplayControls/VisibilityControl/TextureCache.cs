using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine.Profiling;

namespace Nettle {

public class StreamingTexturesCache {
    public event Action<StreamingTexturesCache, StreamingTexture2D, bool> TextureCreated; 
    private readonly List<StreamingTexture2D> _textures = new List<StreamingTexture2D>();

    public List<StreamingTexture2D> Textures {
        get { return _textures; }
    }

    public StreamingTexture2D GetLoadedTexture(string path) {
        return _textures.FirstOrDefault(tex => tex.SourcePath == path && !tex.Streaming);
    }

    public StreamingTexture2D GetFitTexture(NTextureInfo dstFormat, bool addNotLoadingTex, bool addLoadingTex) {
        foreach (var tex in _textures) {
            if (tex.RefCount < 1 && 
                tex.Texture.width == dstFormat.Width && 
                tex.Texture.height == dstFormat.Height &&
                tex.Texture.format == TextureLoader.DxgiToUnityFormat(dstFormat.Format) &&
                tex.Texture.mipmapCount <= dstFormat.MipLevels) {
				
				if(tex.IsLoading()){
					if (addLoadingTex){
                		return tex;
					}
				}else{
					if (addNotLoadingTex){
                		return tex;
					}
				}
            }
        }
        return null;
    }

    public List<StreamingTexture2D> QueryFitTextures(NTextureInfo dstFormat,  bool addNotLoadingTex, bool addLoadingTex, int maxCount) {
        var result = new List<StreamingTexture2D>();
        foreach (var tex in _textures) {
            if (maxCount != -1 && result.Count >= maxCount)
                return result;
            if (tex.RefCount < 1 && tex.Texture.width == dstFormat.Width && tex.Texture.height == dstFormat.Height &&
                tex.Texture.format == TextureLoader.DxgiToUnityFormat(dstFormat.Format)) 
            {
                if (!addLoadingTex && tex.IsLoading())
                    continue;
                if (!addNotLoadingTex && !tex.IsLoading())
                    continue;
				
				if(tex.IsLoading()){
					if (addLoadingTex){
               			result.Add(tex);
					}
				}else{
					if (addNotLoadingTex){
                		result.Add(tex);
					}
				}
            }
        }
        return result; 
    }

    public StreamingTexture2D CreateStreamingTexture(string path, NTextureInfo srcFormat, bool addToCache = true) {
        Profiler.BeginSample("unityTex");
        var unityTex = new Texture2D((int) srcFormat.Width, (int) srcFormat.Height,
            TextureLoader.DxgiToUnityFormat(srcFormat.Format), srcFormat.MipLevels > 1, false) {
                filterMode = FilterMode.Trilinear
            };

        Profiler.EndSample();

        Profiler.BeginSample("new StreamingTexture2D");
        var result = new StreamingTexture2D(unityTex);
        Profiler.EndSample();

        Profiler.BeginSample("Part2");
        TextureLoader.GetTextureInfo(path, out result.SrcFormat);
        result.DstFormat = srcFormat;
        result.DstFormat.MipLevels = (uint)unityTex.mipmapCount;
        result.SourcePath = path;
        Profiler.EndSample();
        if (addToCache) {
            Profiler.BeginSample("_textures.Add");
            _textures.Add(result);
            Profiler.EndSample();
        }
        Profiler.BeginSample("OnTextureCreated");
        OnTextureCreated(result, addToCache);
        Profiler.EndSample();
        return result;
    }

    public void Clean() {
        foreach (var t in _textures) {
            t.Dispose();
        }
		_textures.Clear();
    }

    protected virtual void OnTextureCreated(StreamingTexture2D texture, bool addInCache) {
        var handler = TextureCreated;
        if (handler != null) {
            handler(this, texture, addInCache);
        }
    }
}
}
