using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nettle {
    public class ZoneSwitchParticleClear : MonoBehaviour {
        void Start() {
            VisibilityZoneViewer.Instance.OnShowZone.AddListener(OnShowZone);
        }

        private void OnShowZone(VisibilityZone zone) {
            if (zone.FastSwitchFromPrevious()) {
                StartCoroutine(DelayedClear());
            }
        }

        private IEnumerator DelayedClear() {
            for (int i = 0; i < 3; i++) {
                ParticleSystem[] particles = FindObjectsOfType<ParticleSystem>();
                foreach (ParticleSystem p in particles) {
                    p.Clear();
                }
                yield return null;
            }
        }
    }
}