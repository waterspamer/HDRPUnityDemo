using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Nettle {
    [ExecuteAfter(typeof(DefaultTime))]
    public class Wheel : MonoBehaviour {

        public float Radius = 0.35f;
        public Vector3 Normal = Vector3.forward;

        public float DeltaThreshold = 0.01f;

        private Vector3 _previousWheelPosition;
        private Vector3 _velocity;

        private float _shaftAngle;
        private Vector3 _defaultAngles;
        private float _cirleLen;

        public Vector3 GetVelocity() {
            return _velocity;
        }

        void Awake() {
            _defaultAngles = transform.localEulerAngles;
            _cirleLen = 2.0f * Mathf.PI * Radius;
        }

        void Start() {
            _previousWheelPosition = transform.position;
        }

        public Vector3 GetWorldShaftAxis() {
            return transform.right;
        }

        void LateUpdate() {
            var nowPosition = transform.position;
            var posDelta = nowPosition - _previousWheelPosition;
            var posDeltaLength = posDelta.magnitude;
            if (posDeltaLength < DeltaThreshold) {
                return;
            }
            float direction = Mathf.Sign(Vector3.Dot(-Vector3.Cross(transform.TransformVector(Normal), posDelta), Vector3.up));
            _previousWheelPosition = nowPosition;
            _velocity = posDelta / Time.deltaTime;
            var r = posDeltaLength / _cirleLen;
            _shaftAngle += r * 360.0f * direction;
            transform.localRotation = Quaternion.AngleAxis(_shaftAngle, Normal);

            transform.localEulerAngles += _defaultAngles ;
        }



#if UNITY_EDITOR
        void OnDrawGizmosSelected() {
            Handles.matrix = transform.localToWorldMatrix;
            Handles.color = Color.green;
            Handles.DrawWireDisc(Vector3.zero, Normal, Radius);
            Handles.matrix = Matrix4x4.identity;

            Gizmos.DrawLine(transform.position, transform.position + _velocity);

            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + transform.TransformVector(Normal));

            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position - Vector3.Cross(transform.TransformVector(Normal), transform.position + _velocity));
        }
#endif
    }
}
