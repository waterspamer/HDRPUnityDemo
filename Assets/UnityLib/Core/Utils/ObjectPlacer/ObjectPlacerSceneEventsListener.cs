using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[ExecuteInEditMode]
public class ObjectPlacerSceneEventsListener : MonoBehaviour {

    public Action<Vector2> MouseDown;
    public Action<Vector2> MouseDrag;
    public Action<Vector2> MouseUp;

    private bool _leftBtnDown = false;

    void Awake() {

#if UNITY_EDITOR
        SceneView.onSceneGUIDelegate += OnSceneGuiDelegate;
#endif        
    }

#if UNITY_EDITOR
    private void OnSceneGuiDelegate(SceneView sceneView) {
        if (Event.current.type == EventType.MouseDown && Event.current.button == 0) {
            _leftBtnDown = true;
            MouseDown.Invoke(Event.current.mousePosition);
        }

        if (Event.current.type == EventType.Used && Event.current.button == 0 && Event.current.delta == Vector2.zero) {
            _leftBtnDown = false;
            MouseUp.Invoke(Event.current.mousePosition);
        }

        if (Event.current.type == EventType.Used && _leftBtnDown && Event.current.delta != Vector2.zero) {
            MouseDrag.Invoke(Event.current.mousePosition);
        }
    }
#endif


    void OnDestroy() {
#if UNITY_EDITOR
        SceneView.onSceneGUIDelegate -= OnSceneGuiDelegate;
#endif
    }

}
