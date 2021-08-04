using UnityEngine;
using System.Collections;

namespace Nettle {

public class StereoEyesFlipController : MonoBehaviour {
    public KeyCode flipKey = KeyCode.S;
    public StereoEyes eyes;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyUp(flipKey) && eyes != null) {
            eyes.FlipEyes = !eyes.FlipEyes;
        }
	}
}
}
