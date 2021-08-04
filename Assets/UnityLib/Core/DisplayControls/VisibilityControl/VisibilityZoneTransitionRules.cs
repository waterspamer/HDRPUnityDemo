using UnityEngine;
using System;
using System.Linq;
using UnityEditor;

namespace Nettle {

[Serializable]
public class VisibilityZoneTransitionRules {
    public VisibilityZone[] TargetZones;
    public VisibilityZoneTransition TransitionSettings;

    public bool Match(VisibilityZone zone) {
        return TargetZones != null && TargetZones.Any(v => v == zone);
    }
}
}
