using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Nettle {

public class VisibilityStreamingUtils {

    public static VisibilityZone[] FindVisibilityZones(){
        return Object.FindObjectsOfType<VisibilityZone>().ToArray();
    }

    public static void BuildTexturesCache(TexturesStreaming.StreamTextureType[] streamTypes, TexturesStreaming streamer, StreamingTexturePathUtils pathUtils)
    {
        var zones = FindVisibilityZones();
        var controls = VisibilityManager.FindTargets();

        foreach (var zone in zones){
            if (zone != null){
                var visibilityTag = zone.VisibilityTag;
                List<VisibilityControl> hideList = new List<VisibilityControl>();
                List<VisibilityControl> showList = new List<VisibilityControl>();
                VisibilityManager.QueryObjects(visibilityTag, controls, ref showList, ref hideList);

                var zoneTextures = new List<NTextureInfo>();
                var textures = new List<StreamingTexture2D>();

                //find all textures for zone
                foreach (var control in showList) {
                    foreach (var streamType in streamTypes) {
                        var fileName = pathUtils.GetFileName(control.gameObject, streamType.FilenameFormat);
                        var filePath = pathUtils.GetTexturePath(control.gameObject, fileName, streamType, false);

                        NTextureInfo info;
                        if (TextureLoader.GetTextureInfo(filePath, out info)) {
                            zoneTextures.Add(info);
                        }

                        bool loaded;
                        
                        var tex = streamer.QueryStreamingTexture(filePath, true, out loaded);
                        if (tex != null)
                        {
                            tex.AddRef(null);
                            textures.Add(tex);
                        }
                    }
                }
               
                //add textures to cache
                foreach (var texture in textures) {
                    texture.Release();
                    texture.SourcePath = "";
                }
            }
        }
    }
}
}
