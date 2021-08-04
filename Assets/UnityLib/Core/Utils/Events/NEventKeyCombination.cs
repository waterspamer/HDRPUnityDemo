using UnityEngine;

namespace Nettle {

public class NEventKeyCombination : NEvent {

    public KeyCode[] Keys;
    public OnNEvent OnKeyEvent = new OnNEvent();
    
    void Start () {
        HotkeyChecker.AddHotkey(Callback, Keys);
	}

    private void Callback() {
        if (enabled && OnKeyEvent!=null) {
            OnKeyEvent.Invoke();
        }
    }

    protected override bool Get() {
        return HotkeyChecker.IsHotkeyPressed(Keys);
    }

    private void OnDestroy() {
        HotkeyChecker.RemoveCallback(Callback);
    }
}
}
