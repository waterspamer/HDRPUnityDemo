using System;
using UnityEngine;

namespace Nettle.Core {

    [RequireComponent(typeof(Renderer))]
    public class RendererVisibilityDispatcher : MonoBehaviour {

        public Action BecameVisible;
        public Action BecameInvisible;

        void OnBecameVisible() {
            if (BecameVisible != null) {
                BecameVisible.Invoke();
            }
        }

        void OnBecameInvisible() {
            if (BecameVisible != null) {
                BecameInvisible.Invoke();
            }
        }

    }
}
