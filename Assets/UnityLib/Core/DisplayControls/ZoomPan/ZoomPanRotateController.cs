using System;
using UnityEngine;
using System.Collections;

namespace Nettle {

public class ZoomPanRotateController : MonoBehaviour
{
    public VisibilityZoneViewer VisibilityZoneViewer;

    public KeyCode ResetKey = KeyCode.Home;

    public float ZoomSpeed = 5.0f;
    public float PanSpeed = 1.0f;
    public float RotationSpeed = 1.0f;

    public GameObject Target;

    public float RotationArrowSpeed = 1.0f;
    public float ZoomButtonSpeed = 1.0f;

    public float MinZoom = 0.0f;
    public float MaxZoom = 1.0f;

    private float _smoothZoomSpeed = 3f;

    public KeyCode RotateKeyCode = KeyCode.Mouse0;
    public KeyCode PanKeyCode = KeyCode.None;

    public bool IsUseZoom = true;
    public bool IsUsePan = true;
    public bool IsUseRotate = true;

    private Vector3 _startPosition = Vector3.zero;
    private Quaternion _startRotation = Quaternion.identity;
    private Vector3 _startScale = Vector3.zero;

    private float _currentZoom = 1f;
    private float _previousZoom = 1f;

    private Vector2 rotation = Vector2.zero;

    void Awake()
    {
        VisibilityZoneViewer.OnShowZone.AddListener(VisibilityZoneViewerOnOnShowZone);
    }

    void Start()
    {
        _startPosition = transform.position;
        _startRotation = transform.rotation;
        _startScale = transform.localScale;
        if (Target != null)
            _distance = Vector3.Distance(transform.position, Target.transform.position);
    }

    private void VisibilityZoneViewerOnOnShowZone(VisibilityZone visibilityZone)
    {
        SetParamByZone(visibilityZone);
    }

    public void SetParamByZone(VisibilityZone zone)
    {
        _startScale = zone.transform.localScale;
        MinZoom = zone.MinZoom;
        MaxZoom = zone.MaxZoom;

        _startPosition = zone.transform.position;
        _startRotation = zone.transform.rotation;

        if(Target != null)
        _distance = Vector3.Distance(transform.position, Target.transform.position);

        rotation = Vector2.zero;
    }

    void Update()
    {
        if (Input.GetKey(PanKeyCode) || PanKeyCode == KeyCode.None)
        {
            if (IsUsePan)
                Pan();
        }

        if (Input.GetKey(RotateKeyCode) || RotateKeyCode == KeyCode.None || Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.UpArrow)) {
            IsUsePan = false;

            if (IsUseRotate)
                Rotate();
        }

        if (Input.GetKeyUp(RotateKeyCode)) {
            IsUsePan = true;
        }

        if (Input.GetKey(ResetKey))
        {
            Reset();
        }

        if (IsUseZoom)
            Zoom();
    }

    public void Zoom()
    {
        var axis = -Input.GetAxis("Mouse ScrollWheel");
        _currentZoom = Mathf.Clamp(_currentZoom * Mathf.Pow(ZoomSpeed, axis), MinZoom, MaxZoom);

        if (Input.GetKey("[+]") || Input.GetKey(KeyCode.Equals)) _currentZoom += ZoomButtonSpeed;
        if (Input.GetKey("[-]") || Input.GetKey(KeyCode.Minus)) _currentZoom -= ZoomButtonSpeed;

        _previousZoom = Mathf.Lerp(_previousZoom, _currentZoom, _smoothZoomSpeed * Time.deltaTime);
        transform.localScale = _startScale * _previousZoom;
    }

    public void Pan()
    {
        var direction = new Vector3(Input.GetAxis("Mouse X")*transform.localScale.x/10f, 0,
            Input.GetAxis("Mouse Y")*transform.localScale.x/10f);

        direction = transform.TransformDirection(direction);
        direction *= -PanSpeed;

        transform.localPosition += direction;
    }

    private float _distance;
    public void Rotate()
    {
        if (Target != null) {
            rotation.x += Input.GetAxis("Mouse X");// Mathf.Repeat(rotation.x + Input.GetAxis("Mouse X"), 360f);
            rotation.y += Input.GetAxis("Mouse Y");// Mathf.Clamp(rotation.y + Input.GetAxis("Mouse Y"), -90f, 90f);

            var x = ((Input.GetKey(KeyCode.LeftArrow) ? 1 : 0) - (Input.GetKey(KeyCode.RightArrow) ? 1 : 0))*
                    RotationArrowSpeed;
            var y = ((Input.GetKey(KeyCode.DownArrow) ? 1 : 0) - (Input.GetKey(KeyCode.UpArrow) ? 1 : 0))*
                    RotationArrowSpeed;

            rotation.x += x;
            rotation.y += y;

            var rot = Quaternion.Euler(rotation.y, rotation.x, 0f);
            var position = rot * (new Vector3(0f, _distance, 0f)) + Target.transform.position;

            transform.rotation = rot;
            transform.position = position;

            //transform.rotation = _startRotation*
            //                     Quaternion.Euler(0, 0, rotation.x)*
            //                     Quaternion.Euler(-rotation.y, 0, 0);
        }
    }

    public void Reset()
    {
        transform.position = _startPosition;
        transform.rotation = _startRotation;
        transform.localScale = _startScale;

        rotation = Vector2.zero;
        _currentZoom = 1f;
    }
}
}
