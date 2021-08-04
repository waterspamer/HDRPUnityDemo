using UnityEngine;

namespace Nettle {

public class AnimationSpeedChanger : MonoBehaviour {

	public float AnimationSpeed = 1.0f;
	public string SourceAnimation = "Take 001";

	void Start() {
		GetComponent<Animation>()[SourceAnimation].speed = AnimationSpeed;
	}
}
}
