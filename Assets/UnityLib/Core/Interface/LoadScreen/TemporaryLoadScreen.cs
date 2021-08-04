using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nettle {

public class TemporaryLoadScreen : MonoBehaviour {
    [SerializeField]
    private GameObject _loadScreenObject;

    private void Reset() {
        _loadScreenObject = gameObject;
    }      

    private void Awake () {
        if (_loadScreenObject == null) {
            _loadScreenObject = gameObject;
        }
        else {
            _loadScreenObject.SetActive(false);
        }
	}

    public void Show() {
            _loadScreenObject.SetActive(true);
    }

    private void DisplayChanged(Vector3 position, Quaternion rotation, Vector3 scale) {
        _loadScreenObject.SetActive(false);
    }
}
}
