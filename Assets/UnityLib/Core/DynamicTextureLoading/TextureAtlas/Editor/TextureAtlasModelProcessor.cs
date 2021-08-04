using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Nettle {

public class TextureAtlasModelProcessor : AssetPostprocessor {
    private void OnPostprocessModel(GameObject modelRoot) {
        string[] assets = AssetDatabase.FindAssets("t:TextureAtlasInfo");
        TextureAtlasInfo atlasInfo = null;
        foreach (string guid in assets) {
            TextureAtlasInfo ai = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(guid)) as TextureAtlasInfo;
            if (ai != null && ai.ModelFileName == modelRoot.name) {
                atlasInfo = ai;
                break;
            }
        }
        if (atlasInfo == null) {
            return;
        }
        Debug.Log("Importing atlased mesh " + modelRoot.name);
        MeshFilter[] filters = modelRoot.GetComponentsInChildren<MeshFilter>();
        foreach (MeshFilter f in filters) {
            TextureAtlasInfo.AtlasedObject obj = null;
            foreach (TextureAtlasInfo.Atlas atlas in atlasInfo.Atlases) {
                foreach (TextureAtlasInfo.AtlasedObject ao in atlas.Objects) {
                    if (ao.ObjectName == f.gameObject.name) {
                        obj = ao;
                        break;
                    }
                }
                if (obj != null) {
                    break;
                }

            }
            if (obj == null) {
                continue;
            }
            ApplyAtlasDataToMesh(f.sharedMesh, obj);
        }
    }

    public void ApplyAtlasDataToMesh(Mesh mesh, TextureAtlasInfo.AtlasedObject atlasedObject) {
        Vector2[] uvs = mesh.uv2;
        for (int i = 0; i < uvs.Length; i++) {
            uvs[i] = new Vector2(uvs[i].x * atlasedObject.UVScale.x, uvs[i].y * atlasedObject.UVScale.y);
            uvs[i] += atlasedObject.UVOffset;
        }
        mesh.uv2 = uvs;        
    }
}
}
