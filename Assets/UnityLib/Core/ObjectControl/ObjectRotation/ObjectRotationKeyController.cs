
using UnityEngine;
using System.Collections;
using System;

namespace Nettle {



public class ObjectRotationKeyController : MonoBehaviour {
    public ObjectRotationBase Target;
    public KeyCode UpKey = KeyCode.W;
    public KeyCode LeftKey = KeyCode.A;
    public KeyCode DownKey = KeyCode.S;
    public KeyCode RightKey = KeyCode.D;
    public KeyCode ResetKey = KeyCode.R;   

    void Update () {
        Vector2 rotation = Vector2.zero;
        if (Input.GetKey(UpKey)) {
            rotation.y = 1;
        }
        else if (Input.GetKey(DownKey)) {
            rotation.y = -1;
        }
        if (Input.GetKey(RightKey)) {
            rotation.x = 1;
        }
        else if (Input.GetKey(LeftKey)) {
            rotation.x = -1;
        }
        else if (Input.GetKey(ResetKey)){
            Target.ResetRotation();
        }
        if (rotation.magnitude > 0) {
            Target.Rotate(rotation);
        }
        
    }

}
}
