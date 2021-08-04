using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveByPosition : MonoBehaviour {
    
    public Bounds VisibleBounds;
    public GameObject ObjectsRoot;

    private Renderer[] _renderers;

    private Bounds GlobalBounds {
        get {
            return new Bounds(transform.TransformPoint(VisibleBounds.center), transform.TransformVector(VisibleBounds.size));
        }
    }

    private Bounds LocalBounds(Bounds global) {
        return new Bounds(transform.InverseTransformPoint(global.center), transform.InverseTransformVector(global.size));
    }
    // Use this for initialization
    void Start () {
        _renderers = ObjectsRoot.GetComponentsInChildren<Renderer>();
	}
	
	// Update is called once per frame
	void Update () {
        foreach (Renderer rend in _renderers) {
            rend.enabled = LocalBounds(rend.bounds).Intersects(VisibleBounds);
        }
	}

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(GlobalBounds.center, GlobalBounds.size);
        Gizmos.color = Color.white;
    }

}
