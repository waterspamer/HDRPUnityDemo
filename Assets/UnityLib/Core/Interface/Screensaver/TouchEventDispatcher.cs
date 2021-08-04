using UnityEngine;
using UnityEngine.Events;

namespace Nettle {

public class TouchEventDispatcher : MonoBehaviour {

    public UnityEvent TouchEvent;

	void Update () {
	    if (Input.touchCount > 0) {
	        TouchEvent.Invoke();
        }
	}
}
}
