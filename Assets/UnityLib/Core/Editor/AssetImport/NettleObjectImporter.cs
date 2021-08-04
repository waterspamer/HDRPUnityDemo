using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nettle {

public abstract class NettleObjectImporter {

    protected abstract string TargetProperyName {
        get;
    }

    public void ProcessObject(GameObject gameObject, string propertyName, string propertyValue, string assetPath) {
        if (propertyName == TargetProperyName && gameObject != null) {
            ProcessObject(gameObject, propertyValue, assetPath);
        }
    }

    protected abstract void ProcessObject(GameObject gameObject, string propertyValue, string assetPath);
}
}
