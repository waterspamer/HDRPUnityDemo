using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nettle.Core {
    public class LandscapeBlock : MonoBehaviour {
        public float Length = 100;
        public bool Extendable = true;

        private void OnDrawGizmosSelected() {
            Vector3 start = transform.position + transform.forward * Length / 2;
            Vector3 end = transform.position - transform.forward * Length / 2;
            Gizmos.DrawLine(start, end);
            Gizmos.DrawLine(start - transform.right, start + transform.right);
            Gizmos.DrawLine(end - transform.right, end + transform.right);
        }
    }
}
