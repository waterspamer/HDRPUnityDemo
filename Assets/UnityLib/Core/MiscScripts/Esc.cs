using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

namespace Nettle
{
    public interface iApplicationQuitBlocker
    {
        bool ReadyToQuit();
    }

    public class Esc : MonoBehaviour
    {
        public KeyCode Key;
        public bool Ctrl;
        public bool Alt;
        public bool Shift;
        public UnityEvent OnQuitHotkey;

        private bool _isQuitting = false;

        private List<iApplicationQuitBlocker> _blockers = new List<iApplicationQuitBlocker>();

        private bool AllBlockersReady
        {
            get
            {
                foreach (iApplicationQuitBlocker blocker in _blockers)
                {
                    if (blocker == null)
                    {
                        continue;
                    }                    
                    if (!blocker.ReadyToQuit())
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public void AddQuitBlocker(iApplicationQuitBlocker blocker)
        {
            _blockers.Add(blocker);
        }
        public void RemoveQuitBlocker(iApplicationQuitBlocker blocker)
        {
            _blockers.Remove(blocker);
        }

        void Update()
        {
            if (_isQuitting)
            {
                return;
            }
            if (Alt)
                if (!(Input.GetKey(KeyCode.LeftAlt) | Input.GetKey(KeyCode.RightAlt))) return;
            if (Ctrl)
                if (!(Input.GetKey(KeyCode.LeftControl) | Input.GetKey(KeyCode.RightControl))) return;
            if (Shift)
                if (!(Input.GetKey(KeyCode.LeftShift) | Input.GetKey(KeyCode.RightShift))) return;

            if (Input.GetKeyUp(Key))
            {
                _isQuitting = true;
                if (OnQuitHotkey != null)
                {
                    OnQuitHotkey.Invoke();
                }
                StartCoroutine(DelayedQuit());
            }
        }

        private IEnumerator DelayedQuit()
        {
            yield return new WaitUntil(() => AllBlockersReady);
            Quit();
        }

        public void Quit()
        {
            if (Application.platform != RuntimePlatform.WindowsEditor)
            {
                System.Diagnostics.Process.GetCurrentProcess().Kill();
            }
            //Application.Quit();
        }
    }
}
