using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CopyObjectRotation : MonoBehaviour
{
    [SerializeField]
    private Transform _target;
    [SerializeField]
    private Vector3 _additionalRotation;
    
    void Update()
    {
        transform.rotation = Quaternion.Euler(_additionalRotation) * _target.rotation;
    }
}
