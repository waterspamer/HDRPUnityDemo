using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nettle {

public class LoadingScreenCanvas : MonoBehaviour {
    [SerializeField]
    private StereoEyes _eyes;
    [SerializeField]
    private RectTransform _progressBarFill;
    private LoadScreenManager _loadScreenManager;

    private float _smoothProgress = 0f;

    private void Reset() {
        _eyes = SceneUtils.FindObjectIfSingle<StereoEyes>();
    }

    private void Start () {
        _loadScreenManager = FindObjectOfType<LoadScreenManager>();
        _eyes.BeforeRenderEvent += UpdateScreen;
    }

    private void UpdateScreen() {
        //Update progress bar for one eye only, so that both eyes have progressbar of the same length
        if (!_eyes || _eyes.LeftEyeActive) {
            if (_loadScreenManager) {
                _smoothProgress = Mathf.Lerp(_smoothProgress, _loadScreenManager.LoadProgress, Time.deltaTime * 2f);
            }
            else {
                _smoothProgress = 0.5f;
            }
        }
        _progressBarFill.anchorMax = new Vector2(_smoothProgress, 1);
    }
}
}
