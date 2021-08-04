using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Nettle {

[CanEditMultipleObjects]
[CustomEditor(typeof(MotionParallaxLOD))]
public class MotionParallaxLODEditor : Editor {
    MotionParallaxLOD _target;

    private void Awake() {
        _target = target as MotionParallaxLOD;    
    }

    public override void OnInspectorGUI() {
        if (GUILayout.Button("Find LOD objects")) {
            Undo.RecordObject(_target, "Find LOD objects");
            FindLods();
        }
        if (Application.isPlaying && _target.LODs.Length>0) {
            GUILayout.Label("Current LOD level is " + _target.CurrentID);
            _target.OverrideLevel = EditorGUILayout.IntSlider("Override level" ,_target.OverrideLevel, -1, _target.LODs.Length - 1);
            Repaint();
        }
        base.OnInspectorGUI();
    }

    private void FindLods() {
        if (_target.LODs == null || _target.LODs.Length==0) {
            Debug.LogError("No LOD levels are initialized. You need to set up all required LOD levels first.");
            return;
        }
        List<GameObject>[] levels = new List<GameObject>[_target.LODs.Length];
        for (int i = 0; i < levels.Length; i++) {
            levels[i] = new List<GameObject>();
        }
        Transform[] children = _target.GetComponentsInChildren<Transform>();
        Regex regex = new Regex(@"_LOD(\d+)");
        foreach (Transform child in children) {
            if (child == _target.transform) {
                continue;
            }
            Match match = regex.Match(child.gameObject.name);
            int levelId = 0;
            if (match.Success) {
                int.TryParse(match.Groups[1].Value, out levelId);
                if (levelId >= levels.Length) {
                    Debug.LogWarning("LOD level #" + levelId + " is not set up. Can't add object " + child.gameObject.name);
                    continue;
                }
            }
            levels[levelId].Add(child.gameObject);
        }
        for (int i = 0; i < levels.Length; i++) {
            _target.LODs[i].GameObjects = levels[i].ToArray();
        }
    }
}
}
