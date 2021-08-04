using UnityEngine;
using System.Runtime.InteropServices;
using System;

namespace Nettle {

public static class TextureLoaderDll {
    public const string DllName = "UnityTextureLoader";
#if !UNITY_WEBGL
    [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
    public static extern void CancelTextureStreaming(IntPtr context);

    [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
    public static extern void ReleaseStreamingContext(IntPtr context);

    [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
    public static extern bool IsStreamingFinished(IntPtr context);

    [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
    public static extern void OnStart();

    [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
    public static extern void OnDestroy();

    [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
    public static extern IntPtr CreateStreamingContext(IntPtr dstTex);

    [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
    public static extern void BeginTextureStreaming(IntPtr context, [MarshalAs(UnmanagedType.LPWStr)] string path, ref NTextureInfo src_info, ref NTextureInfo dst_info);

    [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
    public static extern bool GetImageInfo([MarshalAs(UnmanagedType.LPWStr)] string file, out NTextureInfo info);

	[DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
	public static extern void DebugPrintThreadId([MarshalAs(UnmanagedType.LPStr)]string prefixText);
#else
        public static void CancelTextureStreaming(IntPtr context)
        {
        }
        public static void ReleaseStreamingContext(IntPtr context)
        {
        }
        public static bool IsStreamingFinished(IntPtr context)
        {
            return false;
        }
        public static void OnStart()
        {
        }
        public static void OnDestroy()
        {
        }
        public static IntPtr CreateStreamingContext(IntPtr dstTex)
        {
            return new IntPtr();
        }
        public static void BeginTextureStreaming(IntPtr context, [MarshalAs(UnmanagedType.LPWStr)] string path, ref NTextureInfo src_info, ref NTextureInfo dst_info)
        {
        }
        public static bool GetImageInfo([MarshalAs(UnmanagedType.LPWStr)] string file, out NTextureInfo info)
        {
            info = new NTextureInfo();
            return false;
        }
        public static void DebugPrintThreadId([MarshalAs(UnmanagedType.LPStr)] string prefixText)
        {
        }
#endif

        public static void ProcessDeviceObjects() {
        GL.IssuePluginEvent((int)TextureLoader.Command.ProcessDeviceObjects);
    }
}
}
