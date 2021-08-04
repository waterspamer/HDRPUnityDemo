using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Nettle {

public class LanguageUTextController : LanguageTextController {
    [SerializeField]
    private Text _text;
    protected override void Reset() {
        base.Reset();
        _text = GetComponent<Text>();
    }

    protected override void ApplyTextChange(string text) {
        _text.text = text;
    }
}
}
