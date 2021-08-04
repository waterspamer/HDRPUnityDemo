using UnityEngine;

namespace Nettle {

public class BillBoard : MonoBehaviour {

    public GameObject Target;
    public Vector3 WorldForward = Vector3.forward;
    public bool Vertical = true;
    public bool UseWorldForward = false;

    private Quaternion _startRotation;

    private void Start() {
        _startRotation = transform.localRotation;
    }

    private void Update() {
        var targetWorldPos = Target.transform.position;
        if (Vertical) targetWorldPos.y = transform.position.y;

        var q = new Quaternion();

        if (UseWorldForward) {
            q.SetFromToRotation(WorldForward, targetWorldPos - transform.position);
            transform.rotation = q * _startRotation; //* transform.rotation;
        } else {
            q.SetFromToRotation(transform.forward, targetWorldPos - transform.position);
            transform.rotation = q * transform.rotation;
        }

        //transform.LookAt(targetWorldPos, WorldUp);
    }
}
}
