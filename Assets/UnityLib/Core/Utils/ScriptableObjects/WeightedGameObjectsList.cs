using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Nettle
{

    [CreateAssetMenu(fileName = "WeightedGameObjectsList", menuName = "Nettle/WeightedGameObjectsList")]
    public class WeightedGameObjectsList : GameObjectList
    {
        private const float _defaultWeight = 1;
        [System.Serializable]
        private class ObjectWeight
        {
            public GameObject Object;
            public float Weight;
        }
        [SerializeField]
        private List<ObjectWeight> Weights = new List<ObjectWeight>();
        public void SetObjectWeight(GameObject obj, float weight)
        {
            ObjectWeight objWeight = Weights.Find(x => x.Object == obj);
            if (objWeight == null)
            {
                objWeight = new ObjectWeight() { Object = obj };
                Weights.Add(objWeight);
            }
            objWeight.Weight = weight;
        }
        public float GetObjectWeight(GameObject obj)
        {            
            ObjectWeight objWeight = Weights.Find(x => x.Object == obj);
            if (objWeight != null)
            {
                return objWeight.Weight;
            }
            else
            {
                return _defaultWeight;
            }
        }

        public void RemoveAt(int i)
        {
            if (Elements[i] == null)
            {
                Elements.RemoveAt(i);
            }else
            {
                Remove(Elements[i]);
            }
        }

        public void Remove(GameObject obj)
        {
            Elements.Remove(obj);
            ObjectWeight weight = Weights.Find(x => x.Object == obj);
            if (weight != null)
            {
                Weights.Remove(weight);
            }
        }

        public override GameObject GetRandomObject()
        {
            float[] weights = new float[Elements.Count];
            for (int i = 0; i < Elements.Count; i++)
            {
                weights[i] = GetObjectWeight(Elements[i]);
            }
            return Elements[RandomEx.Weight(weights)];
        }
    }
#if UNITY_EDITOR
    [CustomEditor(typeof(WeightedGameObjectsList))]
    public class WeightedGameObjectsListEditor : Editor{

        WeightedGameObjectsList _target;
        void OnEnable()
        {
            _target = target as WeightedGameObjectsList;
        }

        public override void OnInspectorGUI()
        {
            int actionId = -1;
            if (GUILayout.Button("+",GUILayout.Width(30)))
            {
                Undo.RecordObject(_target,"Add object");
                _target.Elements.Add(null);
                EditorUtility.SetDirty (_target);
            }
            for (int i = 0; i < _target.Count; i++)
            {
                GUILayout.BeginHorizontal();
                GameObject newTarget = EditorGUILayout.ObjectField(_target[i], typeof(GameObject), false) as GameObject;
                if (newTarget != _target[i])
                {                    
                    Undo.RecordObject(_target,"Change Object");
                    _target[i] = newTarget;
                     EditorUtility.SetDirty (_target);
                }
                EditorGUI.BeginDisabledGroup(_target[i] == null);
                    float weight = _target.GetObjectWeight(_target[i]);
                    EditorGUI.BeginChangeCheck();
                    float defaultLabelWidth = EditorGUIUtility.labelWidth;
                    EditorGUIUtility.labelWidth = 80;
                    EditorGUILayout.PrefixLabel("Weight");
                    EditorGUIUtility.labelWidth = defaultLabelWidth;
                    weight = EditorGUILayout.FloatField(weight);

                    if (EditorGUI.EndChangeCheck())
                    {
                         Undo.RecordObject(_target,"Set Object's weight");
                        _target.SetObjectWeight(_target[i], weight);
                        EditorUtility.SetDirty (_target);
                    }
                EditorGUI.EndDisabledGroup();

                if (GUILayout.Button("X",GUILayout.Width(30)))
                {
                    actionId = i;                    
                }
                GUILayout.EndHorizontal();
            }
            if (actionId > 0)
            {
                Undo.RecordObject(_target,"Remove Object");
                _target.RemoveAt(actionId);
                EditorUtility.SetDirty (_target);
            }                        
        }
    }
#endif
}