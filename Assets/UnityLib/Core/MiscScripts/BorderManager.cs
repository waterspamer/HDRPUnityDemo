using UnityEngine;
using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;

namespace Nettle.Core {

    public class BorderManager : MonoBehaviour {
        public VisibilityZoneViewer Viewer;
        [Tooltip("If not empty, borders will only appear for zones that match the regular expression")]
        public string RegularExpression = "@";

        private void Reset() {
            if (Viewer == null) {
                Viewer = SceneUtils.FindObjectIfSingle<VisibilityZoneViewer>();                
            }
        }

        private Renderer _rendererToDisable;
        private Renderer _activeRenderer;

        private void Start() {
            if (Viewer != null) {
                Viewer.OnShowZone.AddListener(OnShowZone);
                Viewer.TransitionEnd.AddListener(OnTransitionEnd);
                foreach (VisibilityZone zone in Viewer.Zones) {
                    if (zone.AttachedRenderer != null) {
                        zone.AttachedRenderer.enabled = false;
                    }
                }
            }
        }

        private void OnShowZone(VisibilityZone zone)
        {
            if (_activeRenderer != null)
            {
                if (_rendererToDisable != null)
                {
                    _rendererToDisable.enabled = false;
                }
                _rendererToDisable = _activeRenderer;
            }

            if (string.IsNullOrEmpty(RegularExpression) || Regex.Match(zone.name, RegularExpression).Success)
            {
                _activeRenderer = zone.AttachedRenderer;
            }else
            {
                _activeRenderer = null;
            }
        }

        private void OnTransitionEnd() {
            if (_rendererToDisable!=null) {
                _rendererToDisable.enabled = false;
            }
            if (_activeRenderer != null) {
                _activeRenderer.enabled = true;
            }
        }
    }
}
