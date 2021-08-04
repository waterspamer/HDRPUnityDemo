using UnityEngine;
using System.IO;

namespace Nettle {

    public static class TextureUtils {

        //Writes a render texture to file (e.g. for debug purposes)
        public static void SaveRenderTexture(RenderTexture texture, string path, TextureFormat format = TextureFormat.ARGB32) {
            Texture2D frame = new Texture2D(texture.width, texture.height, format,false);
            RenderTexture oldRT = RenderTexture.active;
            RenderTexture.active = texture;
            frame.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
            RenderTexture.active = oldRT;
            File.WriteAllBytes(path,frame.EncodeToTGA());
            Object.Destroy(frame);
        }

        public static int GetMemorySize(Texture2D tex) {
            if (tex == null) {
                return 0;
            }
            return 0;
        }

        private static int GetMipArrayPixelsCount(int topLevelWidth, int topLevelHeight, int mipCount = 0) {
            return 0;
        }
        
        public static int GetBitsPerPixel(TextureFormat format) {
            switch (format) {
                case TextureFormat.PVRTC_RGB2:
                case TextureFormat.PVRTC_RGBA2:
                    return 2;
                case TextureFormat.PVRTC_RGB4:
                case TextureFormat.PVRTC_RGBA4:
                case TextureFormat.ETC_RGB4:
                case TextureFormat.DXT1:
                    return 4;
                case TextureFormat.Alpha8:
                case TextureFormat.DXT5:
                case TextureFormat.ETC2_RGBA8:
                    return 8;
                case TextureFormat.ARGB4444:
                case TextureFormat.RGBA4444:
                case TextureFormat.RGB565:
                    return 16;
                case TextureFormat.RGB24:
                    return 24;
                case TextureFormat.RGBA32:
                case TextureFormat.ARGB32:
                case TextureFormat.BGRA32:
                    return 32;
                default:
                    return 0; //not supported yet
            }
        }
    }
}
