using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Nettle {

    public class AdditiveSceneLoader : MonoBehaviour {
        private const string _loadScreenSceneName = "LoadScreen";
        [Tooltip ("Objects that will be hidden when the scene is loaded")]
        [SerializeField]
        private GameObject[] _placeholderObjects;
        [SerializeField]
        private string _sceneName = "";
        public string SceneName {
            get {
                return _sceneName;
            }
        }

        [SerializeField]
        private string _startZoneName = "";
        private static AdditiveSceneLoader _loadedScene = null;
        public static AdditiveSceneLoader LoadedScene {
            get {
                return _loadedScene;
            }
        }

        [SerializeField]
        VisibilityManager _visibilityManager;

        public UnityEvent OnLoad;
        public UnityEvent OnUnload;

        private bool _isLoading = false;

        private void Reset () {
            _visibilityManager = SceneUtils.FindObjectIfSingle<VisibilityManager> ();

        }

        public static AdditiveSceneLoader FindLoaderBySceneName (string sceneName) {
            AdditiveSceneLoader[] loaders = FindObjectsOfType<AdditiveSceneLoader> ();
            foreach (AdditiveSceneLoader l in loaders) {
                if (l._sceneName == sceneName) {
                    return l;
                }
            }
            return null;
        }

        public void Load () {
            Load (true);
        }

        public void Load (bool showStartZone) {
            if (_loadedScene == this) {
                if (showStartZone) {
                    VisibilityZoneViewer.Instance.ShowZone (_startZoneName);
                }
            } else {
                Debug.Log ("Loading scene " + _sceneName);
                LoadScreenManager loadScreenManager = FindObjectOfType<LoadScreenManager> ();
                if (loadScreenManager != null) {
                    if (_loadedScene != null) {
                        _loadedScene.Unload ();
                    }
                    if (OnLoad != null) {
                        OnLoad.Invoke ();
                    }
                    _loadedScene = this;
                    _isLoading = true;
                    LoadScreenManager.OnSceneLoaded += FinishLoad;
                    if (showStartZone) {
                        VisibilityZoneViewer.Instance.StartupZone = _startZoneName;
                    }
                    loadScreenManager.LoadSceneAdditive (_sceneName);
                } else {
                    Debug.LogError ("Load screen manager not found");
                }
            }
        }

        private void FinishLoad () {
            StartCoroutine (FinishLoadCoroutine ());
        }

        private IEnumerator FinishLoadCoroutine () {
            //After scene is loaded
            LoadScreenManager.OnSceneLoaded -= FinishLoad;
            if (_visibilityManager != null) {
                _visibilityManager.ResetTag ();
                _visibilityManager.UpdateTargets ();
                if (_visibilityManager is VisibilityManagerStreaming) {
                    (_visibilityManager as VisibilityManagerStreaming).TxStreaming = FindObjectOfType<TexturesStreaming> ();
                }
            }
            VisibilityZoneViewer.Instance.FindZones ();
            VisibilityZoneViewer.Instance.ShowZone (VisibilityZoneViewer.Instance.StartupZone);

            foreach (GameObject obj in _placeholderObjects) {
                if (obj != null) {
                    obj.SetActive (false);
                } else {
                    Debug.LogError ("An Empty placeholder object is assigned to " + _sceneName + " AdditiveSceneLoader! Probably lost reference.");
                }
            }
            if (LanguageInfoController.Instance != null) {
                LanguageInfoController.Instance.RefreshInfoObjects ();
            }
            if (SceneLoaded.Instance != null && !SceneLoaded.Instance.TriggerOnAdditive) {
                SceneLoaded.Instance.Trigger (_sceneName);
            }
            Debug.Log ("Finished Loading " + _sceneName);
            yield return new WaitForSeconds (0.5f);
            _isLoading = false;

        }

        public static void UnloadCurrent () {
            if (_loadedScene != null) {
                if (_loadedScene.isActiveAndEnabled) {
                    _loadedScene.StartCoroutine (_loadedScene.UnloadDelayed ());
                } else {
                    _loadedScene.Unload ();
                }
            }
        }

        private IEnumerator UnloadDelayed () {
            yield return new WaitForEndOfFrame ();
            Unload ();
        }

        public void Unload () {
            if (_loadedScene == this && !_isLoading) {
                Debug.Log ("Unloading scene " + _sceneName);
                if (OnUnload != null) {
                    OnUnload.Invoke ();
                }
                _loadedScene = null;
                foreach (GameObject obj in _placeholderObjects) {
                    obj.SetActive (true);
                }
                SceneManager.UnloadSceneAsync (_sceneName);
                StartCoroutine (FinishUnload (SceneManager.GetSceneByName (_sceneName)));
            }
        }

        private IEnumerator FinishUnload (Scene scene) {
            yield return new WaitUntil (() => { return !scene.isLoaded; });
            if (SceneLoaded.Instance != null) {
                SceneLoaded.Instance.Trigger (gameObject.scene.name);
            }
            VisibilityZoneViewer.Instance.FindZones ();
            if (LanguageInfoController.Instance != null) {
                LanguageInfoController.Instance.RefreshInfoObjects ();
            }
        }

    }
}