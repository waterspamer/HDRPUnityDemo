using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nettle
{
    public class RotateDisplayTowardsUser : MonoBehaviour
    {
        [SerializeField]
        private RotateByAngle _rotateByAngle;
        [SerializeField]
        [Range(0,1.0f)]
        private float _angleDetectionBias = 0.1f;

        private void Reset()
        {
            _rotateByAngle = SceneUtils.FindObjectIfSingle<RotateByAngle>();
        }

        #if !UNITY_EDITOR
        private void Update()
        {
            if (!_rotateByAngle.RotationInProgress)
            {   
                Vector3 eyesDir =  StereoEyes.Instance.transform.position - VisibilityZoneViewer.Instance.ActiveZone.transform.position;
                if (Vector3.Dot(eyesDir.normalized, VisibilityZoneViewer.Instance.ActiveZone.transform.forward) > _angleDetectionBias)
                {
                    _rotateByAngle.Rotate(180,false);
                }
            }
        }

        private void OnEnable()
        {
            _rotateByAngle.DisableManualRotation = true;
        }

        private  void OnDisable()
        {
            _rotateByAngle.DisableManualRotation = false;
        }
        #endif
    }
}