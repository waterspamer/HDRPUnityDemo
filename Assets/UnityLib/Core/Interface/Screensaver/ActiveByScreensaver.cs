using UnityEngine;
using System.Collections;

namespace Nettle {

public class ActiveByScreensaver : MonoBehaviour {

    public GameObject Target;

    private DeprecatedScreenSaver _screensaver;
    private bool _targetWasActive = false; 

    private void Start() {
        _screensaver = FindObjectOfType<DeprecatedScreenSaver>();
    }

    private void Update() {
        if (_screensaver == null || Target == null) { return; }

        if (_screensaver.Active && Target.activeSelf) {
            Target.SetActive(false);
            _targetWasActive = true;
        }

        if (!_screensaver.Active && !Target.activeSelf && _targetWasActive) {
            Target.SetActive(true);
            _targetWasActive = false;
        }

        /*if (Target.activeInHierarchy == _screensaver.Active) {
            Target.SetActive(!_screensaver.Active);
        }*/
    }
}
}
