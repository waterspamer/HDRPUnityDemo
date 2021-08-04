using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nettle
{
    public class ShowObjectByViewDirection : MonoBehaviour
    {

        public GameObject FrontViewObject;
        public GameObject BackViewObject;
        public Vector3 LocalFrontAxis = Vector3.forward;
        public Transform Target;

        private void Start()
        {
            if (Target == null)
            {
                Target = StereoEyes.Instance.transform;
            }
        }

        void Update()
        {
            Vector3 worldFrontAxis = transform.TransformDirection(LocalFrontAxis);
            Vector3 targetDirection = Target.position - transform.position;
            bool targetInFront = Vector3.Dot(worldFrontAxis, targetDirection) > 0;
            FrontViewObject.SetActive(targetInFront);
            BackViewObject.SetActive(!targetInFront);
        }
    }
}