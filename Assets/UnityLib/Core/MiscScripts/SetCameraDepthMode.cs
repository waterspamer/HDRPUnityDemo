using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SetCameraDepthMode : MonoBehaviour {
    [SerializeField]
    private DepthTextureMode mode = DepthTextureMode.Depth;
    private Camera targetCamera;
    private void Awake() {
        targetCamera = GetComponent<Camera>();
    }

    private void OnEnable() {
        if (targetCamera != null) {
            targetCamera.depthTextureMode = mode;
        }
    }

    private void OnDisable() {
        if (targetCamera != null) {
            targetCamera.depthTextureMode = DepthTextureMode.None;
        }
    }
}
