using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MassRotator : MonoBehaviour {

    public Transform[] ObjectsToRotate;
    public Vector3 SpinAxis = Vector3.up;
    public Space RotationSpace = Space.Self;
    public float SpinSpeed = 1;
    public bool RandomStartPhase = false;
    [SerializeField]
    private bool _resetRotationBeforeStart;

    public bool ResetRotationBeforeStart { get => _resetRotationBeforeStart; set => _resetRotationBeforeStart = value; }

    private void Start() {
        if (RandomStartPhase) {
            foreach (Transform tr in ObjectsToRotate) {
                tr.Rotate(SpinAxis.normalized * Random.Range(0f, 360.0f), RotationSpace);
            }
        }
    }
    private void OnEnable() {
        if (ResetRotationBeforeStart) {
            foreach(Transform tr in ObjectsToRotate) {
                tr.rotation = Quaternion.identity;
            }
        }
    }
    // Update is called once per frame
    void Update () {
        Rotate();
	}

    protected virtual void Rotate() {
        foreach (Transform tr in ObjectsToRotate) {
            tr.Rotate(SpinAxis.normalized * SpinSpeed * Time.deltaTime, RotationSpace);
        }
    }
}
