using UnityEngine;
using System.Collections;

namespace Nettle {

public class DontDestroy : MonoBehaviour {
    public bool Use = true;

    private void Awake() {
        if (Use) {
            DontDestroyOnLoad(transform.gameObject);
        }
    }
}
}
