using UnityEngine;
using System.Collections;

namespace Nettle {

public static class RectTransformEx {

    public static Bounds GetWorldBounds(this RectTransform rt) {
        Vector3[] corners = new Vector3[4];

        rt.GetWorldCorners(corners);

        Vector3 boundMin = corners[0];
        Vector3 boundMax = corners[0];

        for (int i = 1; i < 4; ++i) {
            boundMin = Vector3.Min(boundMin, corners[i]);
            boundMax = Vector3.Max(boundMax, corners[i]);
        }

        return new Bounds((boundMax + boundMin) * 0.5f, boundMax - boundMin);
    }
}
}
