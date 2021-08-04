using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nettle {

    public class MaterialRandomizer : MonoBehaviour {


        [SerializeField]
        private Material[] _variations;

        public List<Renderer> Renderers;

        // Use this for initialization
        void Start() {
            for (int i = 0; i < _variations.Length; i++) {
                if (_variations[i] == null) {
                    Debug.LogError("Material randomizer " + gameObject.name + " contains null material at position " + i, gameObject);
                }
            }
            if (_variations.Length > 0) {
                Material material = _variations[Random.Range(0, _variations.Length)];
                if (Renderers!=null && Renderers.Count == 0) {
                    Renderer renderer = GetComponent<Renderer>();
                    if (renderer != null) {
                        renderer.sharedMaterial = material;
                    }
                }
                else {
                    foreach (var renderer in Renderers) {
                        renderer.sharedMaterial = material;
                    }
                }
            }
            Destroy(this);
        }
    }
}
