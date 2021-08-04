using UnityEngine;
using System.Collections;

namespace Nettle {

    public sealed class MotionParallax3D2 : MonoBehaviour {
        public MotionParallaxDisplay Display;
        public StereoEyes eyes;
        [SerializeField]
        private bool _useStereoProjection = true;
        [SerializeField]
        private float _FOVScale = 2.0f;
        [SerializeField]
        private bool _forceNoTilt = true;

        private Vector3[] _worldScreenCorners;
        private Camera _camera;

        [ConfigField]
        public bool UseStereoProjection { get => _useStereoProjection; set => _useStereoProjection = value; }
        [ConfigField]
        public float FOVScale { get => _FOVScale; set => _FOVScale = value; }
        private void Reset() {
            if (Display == null) {
                Display = SceneUtils.FindObjectIfSingle<MotionParallaxDisplay>();
            }
        }

        private void Awake() {
            _camera = GetComponent<Camera>();
#if UNITY_WEBGL
            UseStereoProjection = false;
#endif
        }

        private float CalculateCameraNoseFov() {
            float maxY = 0;
            float currentScreenAspect = Screen.width / (float)Screen.height;
            Display.GetWorldScreenCorners(out _worldScreenCorners);
            foreach (var corner in _worldScreenCorners) {
                Vector3 cameraLocalCorner = transform.InverseTransformPoint(corner);
                cameraLocalCorner /= cameraLocalCorner.z;
                maxY = Mathf.Max(maxY, Mathf.Abs(cameraLocalCorner.y));
                maxY = Mathf.Max(maxY, Mathf.Abs(cameraLocalCorner.x * currentScreenAspect));
            }
            return Mathf.Atan(maxY / currentScreenAspect) * Mathf.Rad2Deg * 2;
        }

        public void LateUpdateManual() {
            Quaternion rotation = new Quaternion();
            rotation.SetLookRotation(Vector3.down);
            transform.rotation = Display.transform.rotation * rotation;

            if (UseStereoProjection) {
                UpdateProjection();
            }
            else {            
                transform.LookAt(Display.transform.position, _forceNoTilt?Vector3.up:Display.transform.up);
                float fov = CalculateCameraNoseFov();
                _camera.fieldOfView = fov / FOVScale;
                _camera.ResetProjectionMatrix();
                _camera.ResetAspect();
            }
        }

        private void UpdateProjection() {
            float eyesScale = Display == null ? 1.0f : Display.Width / 2.0f;
            Vector2 eyeRtSize = eyes.GetEyeRTSize();
            NettleProjectionMatrix(_camera, Display.transform.InverseTransformPoint(_camera.transform.position), eyesScale, eyeRtSize);
        }

        public static void NettleProjectionMatrix(Camera cam, Vector3 eye, float eyesScale, Vector2 eyeRtSize) {
            
            eye /= eyesScale;

            float farClip = cam.farClipPlane; 
            float nearClip = cam.nearClipPlane; 

            Vector2 shift = new Vector2(0.5f * eye.x, 0.5f * eye.z);

            float multiplayer = 2f * nearClip / eye.y;

            float aspect = eyeRtSize.y / eyeRtSize.x; 

            float right = (-shift.x + 0.5f) * multiplayer;
            float left = (-shift.x - 0.5f) * multiplayer;
            float top = (-shift.y + aspect * 0.5f) * multiplayer;
            float bottom = (-shift.y - aspect * 0.5f) * multiplayer;

            cam.aspect = aspect; 
            float fov = (2 * Mathf.Atan(1f / Mathf.Max(eye.y, 0.01f))) * Mathf.Rad2Deg; // 57.2957795f;
            cam.fieldOfView = fov;
            cam.nearClipPlane = nearClip;
            cam.farClipPlane = farClip;

            Matrix4x4 matrix = PerspectiveOffCenter(left, right, bottom, top, nearClip, farClip);
            cam.projectionMatrix = matrix;

        }

        private static Matrix4x4 PerspectiveOffCenter(float left, float right, float bottom, float top, float nearClip,
            float farClip) {
            float x = 2.0f * nearClip / (right - left);
            float y = 2.0f * nearClip / (top - bottom);
            float a = (right + left) / (right - left);
            float b = (top + bottom) / (top - bottom);
            float c = -(farClip + nearClip) / (farClip - nearClip);
            float d = -(2.0f * farClip * nearClip) / (farClip - nearClip);
            float e = -1.0f;
            Matrix4x4 matrix = new Matrix4x4();
            matrix[0, 0] = x;
            matrix[0, 1] = 0;
            matrix[0, 2] = a;
            matrix[0, 3] = 0;
            matrix[1, 0] = 0;
            matrix[1, 1] = y;
            matrix[1, 2] = b;
            matrix[1, 3] = 0;
            matrix[2, 0] = 0;
            matrix[2, 1] = 0;
            matrix[2, 2] = c;
            matrix[2, 3] = d;
            matrix[3, 0] = 0;
            matrix[3, 1] = 0;
            matrix[3, 2] = e;
            matrix[3, 3] = 0;
            return matrix;
        }
    }
}
