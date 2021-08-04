using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nettle {

public class CursorProxy : MonoBehaviour {

        public bool ShowCursorOnStart = false;
        public int StartId = -1;

        public void Start() {
            if (ShowCursorOnStart) {
                Cursor.visible = true;
            }
            if (StartId >= 0) {
                SetCursor(StartId);
            }
        }

        public void SetCursor(int id) {
        if (ShowCursor.Instance != null) {
            ShowCursor.Instance.SetCursor(id);
        }
    }

    public void SetCursor(string name) {
        if (ShowCursor.Instance != null) {
            ShowCursor.Instance.SetCursor(name);
        }
    }

        public void Show(bool show) {
            Cursor.visible = show;
        }
}
}
