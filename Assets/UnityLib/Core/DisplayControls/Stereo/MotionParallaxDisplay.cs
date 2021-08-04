using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Nettle {
    public sealed class MotionParallaxDisplay : MonoSingleton<MotionParallaxDisplay> {
        [SerializeField]
        private float _width = 2.0f;
        private Vector3[] _screenCorners;
        [ConfigField("Width")]
        public float Width { get => _width; set => _width = value; }
#if UNITY_EDITOR
        public AspectRatio editorAspect = AspectRatio.Aspect16by9;
#endif
        private void GetLocalScreenCorners(float virtualScreenWidth, float screenAspect) {
            var halfWidth = virtualScreenWidth * 0.5f;
            var halfHeigth = halfWidth * screenAspect;
            _screenCorners = new Vector3[]
            {
            new Vector3(-halfWidth, 0, halfHeigth),
            new Vector3(halfWidth, 0, halfHeigth),
            new Vector3(halfWidth, 0, -halfHeigth),
            new Vector3(-halfWidth, 0, -halfHeigth)
        };
        }

        private float GetAspect() {
#if UNITY_EDITOR
            switch (editorAspect) {
                case AspectRatio.Aspect5by4:
                    return 4.0f / 5.0f;
                case AspectRatio.Aspect16by10:
                    return 10.0f / 16.0f;
                case AspectRatio.Aspect16by9:
                    return 9.0f / 16.0f;
                case AspectRatio.Aspect4by3:
                    return 3.0f / 4.0f;
            }
#endif
            //TODO: Fix it!
            return 9.0f / 16.0f;
        }

        public void GetWorldScreenCorners(out Vector3[] screenCorners) {
            GetLocalScreenCorners(Width, GetAspect());
            for (var i = 0; i < _screenCorners.Length; ++i) {
                _screenCorners[i] = transform.TransformPoint(_screenCorners[i]);
            }
            screenCorners = _screenCorners;
        }

        public void GetWorldScreenCorners() {
            GetLocalScreenCorners(Width, GetAspect());
            for (var i = 0; i < _screenCorners.Length; ++i) {
                _screenCorners[i] = transform.TransformPoint(_screenCorners[i]);
            }
        }

        public float PixelsPerUnit() {
            return Screen.width / (Width * transform.localScale.x);
        }

        private void OnDrawGizmos() {
#if UNITY_EDITOR
            float screenAspect = GetAspect();
            Vector3 sideLineOffset = Vector3.forward * Width * 0.1f;
            var oldMatrix = Handles.matrix;
            Handles.matrix = transform.localToWorldMatrix;
            Vector3 rightOffset = Vector3.right * Width * 0.5f;

            //Real gizmo
            Handles.color = Color.blue;
            //horizontal line
            Handles.DrawLine(-rightOffset, rightOffset);
            //side lines
            Handles.DrawLine(-rightOffset + sideLineOffset, -rightOffset - sideLineOffset);
            Handles.DrawLine(rightOffset + sideLineOffset, rightOffset - sideLineOffset);
            //coordsys
            Handles.DrawLine(Vector3.zero, Vector3.up * Width * 0.1f);
            Handles.DrawLine(Vector3.zero, Vector3.forward * Width * 0.1f);

            //Abstract gizmo
            var lineSpacing = 4.0f;
            var screenTopOffset = Vector3.forward * Width * 0.5f * screenAspect;

            Handles.color = new Color(0, 0, 1, 0.65f);
            //Left size
            Handles.DrawDottedLine(-rightOffset + sideLineOffset, -rightOffset + screenTopOffset, lineSpacing);
            Handles.DrawDottedLine(-rightOffset - sideLineOffset, -rightOffset - screenTopOffset, lineSpacing);
            //Right side
            Handles.DrawDottedLine(rightOffset + sideLineOffset, rightOffset + screenTopOffset, lineSpacing);
            Handles.DrawDottedLine(rightOffset - sideLineOffset, rightOffset - screenTopOffset, lineSpacing);
            //Top + Bottom
            Handles.DrawDottedLine(-rightOffset + screenTopOffset, rightOffset + screenTopOffset, lineSpacing);
            Handles.DrawDottedLine(-rightOffset - screenTopOffset, rightOffset - screenTopOffset, lineSpacing);

            Handles.matrix = oldMatrix;

            GetWorldScreenCorners();

            Handles.color = Color.red;
            Handles.DrawWireDisc(_screenCorners[0], transform.up, 0.1f);
            Handles.color = Color.green;
            Handles.DrawWireDisc(_screenCorners[1], transform.up, 0.1f);
            Handles.color = Color.yellow;
            Handles.DrawWireDisc(_screenCorners[2], transform.up, 0.1f);
            Handles.color = Color.blue;
            Handles.DrawWireDisc(_screenCorners[3], transform.up, 0.1f);
#endif
        }
    }
}
