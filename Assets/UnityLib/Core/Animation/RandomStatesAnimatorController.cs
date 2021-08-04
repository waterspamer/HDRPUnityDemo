using UnityEngine;

namespace Nettle {

public class RandomStatesAnimatorController : MonoBehaviour {

    public Animator Animator;
    public State[] States;
    private int _currentState;

    private void Reset() {
        if (Animator == null) {
            Animator = GetComponentInChildren<Animator>();
        }

    }

    [System.Serializable]
    public class State {
        public float minTime = 5;
        public float maxTime = 10;
        public bool allowSwitchToAll = true;
        public int[] allowSwitchToState;
    }

    void Awake() {
        for (int i = 0; i < States.Length; i++) {
            if (States[i].allowSwitchToAll) {
                States[i].allowSwitchToState = new int[States.Length - 1];
                int k = 0;
                for (int j = 0; j < States.Length; j++) {
                    if (i != j) {
                        States[i].allowSwitchToState[k] = j;
                        k++;
                    }
                }
            }
        }
    }


    void OnEnable() {
        NextState();
    }

    void OnDisable() {
        CancelInvoke("NextState");
    }

    //TODO: Check if gameobject active. Simple check shall NOT work
    void NextState() {
        int r = Random.Range(0, States[_currentState].allowSwitchToState.Length);
        _currentState = States[_currentState].allowSwitchToState[r];
        Animator.SetInteger("State", _currentState);
        float time = Random.Range(States[_currentState].minTime, States[_currentState].maxTime);
        Invoke("NextState", time);
    }
}
}
