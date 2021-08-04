using UnityEngine;
using System.Collections;

namespace Nettle {

public class ObjectWithGizmos : MonoBehaviour
{
    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawCube(gameObject.transform.position, new Vector3(1, 1, 1));
    }
}
}
