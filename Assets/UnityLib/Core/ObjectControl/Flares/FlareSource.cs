using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Nettle {
        public class FlareSource : MonoBehaviour {
        public Color FlareColor = Color.white;
        public float Intensity = 1;
        public float Scale = 0.1f; 

        private void OnEnable() {
            if (FlareRenderer.Instance != null) {
                FlareRenderer.Instance.RegisterFlare(this);
            }
        }

        private void OnDisable() {
            if (FlareRenderer.Instance != null) {
                FlareRenderer.Instance.UnregisterFlare(this);
            }
        }

        private void OnDrawGizmos() {
            if (enabled) {
                Gizmos.color = FlareColor;
                Gizmos.DrawWireSphere(transform.position, 0.2f);
                Gizmos.color = Color.white;
            }
        }
    }
}
