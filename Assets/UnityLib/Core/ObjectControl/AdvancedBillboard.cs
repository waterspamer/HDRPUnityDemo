using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Nettle {
    public class AdvancedBillboard : MonoBehaviour {

        public enum RotationOrderEnum {
            xyz = 0 | (1 << 8) | (2 << 16),
            xzy = 0 | (2 << 8) | (1 << 16),
            yxz = 1 | (0 << 8) | (2 << 16),
            yzx = 1 | (2 << 8) | (0 << 16),
            zxy = 2 | (0 << 8) | (1 << 16),
            zyx = 2 | (1 << 8) | (0 << 16)
        }

        public Transform Target;

        public bool EnableX;
        public bool EnableY = true;
        public bool EnableZ;

        public Vector3 LocalX = Vector3.right;
        public Vector3 LocalY = Vector3.up;
        public Vector3 LocalZ = Vector3.forward;

        public RotationOrderEnum RotationOrder = RotationOrderEnum.xyz;
        public Vector3 FinalRotationOffset = Vector3.zero;

        private Quaternion _rotationBeforeOffset;

        private void Start() {
            if (Target == null) {
                if (StereoEyes.Instance != null) {
                    Target = StereoEyes.Instance.transform;
                } else {
                    Target = Camera.main.transform;
                }
            }

            _rotationBeforeOffset = transform.localRotation;
        }

        void Rotate(Vector3 planeNormal, Vector3 axis, Vector3 localTarget) {
            var p = new Plane(planeNormal, 0);
            Vector3 projectedTarget = p.ProjectPoint(localTarget);
            var rotation = Quaternion.FromToRotation(axis, projectedTarget);
            if (planeNormal.x * rotation.x + planeNormal.y * rotation.y + planeNormal.z * rotation.z != 0) {  //fix bag, when rotate around incorrect axis
                transform.localRotation = transform.localRotation * rotation;
            }
        }


        void LateUpdate() {
            transform.localRotation = _rotationBeforeOffset;
            var localTarget = transform.InverseTransformPoint(Target.position);
            float testEpsilon = 0.0001f;
            if (localTarget.sqrMagnitude >= testEpsilon) {
                int order = (int)RotationOrder;
                for (int i = 0; i < 3; ++i) {
                    int id = (order & (0xff << (i * 8))) >> (i * 8);
                    if (id == 0 && EnableX) {
                        Rotate(LocalX, LocalZ, localTarget);
                    } else if (id == 1 && EnableY) {
                        Rotate(LocalY, LocalZ, localTarget);
                    } else if (id == 2 && EnableZ) {
                        Rotate(LocalZ, LocalY, transform.InverseTransformPoint(Target.position + Target.up));
                    }
                }

                _rotationBeforeOffset = transform.localRotation;
                transform.localRotation *= Quaternion.Euler(FinalRotationOffset);
            }
        }

        void OnDrawGizmosSelected() {
            Gizmos.matrix = transform.localToWorldMatrix;

            float gizmoScale = 1;

            Gizmos.color = Color.red;
            Gizmos.DrawLine(Vector3.zero, LocalX * gizmoScale);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(Vector3.zero, LocalY * gizmoScale);
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(Vector3.zero, LocalZ * gizmoScale);

#if UNITY_EDITOR
            Handles.matrix = transform.localToWorldMatrix;
            if (Target != null) {
                Handles.DrawDottedLine(Vector3.zero, transform.InverseTransformPoint(Target.position), 3);
            }

            Handles.matrix = Matrix4x4.identity;
#endif
        }

    }
}
