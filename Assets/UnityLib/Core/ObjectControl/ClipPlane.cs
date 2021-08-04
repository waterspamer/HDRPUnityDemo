using UnityEngine;

namespace Nettle {

public class ClipPlane : MonoBehaviour {

    public float XWidthPlane;
    public float ZWidthPlane;
    public Material[] ReadMaterials;

    private Vector3 _normal;
    private Vector3 _position;
    private float _xWidth;
    private float _zWidth;

    private MeshFilter _meshFilter = null;
    private MeshRenderer _meshRenderer = null;

    // Use this for initialization
    void Start () {
	    _normal = GetComponent<Transform>().up;
	    _position = GetComponent<Transform>().position;
	    var d = Vector3.Dot(_normal, _position);
    }
	
	// Update is called once per frame
	void Update () {
        if (_xWidth != XWidthPlane || _zWidth != ZWidthPlane) {
            _xWidth = XWidthPlane;
            _zWidth = ZWidthPlane;

            //adding components if need
            if (_meshFilter == null) {
                _meshFilter = gameObject.AddComponent<MeshFilter>();
            }

            if (_meshRenderer == null) {
                _meshRenderer = gameObject.AddComponent<MeshRenderer>();
            }

            _meshFilter.mesh = CreatePlane(_xWidth, _zWidth, ReadMaterials.Length);
            _meshRenderer.materials = ReadMaterials;
        }

        _normal = GetComponent<Transform>().up;
        _position = GetComponent<Transform>().position;
        var d = Vector3.Dot(_normal, _position);

        Shader.SetGlobalFloat("_cutX", _normal.x);
        Shader.SetGlobalFloat("_cutY", _normal.y);
        Shader.SetGlobalFloat("_cutZ", _normal.z);
        Shader.SetGlobalFloat("_cutW", d);
    }


    Mesh CreatePlane(float xWidth, float zWidth, int subMeshCount) {
        xWidth /= 2f;
        zWidth /= 2f;
        var mesh = new Mesh();
        Vector3[] vertices = new Vector3[] {
             new Vector3(xWidth, 0, zWidth),
             new Vector3(xWidth, 0, -zWidth),
             new Vector3(-xWidth, 0, zWidth),
             new Vector3(-xWidth, 0, -zWidth),
        };
        Vector2[] uv = new Vector2[] {
             new Vector2(1, 1),
             new Vector2(1, 0),
             new Vector2(0, 1),
             new Vector2(0, 0),
        };
        int[] triangles = new int[] {
             0, 1, 2,
             2, 1, 3,
        };
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.subMeshCount = subMeshCount;
        for (int i = 0; i < subMeshCount; i++) {
            mesh.SetTriangles(triangles, i);
        }
        mesh.RecalculateNormals();
        return mesh;
    }

    void OnDrawGizmosSelected() {

        Gizmos.color = Color.green;

        Vector3 leftforward = transform.position - XWidthPlane * transform.right * 0.5f + ZWidthPlane * transform.forward * 0.5f;
        Vector3 rightforward = leftforward + XWidthPlane * transform.right;
        Vector3 rightback = rightforward - ZWidthPlane * transform.forward;
        Vector3 leftback = rightback - XWidthPlane * transform.right;

        Gizmos.DrawLine(leftforward, rightforward);
        Gizmos.DrawLine(rightforward, rightback);
        Gizmos.DrawLine(rightback, leftback);
        Gizmos.DrawLine(leftback, leftforward);
    }
}
}
