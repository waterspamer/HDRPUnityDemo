using System;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

namespace Nettle {

[Serializable]
public class LanguageTextMeshSettings {
    public Language Lang;
    [TextArea]
    public string Text;
}

public abstract class LanguageTextController : MonoBehaviour {

    public LanguageController Manager;
    public LanguageTextMeshSettings[] Settings;

    protected virtual void Reset() {
        if (!Manager) {
            Manager = SceneUtils.FindObjectIfSingle<LanguageController>();
        }        
    }

    private void Start() {
        if (!Manager && Manager.OnLanguageChangedEvent == null) { return; }
        SetText(Manager.GetCurrentLanguage());
        Manager.OnLanguageChangedEvent.AddListener(SetText);
    }

    public void UpdateLanguage()
    {
        SetText(Manager.Lang);
    }
    private void SetText(Language lang) {
        
        var textSetting = Settings.FirstOrDefault(v => v.Lang == lang);
        if (textSetting != null) {
            ApplyTextChange(textSetting.Text);
        }
    }

    protected abstract void ApplyTextChange(string text);
}
}
