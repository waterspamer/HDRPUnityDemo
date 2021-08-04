using UnityEngine;
using System.Collections;
using UnityEngine.Events;

namespace Nettle {

public class DrivingForce : MonoBehaviour {

    public GameObject Target = null;
    public Vector3 StartPoint = Vector3.zero;
    public Vector3 EndPoint = Vector3.zero;
    public float MovingTime = 1f;

    public UnityEvent Start = new UnityEvent();
    public UnityEvent Stop = new UnityEvent();

    private bool _isMoving = false;
    private float _time = 0f;

    public void StartMoving () {
        if (!_isMoving) {
            _time = 0f;
            _isMoving = true;
            Start.Invoke();
        }
    }

    public void StopMoving() {
        if (_isMoving) {
            _isMoving = false;
            Target.transform.position = EndPoint;
            Stop.Invoke();
        }
    }
	
	// Update is called once per frame
	void Update () {
        if (_isMoving) {
            Target.transform.position = Vector3.Lerp(StartPoint, EndPoint, _time / MovingTime);

            if (_time > MovingTime) {
                StopMoving();
            }

            _time += Time.deltaTime;
        }
    }
}
}
