using UnityEngine;

namespace Nettle {

public class NEventScroll : NEvent {

	public float TriggerValue = 0.01f;
	protected override bool Get(){
		
		if (Mathf.Abs(TriggerValue)<Mathf.Epsilon) return false;
		float Zoom = Input.GetAxis("Mouse ScrollWheel");

		if ((Zoom/TriggerValue)>1) return true;
		
		return false;
	}
}
}
