using UnityEngine;

namespace Nettle {

public class ZoompanDisabler : MonoBehaviour {
	
	public NEvent[] predicates;
	public ZoomPan Component;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		
		if (predicates != null) {
			bool disable =false;
			foreach (var p in predicates) {
				if ((bool)p){
					disable = true;
					break;
				}
			}

			Component.enabled = !disable;

			//renderer.enabled = !Hide;
		}
	}
}
}
