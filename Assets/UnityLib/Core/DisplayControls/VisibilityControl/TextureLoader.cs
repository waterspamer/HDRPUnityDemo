using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Nettle {


public class TextureLoader {

    public static TextureFormat DxgiToUnityFormat(NPixelFormat dxgi_format) {
        if (dxgi_format == NPixelFormat.R8G8B8A8_UNORM)
            return TextureFormat.RGBA32;
        if (dxgi_format == NPixelFormat.B8G8R8A8_UNORM)
            return TextureFormat.BGRA32;
        if (dxgi_format == NPixelFormat.B8G8R8A8_UNORM_SRGB)
            return TextureFormat.ARGB32;
        if (dxgi_format == NPixelFormat.BC1_UNORM || dxgi_format == NPixelFormat.BC1_UNORM_SRGB)
            return TextureFormat.DXT1;
        if (dxgi_format == NPixelFormat.BC3_UNORM_SRGB || dxgi_format == NPixelFormat.BC3_UNORM)
            return TextureFormat.DXT5;
        if (dxgi_format == NPixelFormat.B8G8R8X8_UNORM || dxgi_format == NPixelFormat.B8G8R8X8_UNORM_SRGB || dxgi_format == NPixelFormat.B8G8R8X8_TYPELESS)
            return TextureFormat.RGB24;
        if (dxgi_format == NPixelFormat.R8_UNORM || dxgi_format == NPixelFormat.R8G8_TYPELESS)
            return TextureFormat.Alpha8;
        return TextureFormat.Alpha8;
    }

    public enum Command {
        ProcessDeviceObjects = 13,
    }

    private static Hashtable FormatRegistry = new Hashtable();

    public static bool GetTextureInfo(string path, out NTextureInfo info) {
        if (FormatRegistry.ContainsKey(path)) {
            info = (NTextureInfo)FormatRegistry[path];
            return true;
        }
        bool result = TextureLoaderDll.GetImageInfo(path, out info);
        if (result)
            FormatRegistry.Add(path, info);
        return result;
    }

    public static void PrecacheFormats(string directory, bool recursive) {
        if (System.IO.Directory.Exists(directory)) {
            var files = System.IO.Directory.GetFiles(directory, "*.*", recursive ? System.IO.SearchOption.AllDirectories : System.IO.SearchOption.TopDirectoryOnly);
            foreach (var file in files) {
                NTextureInfo info;
                if(file.EndsWith(".meta")){
                    continue;                
                }
                if (TextureLoaderDll.GetImageInfo(file, out info))
                {
                    if (!FormatRegistry.ContainsKey(file))
                        FormatRegistry.Add(file, info);
                }
                else
                    Debug.Log("Failed to get image info for: " + file);
            }
        }
    }

	public static List<string> CheckFormats(NPixelFormat targetFormat) {
		List<string> errorFormats = new List<string>();
		foreach (var e in FormatRegistry.Keys) {
			if (((NTextureInfo)FormatRegistry[e]).Format != targetFormat) {
				errorFormats.Add((string)e);
			}
		}

		return errorFormats;
	}
}
}
