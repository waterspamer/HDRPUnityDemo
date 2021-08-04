using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Nettle {

[Serializable]
public class VisibilityToggleEvent : UnityEvent<bool> { }

public class VisibilityToggle : MonoBehaviour {

    public List<GameObject> objects = new List<GameObject>();
    public VisibilityToggleEvent visibilityChangedEvent;

    public void ToggleVisible(bool on) {
        
        if (objects.Count > 0) {
            foreach (GameObject obj in objects) {
                obj.SetActive(on);
            }
        } else {
            gameObject.SetActive(on);
        }
        InvokeVisibilityChangedEvent();
    }

    public void InvokeVisibilityChangedEvent() {
        if (visibilityChangedEvent != null) {
            if (objects.Count > 0) {
                visibilityChangedEvent.Invoke(objects[0].activeSelf);
            } else {
                visibilityChangedEvent.Invoke(gameObject.activeSelf);
            }
        }
    }

}
}
