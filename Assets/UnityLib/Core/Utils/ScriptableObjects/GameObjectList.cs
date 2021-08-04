using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nettle {
    [CreateAssetMenu(fileName = "GameObjectList", menuName = "Nettle/GameObjectList")]
    public class GameObjectList : ScriptableObject {

        public List<GameObject> Elements=new List<GameObject>();
        public virtual GameObject GetRandomObject()
        {
            if (Elements == null || Elements.Count == 0)
            {
                return null;
            }
            return Elements[Random.Range(0, Elements.Count)];            
        }
        public int Count{
            get
            {
                return Elements.Count;
            }
        }

        public GameObject this[int id]{
            get
            {
                return Elements[id];
            }
            set
            {
                Elements[id] = value;
            }
        }

        public void Add(GameObject obj)
        {
            Elements.Add(obj);
        }
    }
}
