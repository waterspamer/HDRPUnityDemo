using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nettle {

public class TransitionObjectDisabler : MonoBehaviour {

    private bool _disabledAtTransition = false;

    void Start() {
        VisibilityZoneViewer visibilityZoneViewer = FindObjectOfType< VisibilityZoneViewer>();
        if (visibilityZoneViewer != null) {
            visibilityZoneViewer.TransitionBegin.AddListener(OnTransitionBegin);
            visibilityZoneViewer.TransitionEnd.AddListener(OnTransitionEnd);
        }
    }


    public void OnTransitionBegin() {
        if (gameObject.activeSelf && !_disabledAtTransition) {
            gameObject.SetActive(false);
            _disabledAtTransition = true;
        }
    }

    public void OnTransitionEnd() {
        if (_disabledAtTransition) {
            gameObject.SetActive(true);
            _disabledAtTransition = false;
        }
    }
}
}
