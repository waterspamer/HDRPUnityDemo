using UnityEngine;
using System.Collections;

namespace Nettle {

public class ObjectRotationBroadcast : ObjectRotationBase {
    public ObjectRotation[] Targets;

    public override void ResetRotation() {
        if (Targets != null) {
            foreach (var target in Targets) {
                if (target != null && target.gameObject.activeInHierarchy && target != this) {
                    target.ResetRotation();
                }
            }
        }
    }

    public override void Rotate(Vector2 delta) {
        if (Targets != null) {
            foreach (var target in Targets) {
                if (target != null && target.gameObject.activeInHierarchy && target != this) {
                    target.Rotate(delta);
                }
            }
        }
    }
}
}
