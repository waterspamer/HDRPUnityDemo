using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ParticlesEmissionControl : MonoBehaviour
{
    [SerializeField]
    private AnimationCurve _rateRange;
    [SerializeField]
    private AnimationCurve _rateByDistanceRange;
    [SerializeField]
    private AnimationCurve _lifetimeRange;
    [SerializeField]
    [Tooltip("Will not work if changed in playmode, change Rate property instead")]
    [Range(0,1)]
    private float _normalizedRate = 0;
    [SerializeField]
    private bool _resimulateOnChange = true;
    private float _oldRate = -1;
    private ParticleSystem _particles;
    private ParticleSystem.EmissionModule _emission;
    private ParticleSystem.MainModule _particlesMain;
    private ParticleSystem Particles {
        get {
            if (_particles == null && isActiveAndEnabled) {
                _particles = GetComponent<ParticleSystem>();
                _emission = _particles.emission;
                _particlesMain = _particles.main;
            }
            return _particles;
        }
    }

    public float Rate {
        get {
            return _normalizedRate;
        }
        set {
            _normalizedRate = Mathf.Clamp01(value);
            if (_normalizedRate != _oldRate) {
                _oldRate = _normalizedRate;
                Apply();
                Debug.Log("Update");
            }
        }
    }

    private void Apply() {
        if (Particles == null) {
            return;
        }

        if (Particles != null) {
            _emission.rateOverTime = _rateRange.Evaluate(_normalizedRate);
            _emission.rateOverDistance = _rateByDistanceRange.Evaluate(_normalizedRate);
            _particlesMain.startLifetimeMultiplier = _lifetimeRange.Evaluate(_normalizedRate);
            if (_resimulateOnChange) {
                _particles.Clear();
                _particles.Simulate(_particlesMain.startLifetimeMultiplier);
                _particles.Play();
            }
        }
    }

    private void Start() {
        Apply();
    }

#if UNITY_EDITOR
    private void Update() {
        Rate = _normalizedRate;
    }
#endif
}
