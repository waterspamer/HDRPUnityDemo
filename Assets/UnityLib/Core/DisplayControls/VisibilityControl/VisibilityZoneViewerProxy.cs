using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Nettle {

public class VisibilityZoneViewerProxy : MonoBehaviour {

    private string _zone = "";
    private const string _loadScreenSceneName = "LoadScreen";

    public void SetZone(string zoneName) {
        _zone = zoneName;
    }

    private void Awake() {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        if (scene.name == _loadScreenSceneName || string.IsNullOrEmpty(_zone)) { return; }

        var viewer = SceneUtils.FindObjectIfSingle<VisibilityZoneViewer>();
        if (viewer != null) {
            //viewer.ShowZone(_zone);   
            viewer.StartupZone = _zone;
        }

        _zone = "";
    }
}
}
