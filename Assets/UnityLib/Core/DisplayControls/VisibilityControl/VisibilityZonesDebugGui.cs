using UnityEngine;

namespace Nettle {

public class VisibilityZonesDebugGui : MonoBehaviour{
#if UNITY_EDITOR
    private VisibilityZone[] _zones;
    private Vector2 scrollPosition = Vector2.zero;
    public VisibilityZoneViewer viewer;

    void Reset()
    {
         SceneUtils.FindObjectIfSingle(ref viewer);
    }

    void Start(){
        _zones = (VisibilityZone[])FindObjectsOfType(typeof(VisibilityZone));
    }

    void OnGUI() {
        GUILayout.BeginArea(new UnityEngine.Rect(0, 0, 200f, (float)Screen.height));
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);
        foreach (var zone in _zones){
            if (GUILayout.Button(zone.gameObject.name)){
                if (viewer != null)
                {
                    viewer.ShowZone(zone.gameObject.name);
                }
                else
                {
                    Debug.LogError("Visibility zone viewer is not assigned");
                }
            }
        }
        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }
#endif
}
}
