using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nettle {

public class ShowRandomObject : MonoBehaviour {
    [SerializeField]
    private GameObject[] objects;
    [SerializeField]
    private bool destroySelf = true;
    [SerializeField]
    private bool showOnStart = true;

    private void Start () {
        if (showOnStart) {
            ShowObject();
        }

        if (destroySelf) {
            Destroy(this);
        }
    }

    public void ShowObject() {
        if (objects.Length > 0) {
            int activeId = Random.Range(0, objects.Length);
                for (int i = 0; i < objects.Length; i++)
                {
                    if (objects[i] != null)
                    {
                        objects[i].SetActive(i == activeId);
                    }
                }
            }
    }
}
}
