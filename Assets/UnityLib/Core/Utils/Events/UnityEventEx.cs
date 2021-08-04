using System;
using UnityEngine.Events;

namespace Nettle {

    [Serializable]
    public class UnityEventInteger : UnityEvent<int> {
    }

    [Serializable]
    public class UnityEventFloat : UnityEvent<float> {
    }

    [Serializable]
    public class UnityEventString : UnityEvent<string> {
    }

    [Serializable]
    public class UnityEventBool : UnityEvent<bool> {
    }
}

