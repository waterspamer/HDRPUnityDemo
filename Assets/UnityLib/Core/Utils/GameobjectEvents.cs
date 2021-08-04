using UnityEngine;
using System.Collections;
using UnityEngine.Events;

namespace Nettle {

public class GameobjectEvents : MonoBehaviour {
    public UnityEvent Enable = new UnityEvent();
    public UnityEvent Disable = new UnityEvent();

    private void OnEnable() {
        if (Enable != null) {
            Enable.Invoke();
        }
    }

    private void OnDisable() {
        if (Disable != null) {
            Disable.Invoke();
        }
    }
}
}
