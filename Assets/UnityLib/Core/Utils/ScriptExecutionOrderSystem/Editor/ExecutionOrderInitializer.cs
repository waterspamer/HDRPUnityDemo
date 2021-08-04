using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Nettle {

[InitializeOnLoad]
public class ExecutionOrderInitializer : Editor {
    static ExecutionOrderInitializer() {
        ExecutionOrderManager.Instance.SetOrder();
    }
}
}
