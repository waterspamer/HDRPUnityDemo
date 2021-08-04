using UnityEngine;
using System.Collections;
using UnityEngine.Serialization;
using UnityEngine.Events;

namespace Nettle
{
    public class ObjectRotationLimited : ObjectRotation
    {
        public float MaxVerticalAngle = 85;
        public float MinVerticalAngle = -85;
        public bool InvertHorizontal = false;
        public bool InvertVertical = false;
        private Vector2 _currentRotation;
        
        protected override void Awake()
        {
            base.Awake();
            if (Display == null)
            {
                enabled = false;
                Debug.LogError("ObjectRotationLimited requires a reference to Display transform!");
            }
        }

        
        public override void ResetRotation()
        {
            base.ResetRotation();
            _currentRotation = Vector2.zero;
        }

        public override void Rotate(Vector2 delta)
        {
            if (delta.x == 0 && delta.y == 0)
            {
                return;
            }   
            Target.rotation = _defaultRotation;

            if (EnableHorizontalRotation)
            {
                _currentRotation.x = Mathf.Repeat(_currentRotation.x + delta.x * SpeedMultiplier, 360);
                Target.RotateAround(Target.position, Target.TransformDirection(-Vector3.up) * (InvertHorizontal?-1:1), _currentRotation.x);
            }

            if (EnableVerticalRotation)
            {
                _currentRotation.y = Mathf.Clamp(_currentRotation.y + delta.y * SpeedMultiplier,MinVerticalAngle,MaxVerticalAngle);
                Target.RotateAround(Target.position, Display.TransformDirection(Vector3.right)* (InvertVertical?-1:1), _currentRotation.y);
            }
            if (OnRotate != null)
            {
                OnRotate.Invoke(_target.rotation);
            }
        }
    }
}
