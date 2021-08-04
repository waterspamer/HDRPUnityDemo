using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectRotation2Axis : ObjectRotationZoneViewerBase {



    public float SpeedMultiplier = 0.5f;
    public Transform Target;

    public Vector3 HorizontalRotationAxis = Vector3.up;
    public Vector3 VerticalRotationAxis = Vector3.right;
    public bool LimitXRotation = false;
    public float MaxXRotation = 90;
    public bool LimitYRotation = true;
    public float MaxYRotation = 90;
    private Vector3 _defaultAngle;
    private float _currentXRotation;
    private float _currentYRotation;

    protected override void Awake() {
        _defaultAngle = Target.eulerAngles;
        base.Awake();
    }

    private void OnEnable() {
        ResetRotation();
    }

    public override void ResetRotation() {
        Target.localRotation = Quaternion.Euler(_defaultAngle);
        _currentXRotation = 0f;
        _currentYRotation = 0f;
    }

    public override void Rotate(Vector2 delta) {
        float xDelta = -delta.x * SpeedMultiplier;
        if (LimitXRotation) {
            float prevRotation = _currentXRotation;
            _currentXRotation = Mathf.Clamp(_currentXRotation + xDelta, -MaxXRotation, MaxXRotation);
            xDelta = _currentXRotation - prevRotation;
        }

        float yDelta = delta.y * SpeedMultiplier;
        if (LimitYRotation) {
            float prevRotation = _currentYRotation;
            _currentYRotation = Mathf.Clamp(_currentYRotation + yDelta, -MaxYRotation, MaxYRotation);
            yDelta = _currentYRotation - prevRotation;
        }

        Target.Rotate(HorizontalRotationAxis, xDelta);
        Target.Rotate(Target.transform.InverseTransformVector(VerticalRotationAxis), yDelta);
    }

    public void SetTarget(Transform target) {
        Target = target;
    }

}
