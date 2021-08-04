using UnityEngine;
using System.Collections;

namespace Nettle {

public class ExtendedFlycam : MonoBehaviour {

    /*
    EXTENDED FLYCAM
        Desi Quintans (CowfaceGames.com), 17 August 2012.
        Based on FlyThrough.js by Slin (http://wiki.unity3d.com/index.php/FlyThrough), 17 May 2011.
 
    LICENSE
        Free as in speech, and free as in beer.
 
    FEATURES
        WASD/Arrows:    Movement
                  Q:    Climb
                  E:    Drop
                      Shift:    Move faster
                    Control:    Move slower
                        End:    Toggle cursor locking to screen (you can also press Ctrl+P to toggle play mode on and off).
    */

    public Transform CameraTransform;
    public float cameraSensitivity = 90;
    public float climbSpeed = 4;
    public float normalMoveSpeed = 10;
    public float slowMoveFactor = 0.25f;
    public float fastMoveFactor = 10;
    public bool hideMouse = false;

    private float rotationX = 0.0f;
    private float rotationY = 0.0f;

    void Awake() {
        if (CameraTransform == null) {
            CameraTransform = transform;
        }
    }

    void Start() {
        Screen.lockCursor = hideMouse;
    }

    void Update() {
        rotationX += Input.GetAxis("Mouse X") * cameraSensitivity * Time.deltaTime;
        rotationY += Input.GetAxis("Mouse Y") * cameraSensitivity * Time.deltaTime;
        rotationY = Mathf.Clamp(rotationY, -90, 90);

        CameraTransform.localRotation = Quaternion.AngleAxis(rotationX, Vector3.up);
        CameraTransform.localRotation *= Quaternion.AngleAxis(rotationY, Vector3.left);

        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
            CameraTransform.position += CameraTransform.forward * (normalMoveSpeed * fastMoveFactor) * Input.GetAxis("Vertical") * Time.deltaTime;
            CameraTransform.position += CameraTransform.right * (normalMoveSpeed * fastMoveFactor) * Input.GetAxis("Horizontal") * Time.deltaTime;
        } else if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) {
            CameraTransform.position += CameraTransform.forward * (normalMoveSpeed * slowMoveFactor) * Input.GetAxis("Vertical") * Time.deltaTime;
            CameraTransform.position += CameraTransform.right * (normalMoveSpeed * slowMoveFactor) * Input.GetAxis("Horizontal") * Time.deltaTime;
        } else {
            CameraTransform.position += CameraTransform.forward * normalMoveSpeed * Input.GetAxis("Vertical") * Time.deltaTime;
            CameraTransform.position += CameraTransform.right * normalMoveSpeed * Input.GetAxis("Horizontal") * Time.deltaTime;
        }


        if (Input.GetKey(KeyCode.Space)) { CameraTransform.position += CameraTransform.up * climbSpeed * Time.deltaTime; }
        if (Input.GetKey(KeyCode.LeftAlt)) { CameraTransform.position -= CameraTransform.up * climbSpeed * Time.deltaTime; }

        if (Input.GetKeyDown(KeyCode.End)) {
            Screen.lockCursor = (Screen.lockCursor == false) ? true : false;
        }
    }
}
}
