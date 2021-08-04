using UnityEngine;

namespace Nettle.CloudSystem {
    
[ExecuteInEditMode]
public class Dithering3DTextureCreator : MonoBehaviour {

public Texture2D Texture2D;
    private Texture3D _texture3d;
    private void Awake() {
        _texture3d = CreateTexture3D();
        Shader.SetGlobalTexture("_MegaDitherMaskLOD3D", _texture3d);    
    } 

    Texture3D CreateTexture3D() {
        int width = 8;
        int height = 8;
        int depth = 64;
        var texture = new Texture3D(width, height, depth, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Repeat;
        Color[] colorArray = new Color[width * height * depth];
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                for (int z = 0; z < depth; z++) {
                     colorArray[x + y * width + z * width * height] = Texture2D.GetPixel(x + width * z, y);
                }
            }
        }
        texture.SetPixels(colorArray);
        texture.Apply();
        return texture;
    }
}

}
