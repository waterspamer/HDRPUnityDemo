using UnityEngine;

namespace Nettle {

public class NEventChangeScale : NEvent {
	
	
	public GameObject source;
	public float targetScale;
	public enum Operations {More, Less};
	public Operations Operation = Operations.More;
	bool result = false;
	
		// Use this for initialization
	void Start () {
	
	}

	protected override bool Get(){
		result = (targetScale > source.transform.localScale.x);
		if (Operation == Operations.More) {
			return result;
		}else{
			return !result;
		}
	}
}
}
