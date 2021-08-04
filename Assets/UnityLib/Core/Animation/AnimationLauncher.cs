using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
namespace Nettle {

    public class AnimationLauncher : MonoBehaviour {
        [System.Serializable]
        public class FloatEvent : UnityEvent<float>
        {
        
        }
        public enum AutoPlayType { None, OnStart, OnEnable }
        public AutoPlayType AutoPlay = AutoPlayType.None;
        public string AutoPlayAnimation = "";
        public bool UnfreezeAutomatically = true;
        public Animator Animator;
        [Range(0, 1)]
        public float TimeOffset = 0;

        public FloatEvent OnBeforePlay;
        public FloatEvent OnAfterPlay;

        private void OnValidate() {
            EditorInit();
        }

        private void Reset() {
            EditorInit();
        }

        private void EditorInit() {
            if (!Animator) {
                Animator = GetComponent<Animator>();
            }
        }

        public void Play()
        {
            if (!string.IsNullOrEmpty(AutoPlayAnimation))
            {
                Play(AutoPlayAnimation);
            }
        }

        public void Play(string name) {
            if (Animator != null) {
                if (OnBeforePlay != null) {
                    OnBeforePlay.Invoke(TimeOffset);
                }
                Animator.Play(name, -1, TimeOffset);
                if (OnAfterPlay != null)
                {
                    OnAfterPlay.Invoke(TimeOffset);
                }
            }
        }
        public void PlayWithOffset(float time)
        {
            if (UnfreezeAutomatically && Animator.speed == 0)
            {
                Animator.speed = 1;
            }

            if (OnBeforePlay != null)
            {
                OnBeforePlay.Invoke(time);
            }
            Animator.Play(AutoPlayAnimation, -1, time);
            if (OnAfterPlay != null)
            {
                OnAfterPlay.Invoke(time);
            }
        }

        public void PlayWithOffsetInSeconds(float seconds)
        {
            float length = 0;
            RuntimeAnimatorController ac = Animator.runtimeAnimatorController;    //Get Animator controller
            for (int i = 0; i < ac.animationClips.Length; i++)                 //For all animations
            {
                if (ac.animationClips[i].name == AutoPlayAnimation)        //If it has the same name as your clip
                {
                    length = ac.animationClips[i].length;
                }
            }
            if (length > 0)
            {
                //Debug.Log("Offset " + seconds + " is " + seconds / length + " in normalized time");
                PlayWithOffset(seconds/length);
            }else
            {
                Debug.LogWarning("Could not find the length of animation " + AutoPlayAnimation);
            }
        }

        private void Start() {
            if (AutoPlay == AutoPlayType.OnStart && !string.IsNullOrEmpty(AutoPlayAnimation)) {
                Play(AutoPlayAnimation);
            }
        }

        private void OnEnable() {
            if (AutoPlay == AutoPlayType.OnEnable && !string.IsNullOrEmpty(AutoPlayAnimation)) {
                Play(AutoPlayAnimation);
            }
        }

    }
}
