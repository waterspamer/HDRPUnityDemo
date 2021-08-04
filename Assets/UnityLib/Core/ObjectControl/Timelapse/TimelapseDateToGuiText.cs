using UnityEngine;
using System.Collections;
using System.Globalization;
using UnityEngine.UI;

namespace Nettle {

public class TimelapseDateToGuiText : MonoBehaviour {

    public Text DateText;
    public Timelapse TimelapseComp;

    private Language _language = Language.Ru;
    private CultureInfo _cultureInfo = CultureInfo.CurrentCulture;

    public void OnLangueageChanged(Language lang) {
        _language = lang;
        switch (_language) {
            case Language.Ru:
                _cultureInfo = new CultureInfo("ru-RU");
                break;
            case Language.En:
                _cultureInfo = new CultureInfo("en-US");
                break;
        }
    }

    private void Update() {
        if (DateText == null || TimelapseComp == null) {
            return;
        }

        DateText.text = TimelapseComp.CurrenTime.ToString(_cultureInfo.DateTimeFormat.LongDatePattern, _cultureInfo);
    }
}
}
