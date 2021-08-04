using UnityEngine;

namespace Nettle {

public class DebugVisibilityZones : MonoBehaviour {
	#if UNITY_EDITOR
	public VisibilityZone[] zones;
	public bool ShowZones = false;

	public VisibilityManager manager;

	//private Vector2 scrollPosition = Vector2.zero;

    void Reset()
    {
        manager = SceneUtils.FindObjectIfSingle<VisibilityManager>();
    }

    void OnGUI() {

		/*if (ShowZones) {
			GUILayout.BeginArea(new Rect(0, 0, 200f, (float)Screen.height));
			scrollPosition = GUILayout.BeginScrollView(scrollPosition);
			foreach (var zone in zones) {
				if (GUILayout.Button(zone.gameObject.name)) {
					if (manager != null) {
						manager.SwitchZone(zone.gameObject.name);
					}
				}
			}
			GUILayout.EndScrollView();
			GUILayout.EndArea();
		}*/

	}
	#endif
}

}
