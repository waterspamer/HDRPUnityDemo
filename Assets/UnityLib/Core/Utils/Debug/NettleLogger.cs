#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Nettle {


#if UNITY_EDITOR
[InitializeOnLoad]
#endif
public class NettleLogger {

#if UNITY_2017_1_OR_NEWER
    static NettleLogger() {
        Activate();
    }

    private static NettleLogHandler _nettleLoggerHandler;
    private static string _timeFormat = "HH:mm:ss.fff";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void OnBeforeSceneLoadRuntimeMethod() {
        Activate();
    }

    public static void Activate() {
        if (_nettleLoggerHandler == null || Debug.unityLogger.logHandler != _nettleLoggerHandler) {
            if (_nettleLoggerHandler == null) {
                _nettleLoggerHandler = new NettleLogHandler(_timeFormat);
            }
            Debug.unityLogger.logHandler = _nettleLoggerHandler;
        }
    }

#endif

}

}
