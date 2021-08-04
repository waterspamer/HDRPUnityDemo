using System;
using UnityEngine;

namespace Nettle {

public class MotionParallaxLODLevel: MonoBehaviour {

    public Action<int> VisibleEvent;
    public Action<int> InvisibleEvent;
    public int LodLevel;

    /*void Awake() {
        _lodLevel = int.Parse(name.Substring(name.LastIndexOf("LOD") + 3));
    }*/
    

    void OnBecameVisible() {
        if (VisibleEvent != null) {
            VisibleEvent.Invoke(LodLevel);
        }
    }

    void OnBecameInvisible() {
        if (InvisibleEvent != null) {
            InvisibleEvent.Invoke(LodLevel);
        }
    }

}
}
