using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;


namespace Nettle {
    public interface iRuntimeTextureLoader {
        Texture2D Load(string path);   
    }
}
