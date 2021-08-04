using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace Nettle {

public class RandomEx {


    public static Vector3 InsideCircleOnNavmesh(Vector3 center, float radius, int areaMask)
    {
        Vector2 randInCircle = Random.insideUnitCircle * radius;
        Vector3 randPoint = center + new Vector3(randInCircle.x, center.y, randInCircle.y);
        NavMeshHit hit;
        NavMesh.SamplePosition(randPoint, out hit, radius, areaMask);
        return hit.position;

    }

    public static Vector3 Point(MeshFilter meshFilter) {
        var triangleCount = meshFilter.sharedMesh.triangles.Length / 3;
        var index = Random.Range(0, triangleCount) * 3;
        return PointOnTriangle(meshFilter, index);
    }

    /*public static int Weight(float[] weights, float totalWeight) {
        float r = Random.Range(0, totalWeight);
        float weightSum = 0f;
        int k = -1;
        do {
            k++;
            weightSum += weights[k];
        } while (r > weightSum && k < weights.Length - 1);

        return k;
    }*/

    public static int Weight(IEnumerable<float> weights) {
        return Weight(weights, weights.Sum());
    }

    public static int Weight(IEnumerable<float> weights, float totalWeight) {
        float r = Random.Range(0, totalWeight);
        float weightSum = 0f;
        int k = -1;
        IEnumerator<float> enumerator = weights.GetEnumerator();
        while (enumerator.MoveNext() && r > weightSum)
        {
            k++;
            weightSum += enumerator.Current;
        }

        return k;
    }

    public static float CalculateTriangleWeights(MeshFilter meshFilter, ref float[] weights) {
        var vertices = meshFilter.sharedMesh.vertices;
        var indices = meshFilter.sharedMesh.triangles;
        return CalculateTriangleWeights(vertices, indices, ref weights);

    }


    public static float CalculateTriangleWeights(Vector3[] vertices, int[] indices, ref float[] weights) {
        float totalWeight = 0f;
        for (int i = 0; i < weights.Length; i++) {
            Vector3 v0 = vertices[indices[i * 3]];
            Vector3 v1 = vertices[indices[i * 3 + 1]];
            Vector3 v2 = vertices[indices[i * 3 + 2]];
            weights[i] = Vector3.Cross(v1 - v0, v2 - v0).magnitude / 2;
            totalWeight += weights[i];

        }
        return totalWeight;
    }

    public static Vector3 PointUsingWeights(MeshFilter meshFilter) {
        float[] weights = new float[meshFilter.sharedMesh.triangles.Length / 3];
        float totalWeight = CalculateTriangleWeights(meshFilter, ref weights);
        return PointUsingWeights(meshFilter, weights, totalWeight);
    }

    public static Vector3 PointUsingWeights(MeshFilter meshFilter, float[] weights, float totalWeight) {
        int index = Weight(weights, totalWeight);
        return PointOnTriangle(meshFilter, index*3);
    }

    public static Vector3 PointOnTriangle(MeshFilter meshFilter, int index) {

        var vertices = meshFilter.sharedMesh.vertices;
        var indices = meshFilter.sharedMesh.triangles;
        var parentTransform = meshFilter.transform;
        var v0 = parentTransform.TransformPoint(vertices[indices[index + 0]]);
        var v1 = parentTransform.TransformPoint(vertices[indices[index + 1]]);
        var v2 = parentTransform.TransformPoint(vertices[indices[index + 2]]);
        return PointOnTriangle(v0, v1, v2);
    }

    public static Vector3 PointOnTriangle(Vector3[] vertices, int[] indices, int index) {
        var v0 = vertices[indices[index * 3 + 0]];
        var v1 = vertices[indices[index * 3 + 1]];
        var v2 = vertices[indices[index * 3 + 2]];
        return PointOnTriangle(v0, v1, v2);
    }


    public static Vector3 PointOnTriangle(Vector3 v0, Vector3 v1, Vector3 v2) {  //Magic of Barycentric coordinates

        Vector3 a = v1 - v0;
        Vector3 b = v2 - v1;
        Vector3 c = v2 - v0;

        // Generate a random point in the trapezoid
        Vector3 result = v0 + Random.Range(0f, 1f) * a + Random.Range(0f, 1f) * b;

        // Barycentric coordinates on triangles
        float alpha = ((v1.z - v2.z) * (result.x - v2.x) + (v2.x - v1.x) * (result.z - v2.z)) /
                ((v1.z - v2.z) * (v0.x - v2.x) + (v2.x - v1.x) * (v0.z - v2.z));
        float beta = ((v2.z - v0.z) * (result.x - v2.x) + (v0.x - v2.x) * (result.z - v2.z)) /
               ((v1.z - v2.z) * (v0.x - v2.x) + (v2.x - v1.x) * (v0.z - v2.z));
        float gamma = 1.0f - alpha - beta;

        // The selected point is outside of the triangle (wrong side of the trapezoid), project it inside through the center.
        if (alpha < 0 || beta < 0 || gamma < 0) {
            Vector3 center = v0 + c / 2;
            center = center - result;
            result += 2 * center;
        }

        return result;
    }
}
}
