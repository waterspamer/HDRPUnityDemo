using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Nettle {

public class ParticleSystemState : MonoBehaviour {
    public bool SearchInChildren = false;
    public ParticleSystem PSystem;

    public bool Falloff;
    public float FalloffTime = 1.0f;

    public UnityEvent SetStateOn = new UnityEvent();
    public UnityEvent SetStateOff = new UnityEvent();

    private List<ParticleSystem> _pSystemList = new List<ParticleSystem>();


    private void SetState(List<ParticleSystem> list, bool active) {
        if (_pSystemList.Count < 1) {
            return;
        }

        foreach (var element in _pSystemList) {
            if (element == null) continue;
            if (active) {
                element.Play();
            } else {
                element.Stop();
            }
        }
    }

    public void SetState(bool active) {
        if (Falloff) {} else {
            SetState(_pSystemList, active);
        }
        if (active) {
            SetStateOn.Invoke();
        } else {
            SetStateOff.Invoke();
        }
    }

	void Awake () {
	    if (PSystem != null) {
	        _pSystemList.Add(PSystem);
        }
        if (SearchInChildren) {
            _pSystemList.AddRange(GetComponentsInChildren<ParticleSystem>(true).ToList());
        }
    }
}
}
