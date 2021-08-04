using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nettle
{
    public class CarPlacerPostprocessor : MonoBehaviour, IObjectPlacerPostprocessor
    {
        [SerializeField]
        private bool _enabled = true;
        [SerializeField]
        private bool _paintWithVertexColor = true;
        [SerializeField]
        private ObjectPlacerCarData _carData;
        [SerializeField]
        private bool _randomRotation = false;
        [SerializeField]
        private float _randomRotationStep = 180;

        public void PostprocessObject(GameObject newObject)
        {
            if (_randomRotation)
            {
                float randomAngle = _randomRotationStep * Random.Range(0,Mathf.RoundToInt(360/_randomRotationStep));
                GameObject pivot = new GameObject();
                Transform oldParent = newObject.transform.parent;
                pivot.transform.SetParent(oldParent,false);
                newObject.transform.SetParent(pivot.transform);
                pivot.transform.Rotate(Vector3.up * randomAngle,Space.World);
                newObject.transform.SetParent(oldParent);
                if (!Application.isPlaying)
                {
                    DestroyImmediate(pivot);
                }else
                {
                    Destroy(pivot);
                }
            }
            Color rndColor = _carData.GetRandomColor();
            _carData.PaintCar(newObject, rndColor,_paintWithVertexColor);
        }
        public bool IsEnabled()
        {
            return _enabled;
        }
    }
}