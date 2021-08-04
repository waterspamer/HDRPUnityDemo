using UnityEngine;
using System.Collections;

namespace Nettle {

public class SetMaterialOrder : MonoBehaviour {

    //public bool SetToAllMaterials = true;
    public bool SetInUpdate = true;
    public int DrawOrder = 2000;

    private Material[] _mats;

    private void SetMaterialsDrawOrder(Material[] mats, int id) {
        if(mats == null) { return; }
        foreach (var mat in mats) {
            if (mat != null) {
                mat.renderQueue = id;
            }
        }
    }

	void Start () {
	    var r = GetComponent<Renderer>();
        if (r == null) { return; }

	    _mats = r.materials;
        SetMaterialsDrawOrder(_mats, DrawOrder);
	}
	
	void Update () {
	    if (SetInUpdate) {
            SetMaterialsDrawOrder(_mats, DrawOrder);
        }
	}
}
}
