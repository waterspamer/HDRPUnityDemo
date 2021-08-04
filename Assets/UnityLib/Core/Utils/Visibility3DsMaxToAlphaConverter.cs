using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Nettle
{
    public class Visibility3DsMaxToAlphaConverter : MonoBehaviour
    {

        [Serializable]
        public class CurveData
        {

            public AnimationClip Clip;
            public Renderer Renderer;
            public bool HasVisibilityControl = false;
            public VisibilityControl Visibility;
            public AnimationCurve Curve;
        }


        public Animator Animator;

        [HideInInspector]
        [SerializeField]
        private List<CurveData> _curvesData;
        [SerializeField]
        private bool _forceEnableRenderers = true;



        void LateUpdate()
        {
            foreach (var data in _curvesData)
            {
                if (Animator.GetCurrentAnimatorClipInfo(0)[0].clip == data.Clip)
                {
                    Color color = data.Renderer.material.color;
                    color.a = data.Curve.Evaluate(Animator.GetCurrentAnimatorStateInfo(0).normalizedTime * Animator.GetCurrentAnimatorClipInfo(0)[0].clip.length);
                    data.Renderer.material.color = color;
                    if (_forceEnableRenderers)
                    {
                        if (data.HasVisibilityControl)
                        {
                            data.Visibility.ApplyVisibility();
                        }
                        else
                        {
                            data.Renderer.enabled = true;
                        }
                    }
                }
            }
        }


#if UNITY_EDITOR

        [SerializeField]
        private List<Renderer> _excludedRenderers = new List<Renderer>();

        void Reset()
        {
            EditorInit();
        }

        void OnValidate()
        {
            EditorInit();
        }

        void EditorInit()
        {
            if (!Animator)
            {
                Animator = GetComponent<Animator>();
            }

            _curvesData = new List<CurveData>();
            foreach (var clip in Animator.runtimeAnimatorController.animationClips)
            {
                foreach (var binding in AnimationUtility.GetCurveBindings(clip))
                {
                    if (binding.propertyName == "m_Enabled")
                    {
                        Renderer renderer = AnimationUtility.GetAnimatedObject(gameObject, binding) as Renderer;
                        if (renderer == null)
                        {
                            Debug.Log("renderer == null");
                            continue;
                        }
                        if (_excludedRenderers.Contains(renderer))
                        {
                            continue;
                        }

                        CurveData curveData = new CurveData()
                        {
                            Clip = clip,
                            Curve = AnimationUtility.GetEditorCurve(clip, binding),
                            Renderer = renderer
                        };
                        VisibilityControl vc = renderer.GetComponent<VisibilityControl>();
                        if (vc != null)
                        {
                            curveData.HasVisibilityControl = true;
                            curveData.Visibility = vc;
                        }
                        _curvesData.Add(curveData);

                    }
                }
            }
        }
#endif
    }
}