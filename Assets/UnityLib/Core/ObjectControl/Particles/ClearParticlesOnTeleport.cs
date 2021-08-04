using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nettle {
    public class ClearParticlesOnTeleport : MonoBehaviour {
    [SerializeField]
    private ParticleSystem[] _particles;
        [SerializeField]
        private float _distanceThreshold = 2;
        private Vector3 _previousPosition;

        private void Awake () {
            _previousPosition = transform.position;        
        }

        private void Update () {
            if (Vector3.Distance(_previousPosition, transform.position)>_distanceThreshold && Time.deltaTime <0.1f)
            {
                foreach (ParticleSystem ps in _particles)
                {
                    if (ps != null)
                    {
                        ps.Clear();
                    }
                }
            }
            _previousPosition = transform.position;
        }
    }
}