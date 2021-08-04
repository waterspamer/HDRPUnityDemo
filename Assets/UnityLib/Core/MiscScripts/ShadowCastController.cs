using UnityEngine;
using UnityEngine.Rendering;

namespace Nettle {
    public class ShadowCastController : MonoBehaviour {
        [SerializeField]
        private bool _castShadow;
        [SerializeField]
        private bool _receiveShadow;

        private MeshRenderer[] _meshRenderers;

        [EasyButtons.Button]
        public void SetShadowsParams() {
            _meshRenderers = GetComponentsInChildren<MeshRenderer>(true);
            foreach (var renderer in _meshRenderers) {
                renderer.receiveShadows = _receiveShadow;
                renderer.shadowCastingMode = _castShadow ? ShadowCastingMode.On : ShadowCastingMode.Off;
                ;
            }
        }
    }
}
