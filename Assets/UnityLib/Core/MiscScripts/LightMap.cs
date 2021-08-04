using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Nettle {
[ExecuteInEditMode]
[ExecuteAfter(typeof(DefaultTime))]
public class LightMap : MonoBehaviour {
    public Texture2D Lightmap;

    private void Awake() {
        SetLightmap();
    }

    private void Reset() {
    #if UNITY_EDITOR
        if (!Lightmap) {
            var matchingGuids = AssetDatabase.FindAssets(gameObject.name + "_Lightmap");
            if (matchingGuids.Length == 1) {
                var texPath = AssetDatabase.GUIDToAssetPath(matchingGuids[0]);
                Lightmap = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
            }
        }
    #endif
    }

    private void Start() {
        /*foreach (var m in renderer.materials){
			if (Lightmap){
				m.SetTexture("_LightMap",Lightmap);
				m.SetTextureScale("_LightMap", new Vector2(1, -1));
			}
		}*/
        SetLightmap();
    }

    public static bool IsValidIndex(int lmIndex) {
        return (lmIndex >= 0) && (lmIndex < LightmapSettings.lightmaps.Length);
    }

    public static bool SetUnityLightmap(int index, Texture2D value) {
       // Debug.Log("Set lightmap: " + (lm == null ? "null" : lm.name) + " at object: " + name);
        var lm = LightmapSettings.lightmaps.ToArray();
        if (!IsValidIndex(index)) {
            return false;
        }
        lm[index].lightmapColor = value;
        LightmapSettings.lightmaps = lm;

        return true;
    }

    public int GetLightMapIndex() {
        return GetComponent<Renderer>().lightmapIndex;
    }

    public void SetLightMapIndex(int id) {
        GetComponent<Renderer>().lightmapIndex = id;
    }

    public virtual Texture2D GetLightMap() {
        return Lightmap;
    }

    private int FindOrAddLightmap() {

        if (GetLightMap() == null) {
            Debug.Log("Object without lightmaps : " + gameObject.name);
            return -1;
        }

        var index = GetLightMapIndex();

        if (IsValidIndex(index)) {
            return index;
        } else {
            SetLightMapIndex(-1);
        }

        //find lightmap in global table
        index = FindLightmapIndex(Lightmap);
        if (IsValidIndex(index)) { //found => use it
            return index;
        }

        //not found => add to global table
        SetLightMapIndex(AddLightmap(Lightmap));
        return GetLightMapIndex();
    }

    public static int FindFreeLightmapSlot() {
        for (int i = 0; i < LightmapSettings.lightmaps.Length; ++i) {
            if (LightmapSettings.lightmaps[i] == null) {
                return i;
            }
        }
        return -1;
    }

    public static int AddLightmap(Texture2D texture) {
        // int freeSlot = FindFreeLightmapSlot();
        LightmapData data = new LightmapData();
        ;
        /*if (freeSlot != -1) {
            data.lightmapNear = null;
            data.lightmapFar = texture;
            LightmapSettings.lightmaps[freeSlot] = data;
            Debug.Log("Found free slot!");
        } else {*/
        int freeSlot = LightmapSettings.lightmaps.Length;
        var tmp = new LightmapData[freeSlot + 1];
        LightmapSettings.lightmaps.CopyTo(tmp, 0);
        LightmapSettings.lightmapsMode = LightmapsMode.NonDirectional;

        data.lightmapDir = null;
        data.lightmapColor = texture;
        tmp[freeSlot] = data;
        LightmapSettings.lightmaps = tmp;
        //}
        return freeSlot;
    }

    public static int FindLightmapIndex(Texture2D texture) {
        for (int i = 0; i < LightmapSettings.lightmaps.Length; i++) {
            if (LightmapSettings.lightmaps[i].lightmapColor == texture) {
                return i;
            }
        }
        return -1;
    }


    public void SetLightmap() {
        if (enabled) {
          //  if (EditMode || !Application.isEditor ) {
                SetLightMapIndex(FindOrAddLightmap());
                if (IsValidIndex(GetLightMapIndex())) {
                    var lm = GetLightMap();
                    SetUnityLightmap(GetLightMapIndex(), GetLightMap());
                }
          /*  } else {
                GetComponent<Renderer>().lightmapIndex = -1;
            }*/
        }
    }

    private void OnValidate() {
        SetLightmap();
    }

    private void Update() {
     
        //LightMapIndex = -1;

    }
}
}
