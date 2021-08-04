using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Nettle {

public class SceneLoaded : MonoBehaviour {
    [Serializable]
    public class StringEvent : UnityEvent<string> {}
    public bool TriggerOnAdditive = false;
    public StringEvent Event = new StringEvent();

    private static SceneLoaded _instance;
    public static SceneLoaded Instance {
        get {
            if (_instance == null) {
                _instance = FindObjectOfType<SceneLoaded>();
            }
            return _instance;
        }
    }

    private void Awake() {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        if (mode != LoadSceneMode.Additive || TriggerOnAdditive) {
            Trigger(scene.name);
        }
    }

    public void Trigger(string sceneName) {
        Event.Invoke(sceneName);
    } 

}
}
