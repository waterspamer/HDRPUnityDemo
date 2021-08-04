using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class EditorPause : MonoBehaviour
{
    public void Pause() {
#if UNITY_EDITOR
        EditorApplication.isPaused = true;
#endif
    }
}
