using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nettle {

public class InfoLogger : MonoBehaviour {

    private void Reset() {
        Debug.LogError("Set correct BroadCast key on Network component");   
    }
}
}
