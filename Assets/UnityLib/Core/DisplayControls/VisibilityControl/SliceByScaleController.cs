using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nettle {

public class SliceByScaleController : MonoBehaviour {
    [System.Serializable]
    public class SliceLevel {
        public float MaxScale;
        public string VisibilityTag;
    }

    [SerializeField]
    private VisibilityManager _visibilityManager;
    [SerializeField]
    private MotionParallaxDisplay _display;
    [SerializeField]
    private SliceLevel[] slices;
    private int _currentId = -2;
    [SerializeField]
    private string _defaultTag = "No_Slice";

    private void Reset() {
        _visibilityManager = SceneUtils.FindObjectIfSingle<VisibilityManager>();
        _display = SceneUtils.FindObjectIfSingle<MotionParallaxDisplay>();
    }

    // Update is called once per frame
    void Update() {
        if (_display == null || _visibilityManager == null || slices.Length == 0) {
            return;
        }
        float scale = _display.transform.localScale.x;
        float minPixelsPerUnit = Mathf.Infinity;

        int currentID = -1;
        for (int i = 0; i < slices.Length; i++) {
            if (slices[i].MaxScale > scale) {
                if (slices[i].MaxScale < minPixelsPerUnit) {
                    minPixelsPerUnit = slices[i].MaxScale;
                    currentID = i;
                }
            }
        }
        if (_currentId != currentID) {
            _currentId = currentID;
            if (_currentId >= 0) {
                _visibilityManager.BeginSwitch(slices[currentID].VisibilityTag);
            }
            else {
                _visibilityManager.BeginSwitch(_defaultTag); 
            }
        }
    }
}
}
