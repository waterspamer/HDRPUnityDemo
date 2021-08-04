using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnforceConstantTransform : MonoBehaviour {
    public bool EnforcePosition = true;
    public Vector3 Position;
    public bool EnforceRotation = true;
    public Vector3 Rotation;
    public bool EnforceScale = true;
    public Vector3 Scale = Vector3.one;
    private void LateUpdate()
    {
        if (EnforcePosition)
        {
            transform.localPosition = Position;
        }
        if (EnforceRotation)
        {
            transform.localRotation = Quaternion.Euler(Rotation);
        }
        if (EnforceScale)
        {
            transform.localScale = Scale;
        }
    }
}