using System.Runtime.InteropServices;
using Nettle;
using UnityEngine;

public class FastSyncEnabler : MonoBehaviour {

#if (!UNITY_WEBGL && UNITY_64)
    [DllImport("FastSyncNVAPI", CallingConvention = CallingConvention.Cdecl)]
    private static extern bool TryEnableFastSync();

    [ConfigField("FastSync")]
    public bool EnableFastSync = true;

    void Start() {
        if (EnableFastSync) {
            bool success = TryEnableFastSync();
            Debug.Log("FastSync enabled::" + success);
            Destroy(this);
        }
    }
#endif
}

