using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.Linq;

namespace Nettle {

public class CentrifyChildren {
    [MenuItem("Nettle/Centrify Children/BoundsCenter")]
    public static void CentrifyChildrenFunc() {
        var objs = Selection.gameObjects;

        foreach (var obj in objs) {
            var children = obj.transform.GetChildren();
          
            foreach (var child in children) {
                var centers = new List<Vector3>();
                GetBoundsCenterRecursive(child, centers);

                var center = centers.Aggregate(Vector3.zero, (a, b) => a + b) / centers.Count;

                var offset = obj.transform.position - center;
                child.position += offset;
            }
        }
    }

    private static void GetBoundsCenterRecursive(Transform tr, List<Vector3> boundsCenters) {
        var renderer = tr.GetComponent<Renderer>();
        if (renderer != null) {
            boundsCenters.Add(renderer.bounds.center); 
        }

        foreach (var child in tr.GetChildren()) {
            GetBoundsCenterRecursive(child, boundsCenters);
        }
    }
}
}
