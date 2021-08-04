using System;
using UnityEngine;
using System.Collections;

namespace Nettle {

    public class MotionParallaxLOD : MonoBehaviour {

        [System.Serializable]
        public class LOD {
            public GameObject[] GameObjects;
            public float DisplayScale;
            [HideInInspector]
            public float PixelsPerUnitToHide;
        }

        public LOD[] LODs;
        public int CurrentID { get; private set; }
        public int LastID { get; private set; }
        private bool _initialized = false;
        private bool _disableAllLODs = false;

#if UNITY_EDITOR
        [HideInInspector]
        public int OverrideLevel = -1;

        private void Awake() {
            OverrideLevel = -1;
        }
#endif

        private void OnValidate() {
            if (LODs != null) {
                foreach (var lod in LODs) {
                    if (lod.PixelsPerUnitToHide > 0 && Math.Abs(lod.DisplayScale) < 0.0001f) {
                        lod.DisplayScale = 960 / lod.PixelsPerUnitToHide;
                    }
                }
            }

        }

        private void Start() {
            //LastID = CurrentID = 0;
        }

        private void OnEnable() {
            if (_initialized) {
                StartCoroutine(UpdateCoroutine());
            } else {
                StartCoroutine(Initialize());
            }
        }

        private IEnumerator Initialize() {
            for (int i = 0; i < LODs.Length; i++) {
                foreach (var o in LODs[i].GameObjects) {
                    if (o != null) {
                        o.SetActive(true);
                    }
                }
            }
            yield return null;
            _initialized = true;
            LastID = -1;
            StartCoroutine(UpdateCoroutine());
        }


        private IEnumerator UpdateCoroutine() {
            while (true) {
                //if (MotionParallaxDisplay.Instance == null) { yield break; }

                //float pixelsPerUnit = Display.PixelsPerUnit();
                float displayScale = MotionParallaxDisplay.Instance.transform.localScale.x;
                float maxDisplayScale = float.MaxValue;

                CurrentID = LODs.Length - 1;
                for (int i = 0; i < LODs.Length; i++) {
                    if (LODs[i].DisplayScale > displayScale) {
                        if (LODs[i].DisplayScale < maxDisplayScale) {
                            maxDisplayScale = LODs[i].DisplayScale;
                            CurrentID = i;
                        }
                    }
                }

#if UNITY_EDITOR
                if (OverrideLevel >= 0) {
                    CurrentID = OverrideLevel;
                }
#endif
                if (LastID != CurrentID)
                {
                    LastID = CurrentID;

                    for (int i = 0; i < LODs.Length; i++)
                    {
                        foreach (var o in LODs[i].GameObjects)
                        {
                            if (o != null)
                            {
                                o.SetActive(false);
                            }
                        }
                    }

                    if (!_disableAllLODs && CurrentID>=0)
                    {
                        foreach (var o in LODs[CurrentID].GameObjects)
                        {
                            o.SetActive(true);
                        }
                    }

                }
                yield return null;
            }
        }

        public void SetLODActivity(bool enabled) {
            _disableAllLODs = !enabled;
        }
    }
}
