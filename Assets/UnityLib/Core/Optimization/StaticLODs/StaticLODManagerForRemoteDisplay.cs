using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Nettle;
using Nettle.Core;
using UnityEngine;

[RequireComponent(typeof(StaticLODManager))]
public class StaticLODManagerForRemoteDisplay : MonoBehaviour {

    public Transform MainCamera;
    private StaticLODManager _staticLODManager;
    private List<GameObject> _tiles = new List<GameObject>();
    private List<List<GameObject>> _lods = new List<List<GameObject>>();
    private float[] _displaySqrScales;


    void OnValidate() {
        EditorInit();
    }

    void Reset() {
        EditorInit();
    }

    void EditorInit() {
        if (!MainCamera) {
            StereoEyes stereoEyes = FindObjectOfType<StereoEyes>();
            if (stereoEyes) {
                MainCamera = stereoEyes.GetComponentInChildren<Camera>()?.transform;
            }
        }

    }

    void Start() {

        Stopwatch stopwatch = Stopwatch.StartNew();
        _staticLODManager = GetComponent<StaticLODManager>();
        GameObject LOD0 = _staticLODManager.LODs[0];
        int tileCount = LOD0.transform.childCount;

        for (var i = 0; i < tileCount; i++) {
            string tileName = LOD0.transform.GetChild(i).name;
            GameObject newTile = new GameObject(tileName);
            newTile.transform.parent = transform;
            _tiles.Add(newTile);
        }
        for (var tileIndex = 0; tileIndex < _tiles.Count; tileIndex++) {
            _lods.Add(new List<GameObject>());
            for (var lodIndex = 0; lodIndex < _staticLODManager.LODs.Count; lodIndex++) {
                Transform tileLOD = _staticLODManager.LODs[lodIndex].transform.GetChild(0);
                tileLOD.parent = _tiles[tileIndex].transform;
                tileLOD.name = "LOD" + lodIndex;    
                _lods.Last().Add(tileLOD.gameObject);
            }
        }
        _displaySqrScales = new float[_staticLODManager.DisplayScales.Length + 2];
        _displaySqrScales[0] = 0;
        _displaySqrScales[_displaySqrScales.Length - 1] = float.MaxValue;
        for (var i = 1; i < _displaySqrScales.Length - 1; i++) {
            if (Math.Abs(_staticLODManager.DisplayScales[i - 1]) < 0.0001f && _displaySqrScales[i - 1] > 0) {
                _displaySqrScales[i] = float.MaxValue;
            } else {
                _displaySqrScales[i] = _staticLODManager.DisplayScales[i - 1] * _staticLODManager.DisplayScales[i - 1];
            }
        }
        stopwatch.Stop();
        UnityEngine.Debug.Log("StaticLODManagerForRemoteDisplay::Genereta LOD list in " + stopwatch.ElapsedMilliseconds + "ms");
    }

    // Update is called once per frame
    void Update() {
        foreach (List<GameObject> lod in _lods) {
            float sqrDistance = Vector3.SqrMagnitude(lod[0].transform.position - MainCamera.position);

            for (int i = 0; i < lod.Count; i++) {
                bool enabled = _displaySqrScales[i] < sqrDistance && sqrDistance < _displaySqrScales[i + 1];
                lod[i].SetActive(enabled);
            }
        }
    }
}
