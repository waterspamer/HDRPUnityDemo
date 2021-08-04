using UnityEngine;
using System.Collections;

namespace Nettle {

public class SineMovement : MonoBehaviour {

    public float amplitude;
    public float frequency;
    // Use this for initialization

    // Update is called once per frame
    void Update() {
        var v = amplitude * (Mathf.Sin(2 * Mathf.PI * frequency * Time.time) - Mathf.Sin(2 * Mathf.PI * frequency * (Time.time - Time.deltaTime))) * transform.up;
        GetComponent<UnityEngine.AI.NavMeshAgent>().baseOffset += v.y;
        //GetComponent<NavMeshAgent>().baseOffset += amplitude * (Mathf.Sin(2 * Mathf.PI * frequency * Time.time) - Mathf.Sin(2 * Mathf.PI * frequency * (Time.time - Time.deltaTime))) * transform.up;
    }
}
}
