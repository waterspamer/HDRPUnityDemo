using UnityEngine;
using System.Collections;

namespace Nettle
{

    public class RotateByAngle : MonoBehaviour
    {

        public GameObject Target;
        public float Angle = 180f;
        public float TimeOfRotating = 1f;
        public bool DisableManualRotation { get; set; } = false;


        //rotate animation
        private Quaternion _startPointOfRotation = Quaternion.identity;
        private Quaternion _endPointOfRotation = Quaternion.identity;
        private const float START_SIN_RAD = -Mathf.PI / 2f;
        private const float END_SIN_RAD = Mathf.PI / 2f;
        private float _rotationStartTime = 0f;
        private bool _rotationInProcess = false;
        public bool RotationInProgress{
            get
            {
                return _rotationInProcess;
            }
        }
        private float _rotationTime = 0.0f;


        private void Start()
        {
            if (VisibilityZoneViewer.Instance != null)
            {
                VisibilityZoneViewer.Instance.TransitionBegin.AddListener(BreakRotation);
            }
        }

        private void OnDestroy()
        {
            if (VisibilityZoneViewer.Instance != null)
            {
                VisibilityZoneViewer.Instance.TransitionBegin.RemoveListener(BreakRotation);
            }
        }

        public void Rotate()
        {
            Rotate(Angle, true);
        }

        public void Rotate(bool manual)
        {
            Rotate(Angle,manual);
        }

        public void Rotate(float angle)
        {
            Rotate(angle, true);
        }
 
        public void Rotate(float angle, bool manual)
        {
            if (manual && DisableManualRotation)
            {
                return;
            }

            if (VisibilityZoneViewer.Instance != null && VisibilityZoneViewer.Instance.TransitionRunning)
            {
                return;
            }
            _rotationStartTime = Time.realtimeSinceStartup;
            _startPointOfRotation = Target.transform.rotation;
            _endPointOfRotation = (_rotationInProcess ? _endPointOfRotation : _startPointOfRotation) * Quaternion.AngleAxis(angle, Vector3.up);
            StopAllCoroutines();
            _rotationTime = 0.0f;
            StartCoroutine(MakeRotationAnimationCoroutine());
        }

        public void BreakRotation()
        {
            _rotationInProcess = false;
        }

        IEnumerator MakeRotationAnimationCoroutine()
        {
            _rotationInProcess = true;
            while (_rotationInProcess && _rotationTime < TimeOfRotating)
            {
                _rotationTime = Time.realtimeSinceStartup - _rotationStartTime;
                var progressOfRotation = (Mathf.Sin(Mathf.Lerp(START_SIN_RAD, END_SIN_RAD, _rotationTime / TimeOfRotating)) + 1f) / 2f;
                //Debug.Log("Progress: " + progressOfRotation + ", rotation time: " + _rotationTime + ", time since scene startup: " + Time.realtimeSinceStartup + ", rotation start: " + _rotationStartTime);
                Target.transform.rotation = Quaternion.Lerp(_startPointOfRotation, _endPointOfRotation, progressOfRotation);
                yield return null;
            }
            _rotationInProcess = false;
        }
    }
}
