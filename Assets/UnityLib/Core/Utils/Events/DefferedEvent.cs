using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Nettle {

    public class DefferedEvent : MonoBehaviour {
        public bool Active = true;
        public UnityEvent Event = new UnityEvent();
        public float Offset = 0.0f;
        [Tooltip("Allows some deferred events to be launched after this one")]
        public List<DefferedEvent> NextEvents = new List<DefferedEvent>();
        public List<DefferedEvent> PreviousEvents { get; private set; } = new List<DefferedEvent>();
        public bool SkipOldEventsSync = false;
        private bool _isInvoking = false;
        public float TimeLeft{get; private set;}= 0;

        public void SetActive(bool active) {
            Active = active;
        }

        public void CancelAll() {
            DefferedEvent[] events = FindObjectsOfType<DefferedEvent>();
            foreach (DefferedEvent e in events) {
                e.CancelEvent();
            }
        }

        public void OnEvent() {
            if (Active)
            {
                if (Offset > 0)
                {
                    _isInvoking = true;
                    TimeLeft = Offset;
                }else
                {
                    OnTimePassed();
                }
            }
        }

        private void Awake()
        {
            foreach (DefferedEvent e in NextEvents)
            {
                e.PreviousEvents.Add(this);
            }
        }

        private void Update() {
            if (!_isInvoking) { return; }
            if (TimeLeft < 0.0f && Event != null)
            {
                OnTimePassed();
                TimeLeft = 0;
                _isInvoking = false;
                return;
            }
            TimeLeft = TimeLeft - Time.deltaTime;
        }

        private void OnTimePassed()
        {
            Event.Invoke();
            foreach (DefferedEvent e in NextEvents)
            {
                e.OnEvent();
            }
        }

        public void InvokeWithOffset(float offsetOverride)
        {        
            if (Active) {
                _isInvoking = true;
                TimeLeft = offsetOverride;
            }
        }

        public void InvokeInstantly() {
            if (Event != null) {
                Event.Invoke();
            }
        }

        public void CancelEvent() {
            TimeLeft = 0;
            _isInvoking = false;
        }
    }
}
