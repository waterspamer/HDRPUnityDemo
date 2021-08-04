using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

namespace Nettle {

    public class AnimatorHelper : MonoBehaviour {

        [Serializable]
        public class CustomAnimationEvent {
            public string Clip;
            public float Time;
            public UnityEvent Event;
        }

        public Animator Animator;
        public string StateName = "";       
        public bool PlayAtStartUp = true;

        private float _lastSpeed;
        [HideInInspector]
        public List<CustomAnimationEvent> Events = new List<CustomAnimationEvent>();


        private void Awake() {
            if (!PlayAtStartUp) {
                Stop();
            }
        }

        void Start() {            
            for (int i = 0; i < Events.Count; i++) {                
                AddEvent(Events[i]);
            }
        }

        void Reset() {
            if (Animator == null) {
                Animator = GetComponent<Animator>();
            }
        }

        public void Play() {
            Animator.Play(StateName, 0);
            SpeedPlay();
        }

        public void Play(string state) {
            SetState(state);
            Animator.Play(StateName, 0);
            SpeedPlay();
        }

        public void PlayAtTime(float time) {
            Animator.Play(StateName, 0, NormalizeTime(time));
            SpeedPlay();
        }

        public void PlayAtNormalizedTime(float time) {
            Animator.Play(StateName, 0, time);
            SpeedPlay();
        }

        public void PlayAtFrame(int frame) {
            Animator.Play(StateName, 0, FrameToNormalizeTime(frame));
            SpeedPlay();
        }

        public void Restart(string state) {
            SetState(state);
            Restart();
        }

        public void Restart() {
            Animator.Play(StateName, 0, 0);
            SpeedPlay();
        }

        public void PlayOrRestart(string state) {
            SetState(state);
            AnimatorStateInfo stateInfo = Animator.GetCurrentAnimatorStateInfo(0);
            if (!stateInfo.loop && stateInfo.normalizedTime >= 1) {
                Restart();
            } else {
                Play();
            }
        }

        public void Pause() {
            SpeedStop();
        }

        public void PauseAtTime(float time) {
            Animator.Play(StateName, 0, NormalizeTime(time));
            SpeedStop();
        }

        public void PauseAtNormalizedTime(float time) {
            Animator.Play(StateName, 0, time);
            SpeedStop();
        }

        public void PauseAtFrame(int frame) {
            Animator.Play(StateName, 0, FrameToNormalizeTime(frame));
            SpeedStop();
        }

        public void Stop() {
            Animator.Play(StateName, 0, 0);
            SpeedStop();
        }


        private void SpeedPlay() {
            if (Animator.speed == 0) {
                Animator.speed = _lastSpeed;
            }
        }

        private void SpeedStop() {
            if (Animator.speed != 0) {
                _lastSpeed = Animator.speed;
                Animator.speed = 0;
            }
        }

        public void SetState(string stateName) {
            StateName = stateName;
        }

        public void SetSpeed(float speed) {
            _lastSpeed = speed;
            if (Animator.speed != 0) {
                Animator.speed = speed;
            }
        }

        public void SetBoolOn(string name) {
            Animator.SetBool(name, true);
        }

        public void SetBoolOff(string name) {
            Animator.SetBool(name, false);
        }

        public void AddEvent(CustomAnimationEvent animEvent) {
            AnimationClip clip = Animator.runtimeAnimatorController.animationClips.First(v => v.name == animEvent.Clip);
            foreach(AnimationEvent existingEvent in clip.events) {
                if (existingEvent.time == animEvent.Time) {
                    return;
                }
            }


            AnimationEvent animationEvent = new AnimationEvent {
                functionName = "AnimationEventCallback",
                time = animEvent.Time
            };

            clip.AddEvent(animationEvent);
        }

        public void AnimationEventCallback(AnimationEvent animationEvent) {
            foreach (CustomAnimationEvent customEvent in Events) {
                if (customEvent.Clip == animationEvent.animatorClipInfo.clip.name && customEvent.Time == animationEvent.time) {                    
                    customEvent.Event.Invoke();
                    return;
                }
            }
        }

        public void SetDirection(bool forward) {
            Animator.SetFloat("SpeedMultiplier", forward ? 1 : -1);
        }

        private float NormalizeTime(float time) {
            return time / Animator.GetCurrentAnimatorClipInfo(0)[0].clip.length;            
        }

        private float FrameToNormalizeTime(int frame) {
            var clip = Animator.GetCurrentAnimatorClipInfo(0)[0].clip;
            float frames = clip.frameRate * clip.length;
            return frame / frames;
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(AnimatorHelper))]
    public class AnimatorHelperEditor : Editor {
        private ReorderableList _list;

        private AnimatorHelper _source;
        

        private void OnEnable() {
            _source = target as AnimatorHelper;
            _list = new ReorderableList(serializedObject, serializedObject.FindProperty("Events"), true, true, true, true);

            _list.drawElementCallback = (rect, index, isActive, isFocused) => {
                if (_source.Animator == null) {
                    return;
                }

                var clips = _source.Animator.runtimeAnimatorController.animationClips.Select(v => v.name).ToArray();

                var element = _list.serializedProperty.GetArrayElementAtIndex(index);
                rect.y += 2;

                EditorGUI.LabelField(new Rect(rect.x, rect.y, 20f, EditorGUIUtility.singleLineHeight), index.ToString());

                EditorGUI.LabelField(new Rect(rect.x + 20f, rect.y, 30f, EditorGUIUtility.singleLineHeight), "Clip");
                int indexInClips = Array.IndexOf(clips, element.FindPropertyRelative("Clip").stringValue);
                int newIndex = EditorGUI.Popup(new Rect(rect.x + 55f, rect.y, 200f, EditorGUIUtility.singleLineHeight), indexInClips, clips);
                if (newIndex >= 0) {
                    element.FindPropertyRelative("Clip").stringValue = clips[newIndex];
                }

                EditorGUI.LabelField(new Rect(rect.x + 20, rect.y + EditorGUIUtility.singleLineHeight + 3, 35f, EditorGUIUtility.singleLineHeight), "Time");
                EditorGUI.PropertyField(new Rect(rect.x + 55, rect.y + EditorGUIUtility.singleLineHeight + 3, 50, EditorGUIUtility.singleLineHeight),
                    element.FindPropertyRelative("Time"), GUIContent.none);

                EditorGUI.PropertyField(new Rect(rect.x, rect.y + 2 * EditorGUIUtility.singleLineHeight + 6, rect.xMax - rect.x, EditorGUIUtility.singleLineHeight),
                    element.FindPropertyRelative("Event"), GUIContent.none);
            };


            _list.drawHeaderCallback = rect => {
                EditorGUI.LabelField(new Rect(rect.x, rect.y, 100f, EditorGUIUtility.singleLineHeight), "Events");
            };


            
            _list.elementHeightCallback = index => {
                int eventCount = _source.Events[index].Event.GetPersistentEventCount();
                return EditorGUIUtility.singleLineHeight + 110 + (eventCount >= 1 ? eventCount - 1 : 0) * 43;
            };

        }


        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            serializedObject.Update();
            _list.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
