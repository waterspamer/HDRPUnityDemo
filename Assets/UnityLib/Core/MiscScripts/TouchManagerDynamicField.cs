using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nettle
{
    [RequireComponent(typeof(RectTransform))]
    public class TouchManagerDynamicField : MonoBehaviour
    {
        [SerializeField]
        private TouchManager _manager;

        private void Reset()
        {
            _manager = FindObjectOfType<TouchManager>();
        }

        private void OnEnable()
        {
            _manager.FieldsOfTouch.Add(transform as RectTransform);
        }

        private void OnDisable()
        {            
            _manager.FieldsOfTouch.Remove(transform as RectTransform);
        }
    }
}