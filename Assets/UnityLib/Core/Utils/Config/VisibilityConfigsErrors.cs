using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace Nettle {

public class VisibilityConfigsErrors : MonoBehaviour {
    public bool DrawErrors = true;
    public RectTransform ConfigsErrorsPanel;
    public ConfigsController configsController;
    public Text ErrorText;
    // Use this for initialization
    void Start() {
        if (DrawErrors && configsController.GetErrors() != null) {
            ConfigsErrorsPanel.gameObject.SetActive(true);
            Time.timeScale = 0.0f;
            SetErrorText(configsController.GetErrors());
        }
    }

    // Update is called once per frame
    void Update() {

    }

    public void SetErrorText(string errors) {
        ErrorText.text = errors;
    }
}
}
