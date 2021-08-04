using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Serialization;

namespace Nettle {
    [ExecuteAfter(typeof(ZoomPan))]
    public class ZoomPanZoneController : MonoBehaviour {
        public ZoomPan ZoomPan;
        public MotionParallaxDisplay Display;
        public VisibilityZone GlobalBoundsSource;
        public VisibilityZone ViewZone;
        [SerializeField]
        [FormerlySerializedAs("UseGlobalBounds")]
        private bool _useGlobalBounds = false;
        [SerializeField]
        [FormerlySerializedAs("OnCorrectViewZone")]
        private bool _onCorrectViewZone = true;
        [SerializeField]
        [FormerlySerializedAs("CompensateDisplayRotation")]
        private bool _compensateDisplayRotation = true;
        private Vector3[] _displayLocalCorners;
        private Vector3[] _zoneLocalCorners;

        public bool UseGlobalBounds { get => _useGlobalBounds; set => _useGlobalBounds = value; }
        public bool OnCorrectViewZone { get => _onCorrectViewZone; set => _onCorrectViewZone = value; }
        public bool CompensateDisplayRotation { get => _compensateDisplayRotation; set => _compensateDisplayRotation = value; }

        void Reset() {
            ZoomPan = SceneUtils.FindObjectIfSingle<ZoomPan>();
        }

        private void Start() {
            if (UseGlobalBounds && !GlobalBoundsSource) {
                Debug.LogWarning("GlobalBoundsSource is not set!");
            }
        }

        void LateUpdate() {
            CorrectView();
        }

        public static void GetAABB(Vector3[] src, out Vector3 bMin, out Vector3 bMax) {
            bMin = src[0];
            bMax = src[0];
            for (var i = 1; i < src.Length; ++i) {
                bMin = Vector3.Min(bMin, src[i]);
                bMax = Vector3.Max(bMax, src[i]);
            }
        }

        void Correct(VisibilityZone zone) {
            if (zone == null) {
                return;
            }
            NTransform zoneTransform = new NTransform(zone.transform);
            if (CompensateDisplayRotation) {
                Quaternion displayRotationDelta = Quaternion.FromToRotation(zone.transform.forward, Display.transform.forward);
                zoneTransform.Rotation = displayRotationDelta* zoneTransform.Rotation;
            }

            Display.GetWorldScreenCorners(out _displayLocalCorners);
            _displayLocalCorners = _displayLocalCorners.Select(v => zoneTransform.InverseTransformPoint(v)).ToArray();

            _zoneLocalCorners = VisibilityZone.GetLocalSpaceRect(zone.GetAspect(), zone.MaxZoom);

            GetAABB(_zoneLocalCorners, out Vector3 panBoundsMin, out Vector3 panBoundsMax);

            GetAABB(_displayLocalCorners.ToArray(), out Vector3 displayBoundsMin, out Vector3 displayBoundsMax);
            var displaySize = displayBoundsMax - displayBoundsMin;
            var viewZoneSize = panBoundsMax - panBoundsMin;

            if ((displaySize.x > (viewZoneSize.x + 0.0001f)) || (displaySize.z > (viewZoneSize.z + 0.0001f))) {
#if UNITY_EDITOR
                Debug.LogWarningFormat("ZoomPan: Mp3d is bigger than desired view zone!::displaySize x={0}, z={1}::viewZoneSize x={2}, z={3}", displaySize.x, displaySize.z, viewZoneSize.x, viewZoneSize.z);
#endif
            } else {
                var dirRight = Vector3.right;
                var dirForward = Vector3.forward;
                var correctionDelta = new Vector3();
                if (displayBoundsMax.z > panBoundsMax.z) {
                    correctionDelta += -dirForward * (displayBoundsMax.z - panBoundsMax.z);
                }
                if (displayBoundsMax.x > panBoundsMax.x) {
                    correctionDelta += -dirRight * (displayBoundsMax.x - panBoundsMax.x);
                }
                if (displayBoundsMin.z < panBoundsMin.z) {
                    correctionDelta += dirForward * (panBoundsMin.z - displayBoundsMin.z);
                }
                if (displayBoundsMin.x < panBoundsMin.x) {
                    correctionDelta += dirRight * (panBoundsMin.x - displayBoundsMin.x);
                }

                var worldOffset = zoneTransform.TransformVector(correctionDelta);
                Display.transform.position += worldOffset;
            }
        }

        void CorrectView() {
            if (!ZoomPan || ZoomPan.Display == null || ViewZone == null || !ZoomPan.enabled) {
                return;
            }        
            ZoomPan.MinZoom = ViewZone.MinZoom;

            if (ViewZone.ZoomPanInside) {
                ZoomPan.MaxZoom = ViewZone.MaxZoom;
                Correct(ViewZone);
            } else if (UseGlobalBounds && GlobalBoundsSource != null) {
                ZoomPan.MaxZoom = GlobalBoundsSource.MaxZoom * GlobalBoundsSource.transform.lossyScale.x / ViewZone.transform.lossyScale.x;
                Correct(GlobalBoundsSource);
            }
        }
    }
}
