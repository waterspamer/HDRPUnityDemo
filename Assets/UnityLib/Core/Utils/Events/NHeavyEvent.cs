using UnityEngine;

namespace Nettle {

public class NHeavyEvent : NEvent {

	private int Frame = -1;
	private bool State = false;
	
	protected override bool Get(){
		if (Time.frameCount!=Frame) State = UpdateValue();
		Frame = Time.frameCount;
		return State;
	}
	
	protected virtual bool UpdateValue(){
		return false;
	}
	
}
}
