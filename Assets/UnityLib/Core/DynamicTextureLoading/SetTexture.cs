using UnityEngine;
using System.Collections;

namespace Nettle {

public abstract class SetTexture : MonoBehaviour {
    public string parameterName;

    protected abstract Texture2D GetTexture();

    public void Apply() {
        Apply(gameObject, GetTexture(), parameterName);
    }

    public static void Apply(GameObject target, Texture2D tex, string parameterName) {
        var mr = target.GetComponent<Renderer>();
        if (mr == null) {
            Debug.LogWarningFormat("SetTexture: no renderer on {0}", target.name);
            return;
        }

        var materials = mr.materials;

        foreach (var material in materials) {
            if (material != null && material.HasProperty(parameterName)) {
                material.SetTexture(parameterName, tex);
            }
        }
    }
}
}
