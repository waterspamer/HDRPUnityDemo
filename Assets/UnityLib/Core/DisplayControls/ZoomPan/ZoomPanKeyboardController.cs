using UnityEngine;

namespace Nettle {

public class ZoomPanKeyboardController : MonoBehaviour
{
    public ZoomPan ZoomPan;

    [ConfigField("TranslateSpeed")]
    public float TranslateSpeed = 1.0f;
    [ConfigField("UpDownSpeed")]
    public float UpDownSpeed = 0.1f;

    public bool OnKeyboardMove = true;

    public KeyCode Up = KeyCode.UpArrow;
    public KeyCode Down = KeyCode.DownArrow;
    public KeyCode Left = KeyCode.LeftArrow;
    public KeyCode Right = KeyCode.RightArrow;

    public bool OnKeyboardZoom = true;

    public KeyCode UpZoom = KeyCode.PageUp;
    public KeyCode DownZoom = KeyCode.PageDown;

    void Reset()
    {
        ZoomPan = SceneUtils.FindObjectIfSingle<ZoomPan>();
    }

    void Update () 
    {
	    if (OnKeyboardMove)
	    {
	        var axisX = ((Input.GetKey(Left) ? 0 : TranslateSpeed) - (Input.GetKey(Right) ? 0 : TranslateSpeed));
	        var axisY = ((Input.GetKey(Down) ? 0 : TranslateSpeed) - (Input.GetKey(Up) ? 0 : TranslateSpeed));

	        ZoomPan.Move(axisX, axisY);
	    }

	    if (OnKeyboardZoom) {
	        float zoom = (Input.GetKey(DownZoom) ? -UpDownSpeed : UpDownSpeed) -
	                     (Input.GetKey(UpZoom) ? -UpDownSpeed : UpDownSpeed);
	        if (Mathf.Abs(zoom) > 0.001f) {
                ZoomPan.DoZoom(zoom);
            }
	    }
	}
}
}
