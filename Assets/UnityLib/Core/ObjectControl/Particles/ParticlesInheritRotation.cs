using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nettle
{
    [ExecuteInEditMode]
    public class ParticlesInheritRotation : MonoBehaviour
    {
        public Vector3 AdditionalRotation = Vector3.zero;
        private ParticleSystem _particles;
        private ParticleSystem.MainModule _particlesMain;

        private bool _initialized = false;

        void Update()
        {
            if (!_initialized)
            {
                Initialize();
            }
            try
            {
                Vector3 rotation = transform.rotation.eulerAngles;
                _particlesMain.startRotationX = (Mathf.Repeat(rotation.x + AdditionalRotation.x, 360)-180) * Mathf.Deg2Rad;
                _particlesMain.startRotationY = (Mathf.Repeat(rotation.y + AdditionalRotation.y, 360)-180) * Mathf.Deg2Rad;
                _particlesMain.startRotationZ = (Mathf.Repeat(rotation.z + +AdditionalRotation.z, 360)-180) * Mathf.Deg2Rad;
            }
            catch
            {
                Initialize();
            }            
        }

        private void Initialize()
        {
            _particles = GetComponent<ParticleSystem>();
            if (_particles != null)
            {
                _particlesMain = _particles.main;
            }
            else
            {
                return;
            }
            _initialized = true;
        }
    }
}