using System;
using UnityEngine;

namespace Nettle {

public class ColliderTriggerEvents: MonoBehaviour {


    public Action<Collider> TriggerEnter;
    public Action<Collider> TriggerExit;
    public Action<Collider> TriggerStay;

    void OnTriggerEnter(Collider collider) {
        if (TriggerEnter != null) {
            TriggerEnter.Invoke(collider);
        }
    }

    void OnTriggerExit(Collider collider) {
        if (TriggerExit != null) {
            TriggerExit.Invoke(collider);
        }
    }

    void OnTriggerStay(Collider collider) {
        if (TriggerStay != null) {
            TriggerStay.Invoke(collider);
        }
    }

    
        
        
        
        

}
}
