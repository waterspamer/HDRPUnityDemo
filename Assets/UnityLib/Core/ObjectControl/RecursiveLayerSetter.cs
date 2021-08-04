using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class RecursiveLayerSetter : MonoBehaviour
{
    [SerializeField]
    private Transform[] _roots;

    private List<GameObject> _children = new List<GameObject>();
    private void Start()
    {
        FindChildren();
    }

    public void FindChildren()
    {    
        if (_roots == null || _roots.Length == 0)
        {
            _roots = new Transform[] { transform };
        }
        foreach (Transform root in _roots)
        {
            _children.AddRange(root.GetComponentsInChildren<Transform>().Select(x=>x.gameObject));
        }
    }

    public void SetLayer(int id)
    {
        foreach (GameObject child in _children)
        {
            child.layer = id;
        }
    }
}
