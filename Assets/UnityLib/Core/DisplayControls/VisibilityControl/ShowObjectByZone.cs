using System.Collections.Generic;
using UnityEngine;
using System.Collections;

namespace Nettle {

public class ShowObjectByZone : MonoBehaviour {
    public VisibilityZone Target;
    public VisibilityZoneViewer Viewer;

    public bool IsShow;

    public List<GameObject> Objects = new List<GameObject>();

    void Start() {
        if (Viewer != null) {
            Viewer.OnShowZone.AddListener((zone) => {
                if (Target != null) {
                    if (Target.name == zone.name) {
                        if (IsShow)
                            Show();
                        else
                            Hide();
                    } else {
                        if (IsShow)
                            Hide();
                        else
                            Show();
                    }
                }
            });
        }
    }

    public void Show() {
        foreach (var obj in Objects) {
            obj.SetActive(true);
        }
    }

    public void Hide() {
        foreach (var obj in Objects) {
            obj.SetActive(false);
        }
    }
}
}
