using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using UnityEngine.Serialization;
using System;

namespace Nettle {
    public class MovingAtObject : MonoBehaviour {

        [SerializeField]
        private bool _matchTargetWhenStartMoving = true;

        public GameObject Target;
        public GameObject View;

        public Vector3 Offset = Vector3.zero;
        public bool UseCustomViewScale = false;

        [SerializeField]
        [FormerlySerializedAs("SpeedLerp")]
        private float _lerpSpeed = 0.5f;
        public float LerpSpeed { get => _lerpSpeed; set => _lerpSpeed = value; }

        public float Scale = 0;
        [HideInInspector]
        [Obsolete]
        public Vector3 ViewScale = Vector3.zero;
        [SerializeField]
        private float _scaleAccelerationModifire = 1000f;
        [SerializeField]
        private float _deadZoneRadius = 0;
        [SerializeField]
        private bool _limitDistance = false;
        [SerializeField]
        private float _targetMovementCoefficient = 1;


        public float RotationSpeed = 30f;
        public bool MovingByY;
        [SerializeField]
        [FormerlySerializedAs("IsRotate")]
        private bool _isRotate;
        public bool IsRotate { get => _isRotate; set => _isRotate = value; }

        public bool WorkOnRemoteDisplay = false;

        public float StartDelay = 0.0f;
        private float _timeSinceExecuted = 0.0f;
        private bool _executed;
        private bool _isMoving;
        private float _lastSpeed = 0;
        private float _lastScaleSpeed = 0;

        private Vector3 _lastTargetPosition;

        public UnityEvent OnStartMoving = new UnityEvent();
        public UnityEvent OnEndMoving = new UnityEvent();

        private void Reset() {
            EditorInit();
        }

        private void OnValidate() {
            EditorInit();
        }

        private void EditorInit() {
            if (Scale == 0 && ViewScale.x != 0) {
                Scale = ViewScale.x;
            }
        }

        private Vector3 TargetPosition {
            get {
                return new Vector3(Target.transform.position.x + Offset.x, MovingByY ? Target.transform.position.y + Offset.y : View.transform.position.y, Target.transform.position.z + Offset.z);
            }
        }

        void Start() {
            _lastTargetPosition = TargetPosition;
        }

        void LateUpdate() {
            if (!WorkOnRemoteDisplay) {
                return;
            }
            if (_isMoving && Target != null && View != null) {

                View.transform.position += (TargetPosition - _lastTargetPosition) * _targetMovementCoefficient;
                _lastTargetPosition = TargetPosition;

                Vector3 toTarget = TargetPosition - View.transform.position;
                float distance = toTarget.magnitude;
                float maxDistance = MotionParallaxDisplay.Instance.Width * MotionParallaxDisplay.Instance.transform.localScale.x * 9 / 16;
                
                //find closest point to target at dead zone radius
                Vector3 targetAdjustedPosition = TargetPosition;
                float adjustedDistance = distance;
                if (_deadZoneRadius > 0)
                {
                    targetAdjustedPosition = TargetPosition - toTarget.normalized * _deadZoneRadius;
                    adjustedDistance = Vector3.Distance(targetAdjustedPosition, View.transform.position);
                }
                if (_limitDistance && adjustedDistance > maxDistance)
                {
                    View.transform.position = targetAdjustedPosition - toTarget.normalized * maxDistance;
                }else
                {
                    if (distance > _deadZoneRadius)
                    {
                        float desireSpeed = distance * LerpSpeed;
                        View.transform.position = Vector3.Lerp(View.transform.position, targetAdjustedPosition, LerpSpeed * Time.deltaTime);
                    }
                }


                if (IsRotate) {
                    View.transform.rotation = Quaternion.RotateTowards(View.transform.rotation, Target.transform.rotation, RotationSpeed * Time.deltaTime);
                    //View.transform.localEulerAngles = Vector3.Lerp(View.transform.localEulerAngles, TargetRotation(), SpeedLerp * Time.deltaTime);
                }
                if (UseCustomViewScale) {
                    float desireScaleSpeed = Mathf.Abs(Scale - View.transform.localScale.x) * LerpSpeed;

                    if (desireScaleSpeed <= _lastScaleSpeed) {
                        float scale = Mathf.Lerp(View.transform.localScale.x, Scale, LerpSpeed * Time.deltaTime);
                        View.transform.localScale = new Vector3(scale, scale, scale);
                        _lastScaleSpeed = desireScaleSpeed;
                    } else {
                        float currentMaxScaleSpeed = _lastScaleSpeed + _scaleAccelerationModifire * Time.deltaTime;
                        _lastScaleSpeed = Mathf.Clamp(desireScaleSpeed, 0, currentMaxScaleSpeed);

                        float desireChange = Mathf.Abs(Scale - View.transform.localScale.x);
                        float maxChange = currentMaxScaleSpeed * Time.deltaTime;
                        float scaleBy = Mathf.Sign(Scale - View.transform.localScale.x) * Mathf.Clamp(desireChange, 0, maxChange);

                        View.transform.localScale += new Vector3(scaleBy, scaleBy, scaleBy);
                    }


                }
            }
        }

        public void MatchTarget() {
            if (!_isMoving)
            {
                return;
            }
            View.transform.position = TargetPosition;
            if (IsRotate) {
                View.transform.rotation = Target.transform.rotation;
            }
            if (UseCustomViewScale) {
                View.transform.localScale = ViewScale;
            }
        }

        public void SetOffset(float x, float y) {
            Offset = new Vector2(x, y);
        }

        public void SetOffsetString(string param) {
            float x = float.Parse(param.Split(':')[0]);
            float y = float.Parse(param.Split(':')[1]);

            Offset = new Vector2(x, y);
        }

        public void StartMoving() {
            _executed = true;
            _isMoving = false;
            _timeSinceExecuted = 0.0f;
            _lastSpeed = 0;
            _lastScaleSpeed = 0;
            if (_matchTargetWhenStartMoving) {
                StartCoroutine(DelayedMatchTarget());
            }
        }

        public void StartDelayedMatchTarget()
        {
            StartCoroutine(DelayedMatchTarget());
        }

        private IEnumerator DelayedMatchTarget() {
            yield return null;
            MatchTarget();
        }

        public void StopMoving() {
            _isMoving = false;
            _executed = false;
            if (OnEndMoving != null)
                OnEndMoving.Invoke();
        }

        private void Update() {
            if (_executed && !_isMoving) {
                _timeSinceExecuted += Time.deltaTime;
                if (_timeSinceExecuted >= StartDelay) {
                    _isMoving = true;
                    if (OnStartMoving != null) {
                        OnStartMoving.Invoke();
                    }
                }
            }
        }
    }
}
