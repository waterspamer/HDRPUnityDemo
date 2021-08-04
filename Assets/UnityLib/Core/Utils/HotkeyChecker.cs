using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Nettle {

public struct HotkeyInfo {
    public List<HotkeyCallback> callbacks;
    public KeyCode[] hotkeys;
    public string HotkeysList {
        get {
            string result = "";
            for (int i = 0; i < hotkeys.Length; i++) {
                result += hotkeys[i].ToString();
                if (i < hotkeys.Length - 1) {
                    result += "+";
                }
            }
            return result;
        }
    }
}

public delegate void HotkeyCallback();

[ExecuteBefore(typeof(DefaultTime))]
public class HotkeyChecker:MonoBehaviour {

    private static bool _destroying = false;
    private static HotkeyChecker _instance;
    private static HotkeyChecker Instance {
        get {
            if (_instance == null && !_destroying) {
                GameObject go = new GameObject("HotkeyChecker");
                DontDestroyOnLoad(go);
                _instance = go.AddComponent<HotkeyChecker>();
            }
            return _instance;
        }
    }
    private List<HotkeyInfo> keys = new List<HotkeyInfo>();

    public static void AddHotkey(HotkeyCallback callback, params KeyCode[] hotkeys) {
        if (callback == null) {
            return;
        }
        foreach (HotkeyInfo key in Instance.keys) {
            if (ArraysHaveSameElements(hotkeys,key.hotkeys)){
                if (!key.callbacks.Contains(callback)) {
                    key.callbacks.Add(callback);
                    Debug.Log("Binding multiple methods to key combination [" + key.HotkeysList + "]");
                }
                else {
                    Debug.LogWarning("The method " + callback.Method.Name + " is already bound to key combination [" + key.HotkeysList + "]");
                }
                return;
            }
        }
        HotkeyInfo info = new HotkeyInfo() { callbacks = new List<HotkeyCallback>() , hotkeys = new KeyCode[hotkeys.Length]};
        hotkeys.CopyTo(info.hotkeys,0);
        info.callbacks.Add(callback);        
        Instance.keys.Add(info);
    }

    public static void RemoveCallback(HotkeyCallback callback) {
        if (Instance != null) {
            HotkeyInfo[] matches = Instance.keys.Where(x => x.callbacks.Contains(callback)).ToArray();
            foreach (HotkeyInfo info in matches) {
                info.callbacks.Remove(callback);
                if (info.callbacks.Count == 0) {
                    Instance.keys.Remove(info);
                }
            }
        }
    }

    public static bool IsHotkeyPressed(params KeyCode[] hotkeys) {
        if (LoadScreenManager.LoadingInProgress) {
            return false;
        }
        bool hasUp = false;
        foreach (KeyCode keyCode in hotkeys) {
            if (Input.GetKey(keyCode)) {
                continue;
            }
            if (Input.GetKeyUp(keyCode)) {
                hasUp = true;
            } else {
                return false;
            }
        }
        return hasUp;
    }

    private static bool ArraysHaveSameElements(KeyCode[] array1, KeyCode[] array2) {
        if (array1.Length != array2.Length) {
            return false;
        }
        foreach (object obj in array1) {
            if (array2.Count(x => x.Equals(obj)) == 0) {
                return false;
            }
        }
        return true;
    }

    private void Update() {
        if (LoadScreenManager.LoadingInProgress) {
            return;
        }
        foreach (HotkeyInfo info in keys) {
            if (IsHotkeyPressed(info.hotkeys)) {
                foreach (HotkeyCallback callback in info.callbacks) {
                    if (callback != null) {
                        callback.Invoke();
                    }
                }
            }
        }
    }

    private void OnDestroy() {
        _destroying = true;
    }
}
}
