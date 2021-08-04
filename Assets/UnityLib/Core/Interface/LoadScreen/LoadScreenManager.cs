using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Nettle {

    public class LoadScreenManager : MonoBehaviour {

        public int LoadSceneId = 1;
        public event Action<string, LoadSceneMode> StartLoadScreenEvent;
        public event Action<string, LoadSceneMode> EndLoadScreenEvent;

        public static string LastLoadedScene { get; private set; }
        public static bool LoadingInProgress { get; private set; }

        public static event Action OnSceneLoaded;

        [HideInInspector]
        public float LoadProgress;
        public string SceneToLoadName = "";

        private AsyncOperation _loadAsync;
        private string lastLoadedSceneName = "";



        private void Start() {
            LoadingInProgress = false;
            DontDestroyOnLoad(gameObject);
            LoadScene(SceneToLoadName);
        }


        public void LoadScene(int sceneId) {
            if (sceneId >= SceneManager.sceneCount) {
                Debug.LogErrorFormat("LoadScreenManagerInfo: sceneId {0} is out of range, sceneCount: {1}", sceneId, SceneManager.sceneCount);
                return;
            }
            LoadScene(SceneManager.GetSceneAt(sceneId).name);
        }

        public void LoadScene(string sceneName) {
            if (String.IsNullOrEmpty(sceneName) || lastLoadedSceneName.Equals(sceneName)) {
                return;
            }
            lastLoadedSceneName = sceneName;
            StartCoroutine(Loading(sceneName, LoadSceneMode.Single));
        }


        public void LoadSceneAdditive(int sceneId) {
            StartCoroutine(Loading(SceneManager.GetSceneByBuildIndex(sceneId).name, LoadSceneMode.Additive));
        }

        public void LoadSceneAdditive(string sceneName) {
            StartCoroutine(Loading(sceneName, LoadSceneMode.Additive));
        }
        private void OnGUI() {
            LoadProgress = _loadAsync == null ? 0.0f : _loadAsync.progress;
        }

        public string GetLastLoadedSceneName() {
            return lastLoadedSceneName;
        }

        private IEnumerator Loading(string sceneName, LoadSceneMode mode) {
            if (!Application.CanStreamedLevelBeLoaded(sceneName)) {
                throw new Exception("Scene " + sceneName + " doesn't exist");
            }
            LoadingInProgress = true;
            LastLoadedScene = sceneName;
            if (StartLoadScreenEvent != null) {
                StartLoadScreenEvent.Invoke(sceneName, mode);
            }
            MotionParallaxDisplay mpd = FindObjectOfType<MotionParallaxDisplay>();

            GameObject display = null;

            if (mpd != null) {
                display = mpd.gameObject;
                display.SetActive(false);
            }
            //Load loading screen
            AsyncOperation ls = SceneManager.LoadSceneAsync(LoadSceneId, mode);
            yield return new WaitUntil(() => { return ls.isDone; });
            Resources.UnloadUnusedAssets();
            var loadScreen = FindObjectOfType<LoadScreen>();
            if (loadScreen) {
                loadScreen.LoadScrManager = this;
                if (EndLoadScreenEvent != null) {
                    EndLoadScreenEvent.Invoke(sceneName, mode);
                }
            }
            //Load the scene
            _loadAsync = SceneManager.LoadSceneAsync(sceneName, mode);
            yield return new WaitUntil(() => { return _loadAsync.isDone; });
            if (OnSceneLoaded != null) {
                OnSceneLoaded.Invoke();
            }
            if (mode == LoadSceneMode.Additive) {
                SceneManager.UnloadSceneAsync(LoadSceneId);
            }
            yield return null;
            if (display != null) {
                display.SetActive(true);
            }
            LoadingInProgress = false;
        }

        private void Reset() {
            Debug.LogError("Check scene to load Name");
        }


    }
}
