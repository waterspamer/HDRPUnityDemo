using UnityEngine;
namespace Nettle {
    public class NEventToggle : NEvent {


        public NEvent predicate;
        [SerializeField]
        private bool isChecked = true;
        public bool IsChecked{
            get
            {
                return isChecked;
            }
        }


        public OnNEvent SetEvent = new OnNEvent();
        public OnNEvent ResetEvent = new OnNEvent();

        protected override bool Get() {
            return isChecked;
        }

        void Update() {
            if (predicate != null && predicate == true) {
                isChecked = !isChecked;
                InvokeEvents();
            }
        }

        public void Toggle() {
            SetToggle(!isChecked);
        }

        public void SetToggle(bool state) {
            if (isChecked != state) {
                isChecked = state;
                InvokeEvents();
            }
        }

        public void InvokeEvents() {
            if (isChecked) {
                SetEvent.Invoke();
            } else {
                ResetEvent.Invoke();
            }
        }
    }
}
