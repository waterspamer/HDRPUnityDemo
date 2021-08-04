using UnityEngine;

namespace Nettle {

public class TexturesStreamingProfiler : MonoBehaviour {
    public TexturesStreaming streamer;
    public int totalBytesAllocated;

    void Reset(){
        SceneUtils.FindObjectIfSingle(ref streamer);
    }

    // Use this for initialization
    void Start () {
	    if (streamer != null) {
            streamer.TexturesCache.TextureCreated += TexturesCache_TextureCreated;
	    }
	}

    void TexturesCache_TextureCreated(StreamingTexturesCache arg1, StreamingTexture2D arg2, bool arg3) {
        
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnGUI() {
        GUILayout.Label(string.Format("Textures in cache: {0}", streamer.TexturesCache.Textures.Count));
    }
}
}
