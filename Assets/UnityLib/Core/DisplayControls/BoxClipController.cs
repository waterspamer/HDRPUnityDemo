using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nettle {

    public enum BoxClipState {
        Off, Hard, Soft
    }

    public static class BoxClipController {

        private static BoxClipState _state = BoxClipState.Off;

        public static BoxClipState State {
            get {
                return _state;
            }

            set {
                if (_state == value) {
                    return;
                }
                _state = value;
                if (_state == BoxClipState.Off) {
                    Shader.DisableKeyword("BOX_CLIP_HARD");
                    Shader.DisableKeyword("BOX_CLIP_SOFT");
                    Shader.EnableKeyword("BOX_CLIP_OFF");
                }
                else if (_state == BoxClipState.Hard) {
                    Shader.DisableKeyword("BOX_CLIP_OFF");
                    Shader.DisableKeyword("BOX_CLIP_SOFT");
                    Shader.EnableKeyword("BOX_CLIP_HARD");
                }
                else if (_state == BoxClipState.Soft) {
                    Shader.DisableKeyword("BOX_CLIP_OFF");
                    Shader.DisableKeyword("BOX_CLIP_HARD");
                    Shader.EnableKeyword("BOX_CLIP_SOFT");
                }
            }
        }

        public static void SetClipTransform(Transform transform) {
            Shader.SetGlobalMatrix("mp3dWorldToLocal", transform.worldToLocalMatrix);
        }

        public static void SetSoftClipSize(float size) {
            Shader.SetGlobalFloat("_BoxClipSmoothSize", size);
        }                 
    }
}
