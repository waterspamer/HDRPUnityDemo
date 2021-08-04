using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nettle {

[ExecuteBefore(typeof(VisibilityZoneViewer))]
[ExecuteAfter(typeof(Config))]
public class HideByDeviceType : MonoBehaviour {
    public NettleDeviceType TargetType = NettleDeviceType.NettleBox;
    private void Awake() {
        gameObject.SetActive(TargetType == VisibilityZoneViewer.DeviceType);
    }
}
}
