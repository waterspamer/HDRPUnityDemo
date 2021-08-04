using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System.Linq;

namespace Nettle
{
    [RequireComponent(typeof(AudioSource))]
    public class BackgroundMusicPlayer : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Specify filename without extension. The script will search for the file with given name in the StreamingAssets folder. Note that MP3 loading is not supported by Unity on desktop platforms.")]
        private string _musicFileName = "Music";
        [SerializeField]
        private bool _dontDestroy = true;
        [SerializeField]
        private bool _isPlaying = false;
        private AudioSource _audioSource;

        private void Awake()
        {
            
            if (_dontDestroy)
            {
                if (FindObjectsOfType<BackgroundMusicPlayer>().Length > 1)
                {
                    Destroy(gameObject);
                    return;
                }
                DontDestroyOnLoad(gameObject);
            }
            _audioSource = GetComponent<AudioSource>();
            StartCoroutine(LoadAudio());
        }

        private IEnumerator LoadAudio()
        {
            string[] allFiles = Directory.GetFiles(Application.streamingAssetsPath);
            string filePath;
            try
            {
                filePath = allFiles.Where(x => Path.GetFileNameWithoutExtension(x).Equals(_musicFileName)).First();
            }
            catch
            {
                Debug.LogError("Could not find background music file");
                yield break;
            }
            Debug.Log("Trying to load background music from path " + filePath);
            using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip("file:///" + filePath, AudioType.UNKNOWN))
            {
                yield return request.SendWebRequest();
                if (!(request.isNetworkError || request.isHttpError))
                {
                    _audioSource.clip = DownloadHandlerAudioClip.GetContent(request);
                    if (_isPlaying)
                    {
                        _audioSource.Play();
                    }
                }
                else
                {
                    Debug.LogError("Could not load background music");
                }
            }
        }

        public void Play()
        {
            _audioSource.Play();
            _isPlaying = true;
        }
        public void Stop()
        {
            _audioSource.Stop();
            _isPlaying = false;
        }
    }
}