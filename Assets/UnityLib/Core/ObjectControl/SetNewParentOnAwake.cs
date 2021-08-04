using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetNewParentOnAwake : MonoBehaviour
{
    public Transform NewParent;
    private void Start() {
        transform.SetParent(NewParent);
        Destroy(this);
    }
}
