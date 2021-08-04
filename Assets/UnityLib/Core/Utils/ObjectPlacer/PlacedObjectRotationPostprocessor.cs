using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nettle
{
    public class PlacedObjectRotationPostprocessor : MonoBehaviour,IObjectPlacerPostprocessor
    {
        [SerializeField]
        private Vector3 _rotation;
        [SerializeField]
        private Space _space = Space.Self;
        [SerializeField]
        private bool _enabled = true;

        public bool IsEnabled()
        {
            return _enabled;
        }

        public void PostprocessObject(GameObject newObject)
        {
            newObject.transform.Rotate(_rotation, _space);
        }

    }
}