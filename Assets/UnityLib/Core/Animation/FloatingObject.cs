using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingObject : MonoBehaviour {
    [SerializeField]
    private float _positionMagnitude = 0.1f;
    [SerializeField]
    private float _rotationMagnitude = 1;
    [SerializeField]
    private Vector3 _rotationAxis = new Vector3 (1, 0, 0);
    [SerializeField]
    private float _speedScale = 1;

    private Vector3 _startPosition;
    private Quaternion _startRotation;

    private void Awake () {
        _startPosition = transform.localPosition;
        _startRotation = transform.localRotation;
    }

    void Update () {
        float sin = Mathf.Sin (Time.realtimeSinceStartup * _speedScale + transform.position.x - transform.position.z);
        Vector3 upDir = Vector3.up;
        if (transform.parent != null) {
            upDir = transform.parent.InverseTransformDirection (Vector3.up);
        }
        transform.localPosition = _startPosition + upDir* sin * _positionMagnitude / 2;
        transform.localRotation = _startRotation * Quaternion.Euler (_rotationAxis * sin * _rotationMagnitude);
    }
}