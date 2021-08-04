using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nettle
{
    public class DiscreteBillboard : MonoBehaviour
    {
        [SerializeField]
        private Vector3 _localForwardAxis = Vector3.forward;
        [SerializeField]
        private Vector3 _localRotationAxis = Vector3.up;
        [SerializeField]
        private float _rotationStep = 90;
        [SerializeField]
        private Vector3 _additionalRotation;
        private Quaternion _initialRotation;
        private Vector3 _worldForwardAxis;
        private Vector3 _worldRightAxis;        
        private Vector3 _worldRotationAxis;
        public Transform Target = null;

        private void Start()
        {
            if (Target == null)
            {
                Target = StereoEyes.Instance.transform;
            }
            _initialRotation = transform.rotation;
            _worldRotationAxis = transform.TransformDirection(_localRotationAxis);
            _worldForwardAxis = Vector3.ProjectOnPlane(transform.TransformDirection(_localForwardAxis),_worldRotationAxis);
            _worldRightAxis = Vector3.Cross(_worldRotationAxis,_worldForwardAxis);
        }

        private void Update()
        {
            transform.rotation = _initialRotation;
            Vector3 targetDirection = Vector3.ProjectOnPlane(Target.position - transform.position,_worldRotationAxis);
            float angle = Vector3.Angle(targetDirection, _worldForwardAxis);
            float angleStepFraction = angle % _rotationStep;
            if (angleStepFraction > _rotationStep / 2)
            {            
                angle += _rotationStep - angleStepFraction;
            }else
            {
                angle -= angleStepFraction;
            }
            angle *= Mathf.Sign(Vector3.Dot(targetDirection, _worldRightAxis));
            transform.Rotate(_worldRotationAxis*angle,Space.World);
            transform.Rotate(_additionalRotation,Space.Self);
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position,transform.TransformDirection(_localRotationAxis)*5);
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, transform.TransformDirection(_localForwardAxis)*5);
            Gizmos.color = Color.white;
        } 
    }
}