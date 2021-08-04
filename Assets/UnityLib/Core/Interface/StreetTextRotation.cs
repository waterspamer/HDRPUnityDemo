using UnityEngine;

namespace Nettle {

public class StreetTextRotation : MonoBehaviour {
	
	public Transform Target;
    [Range(0, 1)]
	public float DeadZone = 0.1f;
    public float GizmoScale = 10.0f;
    public Vector3 LocalFlipPlaneNormal = Vector3.up;
    public Vector3 LocalRotationAxis = Vector3.forward;
    public bool DrawTargetLine = true;

    //TODO: check if anyone uses this bool
    [HideInInspector]
    public bool Front;

	void Update () {
	    if (Target == null&& StereoEyes.Instance!=null) {
                Target = StereoEyes.Instance.transform;
            }

	    Vector3 localSpaceTarget = transform.InverseTransformPoint(Target.transform.position).normalized;
	    float cosAngle = Vector3.Dot(LocalFlipPlaneNormal, localSpaceTarget);

	    if (Mathf.Abs(cosAngle) > DeadZone && cosAngle < 0) {
	        transform.Rotate(LocalRotationAxis, 180.0f, Space.Self);
	    }else{ }
	}

    void OnDrawGizmosSelected()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(Vector3.zero, LocalFlipPlaneNormal.normalized * GizmoScale * 2);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(Vector3.zero, LocalRotationAxis * GizmoScale);

        if (Target != null && DrawTargetLine){
            var targetLocalSpace = transform.InverseTransformPoint(Target.position);
            Front = Vector3.Dot(LocalFlipPlaneNormal, targetLocalSpace) > 0.0f;
            Gizmos.color = Color.red;
            Gizmos.DrawLine(Vector3.zero, targetLocalSpace);
        }
    }
}
}
