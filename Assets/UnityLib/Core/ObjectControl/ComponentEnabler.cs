using System.Linq;
using UnityEngine;

namespace Nettle {

public class ComponentEnabler : MonoBehaviour {
	public MonoBehaviour Comp;

	public NEvent[] EnableEvent;
    public NEvent[] DisableEvent;

	void Update () {
	    if (Comp == null) {
	        return;
	    }
		if (EnableEvent != null && EnableEvent.Any(v=>v)) {
            Comp.enabled = true;
		} else if (DisableEvent != null && DisableEvent.Any(v => v)) {
            Comp.enabled = false;
        }
	}
}
}
