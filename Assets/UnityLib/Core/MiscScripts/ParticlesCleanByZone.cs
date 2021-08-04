using Nettle;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ParticlesCleanByZone : MonoBehaviour {
    public bool IncludeChildren = true;
    public bool OnlyOnFastSwitch = true;
    public bool ClearOnEndOfSecondFrame = true;
    public bool InvertVisibilityZone = false;
    [Tooltip("Clear on zones. If empty, clear always.")]
    public List<VisibilityZone> VisibilityZones;

    private List<ParticleSystem> _particlesList;

    private void Start() {
        VisibilityZoneViewer.Instance.OnShowZone.AddListener(OnShowZone);

        if (IncludeChildren) {
            _particlesList = GetComponentsInChildren<ParticleSystem>(true).ToList();
        } else {
            _particlesList = new List<ParticleSystem>();
            var particles = GetComponentInChildren<ParticleSystem>(true);
            if (particles) {
                _particlesList.Add(particles);
            }
        }
    }

    private void OnDestroy() {
        VisibilityZoneViewer.Instance.OnShowZone.RemoveListener(OnShowZone);

    }

    private void OnShowZone(VisibilityZone zone) {        
        if (!gameObject.activeInHierarchy || 
            (VisibilityZones.Count > 0 && 
            ((!InvertVisibilityZone && !VisibilityZones.Contains(zone)) || (InvertVisibilityZone && VisibilityZones.Contains(zone))))
            ) {
            return;
        }

        if (zone.FastSwitchFromPrevious() || !OnlyOnFastSwitch) {
            if (ClearOnEndOfSecondFrame) {
                StartCoroutine(ClearEndOfFrame());
            } else {
                Clear();
            }
        }
    }

    private IEnumerator ClearEndOfFrame() {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        Clear();
    }

    private void Clear() {
        foreach (var particles in _particlesList) {
            particles.Clear(true);
        }
    }
}
