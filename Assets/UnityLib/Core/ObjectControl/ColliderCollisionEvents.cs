using System;
using UnityEngine;
using UnityEngine.Events;

namespace Nettle {

public class ColliderCollisionEvents: MonoBehaviour {

    public Action<Collision> CollisionEnter;
    public Action<Collision> CollisionExit;
    public Action<Collision> CollisionStay;

    void OnCollisionEnter(Collision collision) {
        if (CollisionEnter != null) {
            CollisionEnter.Invoke(collision);
        }
    }

    void OnCollisionExit(Collision collision) {
        if (CollisionExit != null) {
            CollisionExit.Invoke(collision);
        }
    }

    void OnCollisionStay(Collision collision) {
        if (CollisionStay != null) {
            CollisionStay.Invoke(collision);
        }
    }
}
}
