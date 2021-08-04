using UnityEngine;

namespace Nettle {

public class VisibilityLabels : MonoBehaviour {
    public Language Lang = Language.Ru;
    private MeshRenderer _meshRenderer;

    private void Reset() {
        if (gameObject.name.StartsWith("en_")) {
            Lang = Language.En;
        } else {
            Lang = Language.Ru;
        }
    }

    void Awake() {
        _meshRenderer = gameObject.GetComponent<MeshRenderer>();
    }

    public void Show() {
        _meshRenderer.enabled = true;
    }

    public void Hide() {
        _meshRenderer.enabled = false;
    } 
}
}
