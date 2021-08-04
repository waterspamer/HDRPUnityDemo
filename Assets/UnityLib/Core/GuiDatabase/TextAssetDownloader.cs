using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nettle {

public class TextAssetDownloader : MonoBehaviour {
    public delegate void LoadedTextHandler(string text);
    public delegate void LoadedDataHandler(byte[] data);
    private bool isBusy = false;
    private static TextAssetDownloader instance = null;
    public static TextAssetDownloader Instance {
        get {
            if (instance == null) {
                GameObject go = new GameObject("TextAssetDownloader");
                DontDestroyOnLoad(go);
                instance = go.AddComponent<TextAssetDownloader>();
            }
            return instance;
        }
    }

    /// <summary>
    /// Load text asset from given URL
    /// </summary>
    /// <param name="url">URL of the text to be loaded</param>
    /// <param name="callback">Method which should be called when the text is loaded</param>
    public void Load(string url, LoadedTextHandler callback, LoadedTextHandler onError = null) {
        StartCoroutine(Download(url, callback, onError));
    }
    /// <summary>
    /// Load bytes array from given URL
    /// </summary>
    /// <param name="url">URL of the document to be loaded</param>
    /// <param name="callback">Method which should be called when the document is loaded</param>
    public void Load(string url, LoadedDataHandler callback, LoadedTextHandler onError = null) {
        StartCoroutine(Download(url, callback, onError));
    }

    private IEnumerator Download(string url, LoadedTextHandler callback, LoadedTextHandler onError) {
        isBusy = true;
        WWW www = new WWW(url);
        yield return www;
        if (!string.IsNullOrEmpty(www.error)) {
            Debug.LogError("Error loading text asset: " + www.error);
            if (onError != null) {
                onError(www.error);
            }
        }
        else {
            if (callback != null) {
                callback(www.text);
            }
        }
    }
    private IEnumerator Download(string url, LoadedDataHandler callback, LoadedTextHandler onError) {
        isBusy = true;
        WWW www = new WWW(url);
        yield return www;
        if (!string.IsNullOrEmpty(www.error)) {
            Debug.LogError("Error loading document: " + www.error);
            if (onError != null) {
                onError(www.error);
            }
        }
        else {
            if (callback != null) {
                callback(www.bytes);
            }
        }
    }
}
}
