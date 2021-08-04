using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

namespace Nettle {

[Serializable]
public class OnNEvent : UnityEvent { }

public class NEvent : MonoBehaviour {
    public struct NEventLinkId {
        public int Id;
        public NEvent EventObject;
        public NEventLinkId(int id, NEvent obj) { this.Id = id; EventObject = obj; }
    }

    public static List<NEventLinkId> Links = new List<NEventLinkId>();
    public static int NextId = 0;
    private int _thisId;

    [HideInInspector]
    public bool RemoteEvent = false;

    void Awake() {
        lock (Links) {
            _thisId = NextId;
            ++NextId;
            Links.Add(new NEventLinkId(_thisId, this));
        }
    }

    public int GetId() {
        return _thisId;
    }

    public static implicit operator bool(NEvent m) {
        if (m == null) {
            return false;
        }
        if (m.RemoteEvent) {
            m.RemoteEvent = false;
            return true;
        }

        if (m.Get()) {
            return true;
        }
        return false;
    }

    protected virtual bool Get() {
        return false;
    }
}
}
