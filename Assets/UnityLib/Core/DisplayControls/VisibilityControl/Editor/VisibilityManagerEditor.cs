using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Nettle
{

    [CustomEditor(typeof(VisibilityManager), true)]
    public class VisibilityManagerEditor : Editor
    {
        private VisibilityManager _manager;
        private int _currentId = 0;
        private void OnEnable()
        {
            _manager = target as VisibilityManager;
            _manager.OnBeginSwitch += UpdateId;
            if (!Application.isPlaying)
            {
                _manager.UpdateTargets();
                UpdateId(_manager.CurrentTag);
            }
        }

        private void OnDisable()
        {
            _manager.OnBeginSwitch -= UpdateId;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUI.BeginChangeCheck();
            _manager.EditorModeSlice = EditorGUILayout.Toggle("Slice in edit mode", _manager.EditorModeSlice);
            if (Application.isPlaying || _manager.EditorModeSlice)
            {
                _currentId = EditorGUILayout.Popup("Current tag", _currentId, _manager.AllTags.ToArray());
            }
            if (EditorGUI.EndChangeCheck())
            {
                _manager.UpdateTargets();
                if (_currentId >= 0 && _manager.AllTags.Count > _currentId)
                {
                    _manager.BeginSwitch(_manager.AllTags[_currentId], true);
                    _manager.LateUpdate();
                }
            }
        }

        private void UpdateId(string tag)
        {
            _currentId = _manager.AllTags.FindIndex(x => x == tag);
            Repaint();
        }

    }
}