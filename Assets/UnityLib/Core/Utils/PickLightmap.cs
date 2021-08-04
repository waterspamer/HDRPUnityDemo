using UnityEngine;

namespace Nettle {

public class PickLightmap : MonoBehaviour {
    public GameObject lightmapSource;
    public Texture2D lightmap;
    public int lightmapId;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (lightmapSource != null) {
            var r = lightmapSource.GetComponent<Renderer>();
            if (r != null) {
                lightmapId = r.lightmapIndex;
                if (lightmapId == -1) {
                    lightmap = null;
                } else {
                    lightmap = LightmapSettings.lightmaps[lightmapId].lightmapColor;
                }
            }
        }
	}
}
}
