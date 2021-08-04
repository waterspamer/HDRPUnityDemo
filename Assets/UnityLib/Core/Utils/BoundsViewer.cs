using UnityEngine;
using System.Collections;

namespace Nettle {

public class BoundsViewer : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnDrawGizmos() {
       var bounds = GetComponent<MeshFilter>().sharedMesh.bounds.Transform(transform);
        Gizmos.DrawWireCube(bounds.center, bounds.size);
    }
}
}
