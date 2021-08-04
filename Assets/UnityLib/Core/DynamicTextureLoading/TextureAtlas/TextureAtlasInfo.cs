using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nettle {

public class TextureAtlasInfo : ScriptableObject {
    [System.Serializable]
    public class AtlasedObject {
        public Vector2 UVOffset;
        public Vector2 UVScale;
        public string ObjectName;
        public string ImageType;
    }

    [System.Serializable]
    public class Atlas {
        public Texture2D Texture;
        public List<AtlasedObject> Objects = new List<AtlasedObject>();
    }

    public List<Atlas> Atlases = new List<Atlas>();
    [Tooltip("The name of the model file that uses these atlases (without file extension)")]
    public string ModelFileName;


}
}
