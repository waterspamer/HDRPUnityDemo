using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

namespace Nettle {

    public class FramePackingCursor : MonoBehaviour {
#if !UNITY_WEBGL
        public StereoEyes Eyes;
        private bool focused = true;

        private static FramePackingCursor _instance;

        [DllImport("user32.dll")]
        public static extern bool SetCursorPos(int x, int y);
        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out Point pos);

        [StructLayout(LayoutKind.Sequential)]
        public struct Point {
            public int X;
            public int Y;
            public static implicit operator Vector2(Point p) {
                return new Vector2(p.X, p.Y);
            }
        }

#if UNITY_EDITOR
        void OnValidate() {
            HierarchyInit();
        }

        void Reset() {
            HierarchyInit();
        }

        void HierarchyInit() {
            if (!GameObjectEx.IsPrefabInAsset(gameObject)) {
                if (!Eyes) {
                    Eyes = FindObjectOfType<StereoEyes>();
                }
            }
        }
#endif
        private void OnApplicationFocus(bool focus) {
            focused = focus;
        }

        private void OnLevelWasLoaded() {
            if (!Eyes) {
                Eyes = SceneUtils.FindObjectIfSingle<StereoEyes>();
            }
        }

        // Use this for initialization
        void Start() {
            if (_instance == null) {
                _instance = this;
                if (!Eyes) {
                    Eyes = SceneUtils.FindObjectIfSingle<StereoEyes>();
                }
            }
            else {
                Destroy(this);
            }

        }

        // Update is called once per frame
        void Update() {
            if (Eyes != null && !Application.isEditor && focused) {
                var leftEyeRect = Eyes.GetCameraViewportRect(true);


                int minY = (int)(leftEyeRect.y * Screen.height);

                Point cursor;
                GetCursorPos(out cursor);

                if (cursor.Y < minY) {
                    cursor.Y = minY;
                    SetCursorPos(cursor.X, cursor.Y);
                }
            }

        }
    #endif
    }
}