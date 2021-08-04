using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationSmooth : MonoBehaviour {
    [Range(0,1)]
    public float Smooth = 0.5f;
    public bool IgnoreX = false;
    public bool IgnoreY = false;
    public bool IgnoreZ = false;

    private Quaternion _startRotation;

    private void Awake() {
        _startRotation = transform.rotation;
    }

    private void LateUpdate() {
        Vector3 eulers = transform.rotation.eulerAngles;
        Vector3 newEulers = Quaternion.Slerp(transform.rotation, _startRotation, Smooth).eulerAngles;
        if (IgnoreX) {
            newEulers.x = eulers.x ;
        }
        if (IgnoreY) {
            newEulers.y = eulers.y;
        }
        if (IgnoreZ) {
            newEulers.z = eulers.z;
        }
        transform.rotation = Quaternion.Euler(newEulers);
    }
}
