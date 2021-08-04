using UnityEngine;
using System.Collections;

namespace Nettle {

public class ScaleLink : MonoBehaviour {

    public Transform SourceTransform;
    public Transform TargetTransform;

    public float SourceScale = 1.0f;
    public float LinkMultiplier = 1.0f;

    private Vector3 _targetStartScale;

    void Start() {
        _targetStartScale = TargetTransform.localScale;
    }
	
	void Update () {
	    var offset = (SourceTransform.localScale.x - SourceScale) * LinkMultiplier;
        TargetTransform.localScale = new Vector3(_targetStartScale.x + offset, _targetStartScale.y + offset, _targetStartScale.z + offset);


    }
}
}
