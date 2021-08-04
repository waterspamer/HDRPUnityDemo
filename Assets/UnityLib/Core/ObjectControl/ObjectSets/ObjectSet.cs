using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EasyButtons;

namespace Nettle {
    public class ObjectSet : MonoBehaviour
    {
        [SerializeField]
        private List<GameObject> _objects;

        private void OnEnable()
        {
            ToggleAllObjects(true);
        }

        private void OnDisable()
        {
            ToggleAllObjects(false);
        }
        
        [Button()]
        public void EnableInEditor()
        {
            gameObject.SetActive(true);
            ToggleAllObjects(true);
        }
        [Button()]
        public void DisableInEditor()
        {
            gameObject.SetActive(false);
            ToggleAllObjects(false);
        }

        private void ToggleAllObjects(bool on)
        {
            foreach (GameObject o in _objects)
            {
                if (o != null)
                {

#if UNITY_EDITOR
                    if (!Application.isPlaying)
                    {
                        UnityEditor.Undo.RecordObject(o, "Toggle object set " + (on ? "on" : "off"));
                    }
#endif
                    o.SetActive(on);
                }
            }
        }
    }
}