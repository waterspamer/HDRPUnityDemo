using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Nettle {

public class HideByTag : MonoBehaviour {
    public enum TagAction { Show, Hide }

    public VisibilityManager Manager;
    public TagAction Action = TagAction.Hide;
    public List<string> Tags = new List<string>();
    
    private void Awake() {
        if (Manager == null) {
            Manager = SceneUtils.FindObjectIfSingle<VisibilityManager>();
        }
        ListenTagCheck();
    }

    private void ListenTagCheck() {
        if (Manager != null) {
            Manager.TagChanged.AddListener(OnTagChanged);
        }
    }

    private void OnDestroy() {
        if (Manager != null) {
            Manager.TagChanged.RemoveListener(OnTagChanged);
        }
    }

    private void OnTagChanged(string tag) {
        if (Tags.Contains(tag)) {
            gameObject.SetActive(Action == TagAction.Show);
            } else {
            gameObject.SetActive(Action != TagAction.Show);
            }
    }
}
}
