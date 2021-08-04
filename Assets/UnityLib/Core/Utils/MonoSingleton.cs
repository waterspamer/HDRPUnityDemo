using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nettle
{
    public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        private static bool _instanceIsSet = false;
        public static T Instance
        {
            get
            {
                if (!_instanceIsSet)
                {
                    _instance = FindObjectOfType<T>();
                    _instanceIsSet = _instance != null;
                    if (!_instanceIsSet)
                    {
                        Debug.LogWarning("Could not find singleton instance of type " + typeof(T).Name);
                    }
                }
                return _instance;
            }
        }

        protected static void ClearInstance()
        {
            _instanceIsSet = false;
            _instance = null;
        }        
    }
}