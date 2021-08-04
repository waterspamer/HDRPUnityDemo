using UnityEngine;

namespace Nettle {

public class DebugObjectMover: MonoBehaviour {

    public Vector3 startPos = new Vector3(200, 50, 100);
    public Vector3 endPos = new Vector3(100, 50, 100);

    public float time = 2f;

    // Use this for initialization
    void Start() {

    }

    public bool direction;
    // Update is called once per frame
    void Update() {
        direction = Mathf.FloorToInt(Time.time / time) % 2 != 0;
        Vector3 start;
        Vector3 end;
        if (direction) {
            start = startPos;
            end = endPos;
        } else {
            start = endPos;
            end = startPos;
        }
        transform.position = Vector3.Lerp(start, end, Mathf.Repeat(Time.time, time) / time);
    }
}
}
