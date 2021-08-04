using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Script controlling the camera that renders texture for magnifying glass.
/// </summary>
public class MagnifierCamera : MonoBehaviour {
    [SerializeField]
    private Transform _magnifierTransform;
    [SerializeField]
    private Camera _targetCamera;
    [SerializeField]
    [Range(0,1)]
    private float _parallaxStrength = 0;

    private void Reset() {
        _targetCamera = Camera.main;
    }

    private void OnPreRender() {
        Vector3 relativeDirection = _magnifierTransform.InverseTransformDirection((_magnifierTransform.position - _targetCamera.transform.position).normalized);
        transform.rotation = Quaternion.LookRotation(transform.parent.TransformDirection(relativeDirection)* _parallaxStrength,transform.parent.up);
    }
}
