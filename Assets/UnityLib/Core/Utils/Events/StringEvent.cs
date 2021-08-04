using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class StringEvent : MonoBehaviour {

    [System.Serializable]
    public class StringEventObject:UnityEvent<string> {
    }

    public StringEventObject Event;
    public string CurrentParameter { get; set; }

    public void Invoke(string parameter) {
        Event.Invoke(parameter);
    }

    public void Repeat() {
        Event.Invoke(CurrentParameter);
    }
}
