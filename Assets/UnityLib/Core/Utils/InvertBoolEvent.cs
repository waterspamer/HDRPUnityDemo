using System;
using UnityEngine;
using System.Collections;
using UnityEngine.Events;

namespace Nettle {

public class InvertBoolEvent : MonoBehaviour {
    [Serializable]
    public class BoolEvent : UnityEvent<bool> {}
    public BoolEvent Event = new BoolEvent();

    public void OnEvent(bool value) {
        if (Event != null) {
            Event.Invoke(!value);
        }
    }
}
}
