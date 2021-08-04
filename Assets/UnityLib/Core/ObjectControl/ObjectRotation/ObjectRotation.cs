using UnityEngine;
using System.Collections;
using UnityEngine.Serialization;
using UnityEngine.Events;

namespace Nettle
{
[System.Serializable]
    public class RotationEvent : UnityEvent<Quaternion>
    {
    }

    public class ObjectRotation : ObjectRotationZoneViewerBase
    {

        public enum RotationSpace
        {
            World, Local, Screen
        }

        public Transform Display;

        public float SpeedMultiplier = 1.0f;
        public RotationSpace Space = RotationSpace.Local;
        public bool EnableVerticalRotation = true;
        public bool EnableHorizontalRotation = true;
        public Vector3 UpRotationAxis = Vector3.right;
        public Vector3 RightRotationAxis = Vector3.up;
        protected Quaternion _defaultRotation;
        public RotationEvent OnRotate;
        private bool _initialized = false;

        [FormerlySerializedAs("Target")]
        [SerializeField]
        protected Transform _target;
        public Transform Target
        {
            get
            {
                return _target;
            }

            set
            {

                if (_initialized && _target != null)
                {
                    ResetRotation();
                }
                _target = value;
                if (_target != null)
                {
                    _defaultRotation = _target.rotation;
                    _initialized = true;
                }
            }
        }

        public override void ResetRotation()
        {
            if (_target != null)
            {
                _target.localRotation = _defaultRotation;
            }
        }

        private void Reset()
        {
            if (Display == null)
            {
                MotionParallaxDisplay display = SceneUtils.FindObjectIfSingle<MotionParallaxDisplay>();
                if (display != null)
                {
                    Display = display.transform;
                }
            }
        }

        protected override void Awake()
        {
            base.Awake();
            if (_target != null)
            {
                Target = _target;
            }
        }

        private void Start()
        {            
            if (Space == RotationSpace.Screen && Display == null)
            {
                Display = StereoEyes.Instance.transform.parent;
            }
        }

        public void SetRotation(Quaternion rotation)
        {
            if (Target != null)
            {
                Target.rotation = rotation;
            }
        }

        public override void Rotate(Vector2 delta)
        {
            if (delta.x == 0 && delta.y == 0)
            {
                return;
            }            
            Space space = UnityEngine.Space.World;
            Vector3 rotationEuler = Vector3.zero;
            if (EnableHorizontalRotation)
            {
                rotationEuler += RightRotationAxis.normalized * delta.x;
            }
            if (EnableVerticalRotation)
            {
                rotationEuler += UpRotationAxis.normalized * delta.y;
            }
            if (Space != RotationSpace.Screen)
            {
                if (Space == RotationSpace.Local)
                {
                    space = UnityEngine.Space.Self;
                }
            }
            else if (Display != null)
            {
                rotationEuler = Vector3.zero;
                if (EnableHorizontalRotation)
                {
                    rotationEuler += Display.TransformDirection(RightRotationAxis).normalized * delta.x ;
                }
                if (EnableVerticalRotation)
                {
                    rotationEuler += Display.TransformDirection(UpRotationAxis).normalized * delta.y;
                }
            }
            _target.Rotate(rotationEuler * SpeedMultiplier, space);
            if (OnRotate != null)
            {
                OnRotate.Invoke(_target.rotation);
            }
        }
    }
}
