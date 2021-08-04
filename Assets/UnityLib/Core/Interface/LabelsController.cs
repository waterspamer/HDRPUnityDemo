using System.Collections;
using UnityEngine;

namespace Nettle {

    [ExecuteInEditMode]
    public class LabelsController : MonoBehaviour {

        // Use this for initialization
        void Start () {

        }

        void ApplyTarget () {
            if (MotionParallaxDisplay.Instance != null) {
                Shader.SetGlobalVector ("_labelsWorldTarget", StereoEyes.Instance.transform.position);
                Shader.SetGlobalVector ("_MPD_Position", MotionParallaxDisplay.Instance.transform.position);
            }
        }

        // Update is called once per frame
        void Update () {

            //var m = Matrix4x4.TRS(Vector3.zero, Quaternion.AngleAxis(10, Vector3.up), Vector3.one);

            ApplyTarget ();
        }
    }
}