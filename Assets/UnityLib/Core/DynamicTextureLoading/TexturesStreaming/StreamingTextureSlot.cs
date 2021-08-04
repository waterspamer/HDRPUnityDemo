using System.Linq;
using UnityEngine;

namespace Nettle {

public class StreamingTextureSlot {
   // public string ParamName;
    public int MaterialId;
    private StreamingTexture2D loadingTexture;
    private StreamingTexture2D activeTexture;

    public StreamingTexture2D ActiveTexture {
        get { return activeTexture;}
    }

    public StreamingTexture2D LoadingTexture {
        get { return loadingTexture; }
    }

    public TexturesStreaming.StreamTextureType StreamDesc { get; private set; }
    public MeshRenderer Owner;

    public StreamingTextureSlot(MeshRenderer owner, TexturesStreaming.StreamTextureType streamDesc)
    {

        StreamDesc = streamDesc;
        Owner = owner;
        MaterialId = -1;
      //  ParamName = "_LightMap";
    }

    public void SetLoadingTexture(StreamingTexture2D tex) {
        SetTexture(this, ref loadingTexture, tex);
    }

    public void SetActiveTexture(StreamingTexture2D tex) {
        SetTexture(this, ref activeTexture, tex);
    }

    private static void SetTexture(StreamingTextureSlot _this, ref StreamingTexture2D oldTex,  StreamingTexture2D tex) {
        if (tex == oldTex) {
            return;
        }
        if (oldTex != null) {
            oldTex.Release();
        }
        oldTex = tex;
        if (tex != null) {
            tex.AddRef(_this);
        }
    }


    public void ApplyTexture(TexturesStreaming streaming, Texture2D texture) {
        if (Owner == null) {
            return;
        }

        var renderer = Owner.GetComponent<Renderer>();

        if (renderer != null && renderer.materials != null) {
            var defaultTex = StreamDesc.GetDefaultTexture(streaming.PathUtils == null ? 0 :streaming.PathUtils.GetDefaultTextureId());


            if (StreamDesc.IsUnityLightmap) {
                
                if (texture == null) {
                   

                    if (renderer.lightmapIndex != -1) {
                        var lm = LightmapSettings.lightmaps.ToList();
#if UNITY_2017_1_OR_NEWER
                        lm[renderer.lightmapIndex].lightmapColor = defaultTex;
#else
                        lm[renderer.lightmapIndex].lightmapLight = defaultTex;
#endif
                        LightmapSettings.lightmaps = lm.ToArray();

                       // LightmapSettings.lightmaps[renderer.lightmapIndex].lightmapFar = defaultTex;

                    } else {
                        renderer.lightmapIndex = LightMap.AddLightmap(defaultTex);
                    }

                } else {
                    //TODO: check for lightmap cache to NOT grow!

                    if (renderer.lightmapIndex != -1) {
                        var lm = LightmapSettings.lightmaps.ToList();
#if UNITY_2017_1_OR_NEWER
                        lm[renderer.lightmapIndex].lightmapColor = texture;
#else
                        lm[renderer.lightmapIndex].lightmapLight = texture;
#endif
                        LightmapSettings.lightmaps = lm.ToArray();

#if UNITY_2017_1_OR_NEWER
                        if (LightmapSettings.lightmaps[renderer.lightmapIndex].lightmapColor != texture) {
                            Debug.Log("WHAT THE FUCK???");
                        }
#else
                        if (LightmapSettings.lightmaps[renderer.lightmapIndex].lightmapLight != texture) {
                            Debug.Log("WHAT THE FUCK???");
                        }
#endif

                    } else {
                        renderer.lightmapIndex = LightMap.AddLightmap(texture);
                    }
                }
            } else {
                var paramName = StreamDesc.ShaderParamName;
               
                if (MaterialId >= 0) {
                    renderer.materials[MaterialId].SetTexture(paramName, texture == null ? defaultTex : texture);
                    renderer.materials[MaterialId].SetTextureScale(paramName, new Vector2(1, StreamDesc.FlipVertical ? -1 : 1));
                } else {
                    foreach (var t in Owner.gameObject.GetComponent<Renderer>().materials) {
                        t.SetTexture(paramName, texture == null ? defaultTex : texture);
                        t.SetTextureScale(paramName, new Vector2(1, StreamDesc.FlipVertical ? -1 : 1));
                    }
                }
            }
        }
    }

    public void ApplyTexture(TexturesStreaming streaming) {
        

        SetTexture(this, ref activeTexture, loadingTexture);
        SetTexture(this, ref loadingTexture, null);

        if (activeTexture.RefCount < 1) {
            Debug.Log("FAIL! Applying texture without refcount!");
        }

        ApplyTexture(streaming, activeTexture == null ? null : activeTexture.Texture);
    }
}
}
