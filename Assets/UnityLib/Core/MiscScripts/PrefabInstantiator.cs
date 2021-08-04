using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabInstantiator : MonoBehaviour {
    [SerializeField]
    private Transform _newParent;
    [SerializeField]
    private Transform _prefab;
    [SerializeField]
    private bool _spawnOnEnable = false;
    [SerializeField]
    private bool _destroyOnDisable = false;
    [SerializeField]
    private bool _keepPrefabRotation = true;

    private Transform _spawned;

    private void Reset() {
        _newParent = transform;
    }

    public void InstantiatePrefab() {
        if (_newParent != null) {
            _spawned = Instantiate(_prefab, _newParent);
            if (!_keepPrefabRotation) {
                _spawned.localRotation = Quaternion.identity;
            }
        } else {
            _spawned = Instantiate(_prefab, transform.position, _keepPrefabRotation ? _prefab.transform.rotation : transform.rotation);
        }
    }

    public void DestroyPrefab() {
        if (_spawned) {
            Destroy(_spawned.gameObject);
        }
    }

    private void OnEnable() {
        if (_spawnOnEnable) {
            InstantiatePrefab();
        }
    }

    private void OnDisable() {
        if (_destroyOnDisable) {
            DestroyPrefab();
        }
    }
}