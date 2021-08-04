using UnityEngine;
using UnityEngine.Events;

namespace Nettle {

    public class StartAnimationByZone : MonoBehaviour {
        public VisibilityZone zone;
        public VisibilityZoneViewer viewer;
        public GameObject[] hideObject;

        public bool IsZoneSwitch = false;
        public bool WaitForTransitionEnd = true;

        public bool WaitAnimationEnd = false;

        public float StartTimeOffset = 0.0f;
        private float _startTimeOffset = 0.0f;

        private bool _targetZoneShowed = false;
        private bool _active = false;
        private bool _transitionComplete = false;
        private float _timeSinceActivated = 0.0f;

        public UnityEvent AnimationStarted = new UnityEvent();
        public UnityEvent AnimationStoped = new UnityEvent();

        private void Awake() {
            if (viewer != null && WaitForTransitionEnd) {
                viewer.TransitionEnd.AddListener(OnTransitionEnd);
            }
            _startTimeOffset = StartTimeOffset;
        }

        void Start() {
            if (viewer != null) {
                //viewer.TransitionEnd.AddListener(OnZoneSwitchEnd);
                viewer.OnShowZone.AddListener(OnZoneSwitchBegin);
            }

            //Play();
        }

        public void SetStarttimeOffset(float offset) {
            StartTimeOffset = offset;
        }

        public void ResetStartTimeOffset() {
            StartTimeOffset = _startTimeOffset;
        }

        private void Update() {
            if (!_targetZoneShowed) return;

            if (!WaitForTransitionEnd || (WaitForTransitionEnd && _transitionComplete)) {
                _timeSinceActivated += Time.deltaTime;
            } else {
                return;
            }

            if (_timeSinceActivated > StartTimeOffset && !_active) {
                if (hideObject != null) {
                    foreach (var obj in hideObject) obj.SetActive(true);
                }

                _active = true;
                Play();
                AnimationStarted.Invoke();
            }
        }

        public void OnTransitionEnd() {
            //Debug.Log("Anim by zone: OnTransitionEnd");
            _transitionComplete = true;
        }

        public void OnZoneSwitchBegin(VisibilityZone newZone) {
            //Debug.Log("Anim by zone: OnZoneSwitchBegin");
            if (!IsZoneSwitch) return;

            _targetZoneShowed = viewer.ActiveZone.name == zone.name;

            if (hideObject != null) {
                foreach (var obj in hideObject)
                    obj.SetActive(false);
            }

            if (_targetZoneShowed) {
                _transitionComplete = false;
                _timeSinceActivated = 0.0f;
                Reset();
                if (_active) {
                    _active = false;
                    AnimationStoped.Invoke();
                }
            } else if (!WaitAnimationEnd) {
                _targetZoneShowed = false;
                _active = false;
                Reset();
                AnimationStoped.Invoke();
            }
        }

        /*public void OnZoneSwitchEnd() {
            if (!IsZoneSwitch) return;

            if (viewer.ActiveZone.name == zone.name) {
                if (hideObject != null) {
                    foreach (var obj in hideObject)
                        obj.SetActive(true);
                }
                Reset();

            } else {
                Reset();
                AnimationStoped.Invoke();
            }
        }*/

        public void SetLayer(string nameLayer) {
            for (var i = 0; i < GetComponent<Animator>().layerCount; i++) {
                GetComponent<Animator>().SetLayerWeight(i, 0f);
            }

            var index = GetComponent<Animator>().GetLayerIndex(nameLayer);
            GetComponent<Animator>().SetLayerWeight(index, 1f);
        }

        public void SetBool(string param) {
            var strings = param.Split('/');
            GetComponent<Animator>().SetBool(strings[0], strings[1] == "true");
        }

        public void SetInt(string param) {
            var strings = param.Split('/');
            int val;
            if (strings.Length > 1 && int.TryParse(strings[1], out val)) {
                GetComponent<Animator>().SetInteger(strings[0], val);
            }
        }

        public void Reset() {
            GetComponent<Animator>().SetTrigger("Reset");
            GetComponent<Animator>().speed = 1;

            //print("Reset!");
        }

        public void Play() {
            GetComponent<Animator>().SetTrigger("Start");
            GetComponent<Animator>().speed = 1;

            //print("Play!");
        }

        public void Pause() {
            GetComponent<Animator>().speed = 0;
            //print("Pause!");
        }

        public void Stop() {
            GetComponent<Animator>().SetTrigger("Reset");
            GetComponent<Animator>().speed = 0;
        }


    }
}
