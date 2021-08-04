using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Nettle {

[RequireComponent(typeof(LODGroup))]
public class MotionParallaxForceLOD: MonoBehaviour {
    public StereoEyes StereoEyes;
    protected LODGroup _lodGroup;
    protected int _currentLODLevel;

    protected virtual void Reset() {
        if (!StereoEyes) {
            StereoEyes = FindObjectOfType<StereoEyes>();
        }
    }


    void Awake() {
        _lodGroup = GetComponent<LODGroup>();
        for (int i = 0; i < _lodGroup.GetLODs().Length; i++) {
            foreach (var lodRenderer in _lodGroup.GetLODs()[i].renderers) {
                MotionParallaxLODLevel motionParallaxLodLevel = lodRenderer.gameObject.AddComponent<MotionParallaxLODLevel>();
                motionParallaxLodLevel.VisibleEvent += OnLODVisible;
                motionParallaxLodLevel.InvisibleEvent += OnLODInvisible;
                motionParallaxLodLevel.LodLevel = i;
            }
        }
        List<LOD> lodList = _lodGroup.GetLODs().ToList();
        lodList.Add(new LOD());
        _lodGroup.SetLODs(lodList.ToArray());
    }

    void OnEnable() {
        StereoEyes.BeforeRenderEvent += OnBeforeRenderEvent;

    }

    void OnDisable() {
        StereoEyes.BeforeRenderEvent -= OnBeforeRenderEvent;

    }

    void OnLODVisible(int lodLevel) {
        if (!StereoEyes.LeftEyeActive) {
            _currentLODLevel = lodLevel;
        }
    }

    void OnLODInvisible(int lodLevel) {
        if (StereoEyes.LeftEyeActive) {
            if (_lodGroup.lodCount - 2 == lodLevel || lodLevel == _currentLODLevel) {
                _currentLODLevel = _lodGroup.lodCount - 1;
            }
        }
    }

    void OnBeforeRenderEvent() {
        if (StereoEyes.LeftEyeActive) {
            _lodGroup.ForceLOD(_currentLODLevel);
            OnForceLOD();
        } else {
            _lodGroup.ForceLOD(-1);
        }
    }

    protected virtual void OnForceLOD() {
    }

}
}
