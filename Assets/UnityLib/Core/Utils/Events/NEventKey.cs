using UnityEngine;

namespace Nettle {

    public class NEventKey : NEvent {

        public static bool IsGlobalActive = true;
        
        public KeyCode Key;
        public enum KeyDirection { Down, Up, Pressed };
        public KeyDirection Direction = KeyDirection.Up;


        public bool Alt = false;
        public bool Ctrl = false;
        public bool Shift = false;

        public OnNEvent OnKeyEvent = new OnNEvent();

        public bool IgnoreGlobalDeactivation = false;
        private bool _state;

        private bool GetState() {
            if (LoadScreenManager.LoadingInProgress || (!IsGlobalActive && !IgnoreGlobalDeactivation)) {
                return false;
            }
            if (Alt != (Input.GetKey(KeyCode.LeftAlt) | Input.GetKey(KeyCode.RightAlt))) return false;
            if (Ctrl != (Input.GetKey(KeyCode.LeftControl) | Input.GetKey(KeyCode.RightControl))) return false;
            if (Shift != (Input.GetKey(KeyCode.LeftShift) | Input.GetKey(KeyCode.RightShift))) return false;

            if (Direction == KeyDirection.Up) {
                if (Input.GetKeyUp(Key)) {
                    Invoke();
                    return true;
                }
                return false;
            } else if (Direction == KeyDirection.Down) {
                if (Input.GetKeyDown(Key)) {
                    Invoke();
                    return true;
                }
                return false;
            } else {
                if (Input.GetKey(Key)) {
                    Invoke();
                    return true;
                }
                return false;
            }
        }

        public void Invoke() {
            OnKeyEvent.Invoke();
        }

        private void Update() {
            _state = GetState();
        }

        protected override bool Get() {
            return _state;
        }
    }
}
