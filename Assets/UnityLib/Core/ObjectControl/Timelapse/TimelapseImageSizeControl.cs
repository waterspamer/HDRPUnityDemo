using UnityEngine;
using System.Collections;

namespace Nettle {

public class TimelapseImageSizeControl : MonoBehaviour {

    public RectTransform RTransform;
    [ConfigField]
    public float VertSize = 30.0f;

    private void Start() {
        if (RTransform != null) {
            var sizeDelta = RTransform.sizeDelta;
            sizeDelta.y = VertSize;
            RTransform.sizeDelta = sizeDelta;
        }
    }

}
}
