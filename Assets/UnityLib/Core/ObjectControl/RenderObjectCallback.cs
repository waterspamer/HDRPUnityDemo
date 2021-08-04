using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Nettle {

[ExecuteInEditMode]
public class RenderObjectCallback : MonoBehaviour {
    public UnityEvent WillRenderObject;
    private void OnWillRenderObject() {
        if (WillRenderObject != null) {
            WillRenderObject.Invoke();
        }
    }
}
}
