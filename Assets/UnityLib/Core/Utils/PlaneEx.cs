using UnityEngine;
using System.Collections;

namespace Nettle {

public static class PlaneEx {
    public static Vector3 ProjectPoint(this Plane p, Vector3 point) {
        var distance = p.GetDistanceToPoint(point);
        return point - p.normal * distance;
    }
}
}
