using System;
using UnityEngine;
using System.Collections;

namespace Nettle {

[Obsolete("Use RotateByAngle", true)]
public class Rotate180 : MonoBehaviour {

    public GameObject Target;
    public float TimeOfRotating = 1f;

    private Quaternion _rotation180 = Quaternion.identity;

    //rotate animation
    private Quaternion _startPointOfRotation = Quaternion.identity;
    private Quaternion _endPointOfRotation = Quaternion.identity;
    private const float START_SIN_RAD = -Mathf.PI / 2f;
    private const float END_SIN_RAD = Mathf.PI / 2f;
    private float _rotationStartTime = 0f;
    private bool _rotationInProcess = false;
    private float _rotationTime = 0.0f;

    void Awake() {
        _rotation180 = Quaternion.AngleAxis(180, Vector3.up);
    }

    public void Rotate() {
        _rotationStartTime = Time.realtimeSinceStartup;
        _startPointOfRotation = Target.transform.rotation;
        _endPointOfRotation = (_rotationInProcess ? _endPointOfRotation : _startPointOfRotation) * _rotation180;

        StopAllCoroutines();
        _rotationTime = 0.0f;
        StartCoroutine(MakeRotationAnimationCoroutine());
    }

    IEnumerator MakeRotationAnimationCoroutine() {
        _rotationInProcess = true;
        while (_rotationTime < TimeOfRotating) {
            _rotationTime = Time.realtimeSinceStartup - _rotationStartTime;
            var progressOfRotation = (Mathf.Sin(Mathf.Lerp(START_SIN_RAD, END_SIN_RAD, _rotationTime / TimeOfRotating)) + 1f) / 2f;
            //Debug.Log("Progress: " + progressOfRotation + ", rotation time: " + _rotationTime + ", time since scene startup: " + Time.realtimeSinceStartup + ", rotation start: " + _rotationStartTime);
            Target.transform.rotation = Quaternion.Lerp(_startPointOfRotation, _endPointOfRotation, progressOfRotation);
            yield return null;
        }
        _rotationInProcess = false;
    }
}
}
