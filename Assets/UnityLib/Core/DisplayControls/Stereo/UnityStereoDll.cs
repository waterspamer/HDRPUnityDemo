using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System;

namespace Nettle {

public static class UnityStereoDll  {
#if !UNITY_WEBGL
    [DllImport("UnityStereo")]
    public static extern void Dummy();
    [DllImport("UnityStereo")]
    public static extern IntPtr GetRenderEventFunc();
#else
        public static void Dummy()
        {
        }
        public static IntPtr GetRenderEventFunc()        
        {
            return new IntPtr();
        }
#endif

        public enum GraphicsEvent : int {
        SetLeftEye = 0,
        SetRightEye = 1
    };
}


}
