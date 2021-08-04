using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nettle {

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public abstract class ExecutionOrderAttribute : Attribute {
    public Type OtherScript { get; protected set; }
    public ExecutionOrderAttribute(Type script) {
        if (script.IsSubclassOf(typeof(MonoBehaviour))){
            OtherScript = script;
        }
    }
}

public class ExecuteAfterAttribute : ExecutionOrderAttribute {
    public ExecuteAfterAttribute(Type script):base(script) {
    }
}

public class ExecuteBeforeAttribute : ExecutionOrderAttribute {
    public ExecuteBeforeAttribute(Type script) : base(script) {
    }
}

/// <summary>
/// This class is defined only for use in execution order attributes, to determine whether the script should go before or after default time
/// </summary>
public abstract class DefaultTime : MonoBehaviour {
}
}
