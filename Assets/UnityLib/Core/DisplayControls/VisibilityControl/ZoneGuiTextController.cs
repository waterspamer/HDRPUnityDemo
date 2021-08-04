using System;
using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEngine.UI;

namespace Nettle {

[Serializable]
public class TextLanguage {
    public Language Lang;
    public string Text; 
}

[Serializable]
public class VisibilityZoneText {
    public TextLanguage[] Text;
    public VisibilityZone Zone;
}

public class ZoneGuiTextController : MonoBehaviour {

    public Text Text;
    public VisibilityZoneText[] Settings;

    private Language _currentLanguage = Language.Ru;

    private VisibilityZone _currentZone;
    private string _startText = "";

    private void Start() {
        if (Text != null) {
            _startText = Text.text;
        }
    }

    private void UpdateText() {
        if (Text != null && Settings != null && Settings.Length > 0 && _currentZone != null) {
            for (int i = 0; i < Settings.Length; ++i) {
                if (_currentZone == Settings[i].Zone) {
                    Text.text = Settings[i].Text.FirstOrDefault(v => v.Lang == _currentLanguage).Text;
                    return;
                }
            }
            Text.text = _startText;
        }
    }

	public void OnShowZone (VisibilityZone zone) {
	    _currentZone = zone;
	    UpdateText();
	}

    public void OnLanguageChanged(Language lang) {
        _currentLanguage = lang;
        UpdateText();
    }
}
}
