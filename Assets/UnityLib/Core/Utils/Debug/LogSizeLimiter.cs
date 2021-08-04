using System;
using System.Collections;
using System.IO;
using UnityEngine;

public class LogSizeLimiter : MonoBehaviour {


    public float CheckInterval = 9.99f;
    public float MaxSizeInMB = 1024f;

    

    void Start() {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        string logPath = Path.Combine(Environment.GetEnvironmentVariable("AppData"), "..", "LocalLow", Application.companyName, Application.productName, "Player.log");
        StartCoroutine(CheckLogSizeCoroutine(logPath));
#endif
    }

    IEnumerator CheckLogSizeCoroutine(string logPath) {
        while (true) {
            yield return new WaitForSeconds(CheckInterval);
            FileInfo info = new FileInfo(logPath);
            float size = (float)info.Length / (1024f * 1024f);
            if (size > MaxSizeInMB) {
                Debug.Log("Logs will be disabled cause log file size(" + size + ") > MaxSizeInMB(" + MaxSizeInMB + ").");
                Debug.unityLogger.logEnabled = false;
            }
        }
    }
}
