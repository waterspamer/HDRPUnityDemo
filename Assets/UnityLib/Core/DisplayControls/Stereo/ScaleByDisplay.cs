using UnityEngine;
using System.Collections;

namespace Nettle {

public class ScaleByDisplay : MonoBehaviour {
    public MotionParallaxDisplay display;
    public float displaySizeMul = 0.1f;
    public MeshFilter ReferenceObject;
    public float minZoom = 0.5f;
    public float maxZoom = 2.0f;
    private float refScale = 1.0f;
    private float srcDiagonalLength;
    private Vector3[] displayCorners;

    void Start () {
        UpdateStartParams();
    }

    void UpdateStartParams() {
        srcDiagonalLength = GetDiagonalSize(GetComponent<MeshFilter>());
        if (ReferenceObject != null) {
            refScale = srcDiagonalLength / GetDiagonalSize(ReferenceObject);
        }
    }

    float GetDiagonalSize(MeshFilter mf) {
        Vector3 min;
        Vector3 max;
        GetWorldAnchorPoints(mf, out min, out max);
        return Vector3.Distance(min, max);
    }

	void Update () {
        transform.localScale *= CalcScale();
    }

    float CalcScale() {
        Vector3 min;
        Vector3 max;

        GetWorldAnchorPoints(GetComponent<MeshFilter>(), out min, out max);

        var anchorSize = Vector3.Distance(min, max) ;
        display.GetWorldScreenCorners(out displayCorners);
        var displayWidth = Vector3.Distance(displayCorners[0], displayCorners[1]);

        var targetSize = displayWidth * displaySizeMul * refScale;
        targetSize = Mathf.Clamp(targetSize, srcDiagonalLength * minZoom, srcDiagonalLength * maxZoom);

        var scale = targetSize / anchorSize;
        return scale;
    }

    void GetWorldAnchorPoints(MeshFilter mf, out Vector3 p1, out Vector3 p2) {
        var localMin = mf.sharedMesh.bounds.min;
        var localMax = mf.sharedMesh.bounds.max;

        p1 = mf.transform.TransformPoint(localMin);
        p2 = mf.transform.TransformPoint(localMax);
    }

    MeshFilter GetSourceMeshFilter() {
        if (ReferenceObject != null) {
            return ReferenceObject;
        } else {
            return GetComponent<MeshFilter>();
        }
    }

    void OnDrawGizmosSelected() {
        if (!Application.isPlaying) {
            UpdateStartParams();
        }
      
        Vector3 min;
        Vector3 max;
        var mf = GetComponent<MeshFilter>();

        GetWorldAnchorPoints(mf, out min, out max);
        var anchorSize = Vector3.Distance(min, max);
      
        Gizmos.color = Color.red;
        Gizmos.DrawLine(min, max);
        Gizmos.DrawWireSphere(min, anchorSize * 0.025f);
        Gizmos.DrawWireSphere(max, anchorSize * 0.025f);

        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(mf.sharedMesh.bounds.center, mf.sharedMesh.bounds.size);
        Gizmos.color = Color.red;
        Gizmos.matrix = transform.localToWorldMatrix * Matrix4x4.Scale(new Vector3(maxZoom, maxZoom, maxZoom) / (anchorSize / srcDiagonalLength));
        Gizmos.DrawWireCube(mf.sharedMesh.bounds.center, mf.sharedMesh.bounds.size);
        Gizmos.color = Color.green;
        Gizmos.matrix = transform.localToWorldMatrix * Matrix4x4.Scale(new Vector3(minZoom, minZoom, minZoom) / (anchorSize / srcDiagonalLength));
        Gizmos.DrawWireCube(mf.sharedMesh.bounds.center, mf.sharedMesh.bounds.size );
    }
}
}
