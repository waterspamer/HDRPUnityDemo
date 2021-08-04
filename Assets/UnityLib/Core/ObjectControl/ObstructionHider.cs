using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nettle
{
    [RequireComponent(typeof(ObjectFadeController))]
    public class ObstructionHider : MonoBehaviour
    {
        [SerializeField]
        private Vector3 _outwardAxis = Vector3.right;
        [SerializeField]
        private Renderer[] _additionalRenderers;
        [SerializeField]
        [Tooltip("If the angle at which user looks at this object is lower than this value, the object will fade out")]
        private float _angleThreshold = 80;
        [SerializeField]
        private float _fadeSpeed = 0.5f;

        public bool EnableHiding{
            get
            {
                return _enableHiding;
            }
            set
            {
                _enableHiding = value;
                if (!_enableHiding && _fadeController!=null)
                {
                    _fadeController.FadeIn();
                }

            }
        }
        private bool _enableHiding = true;

        private ObjectFadeController _fadeController;
        

        public Vector3 OutwardAxisWorld{
            get
            {
                return transform.TransformDirection(_outwardAxis);
            }
        }        

        void Start()
        {
            _fadeController = GetComponent<ObjectFadeController>();
            _fadeController.AddRenderers(_additionalRenderers);
            _fadeController.FadeSpeed = _fadeSpeed;
        }
        
        void Update()
        {
            if (!_enableHiding)
            {
                return;
            }
            Vector3 eyesPos = StereoEyes.Instance.transform.position;
            Vector3 eyesDir = (eyesPos - transform.position);
            float angle = Vector3.Angle(eyesDir,OutwardAxisWorld);
            bool isVisible = angle > _angleThreshold;
            if (isVisible != _fadeController.IsVisible)
            {
                if (isVisible)
                {
                    _fadeController.FadeIn();
                }else
                {
                    _fadeController.FadeOut();
                }                
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, OutwardAxisWorld);
            Gizmos.color = Color.white;
        }
    }
}