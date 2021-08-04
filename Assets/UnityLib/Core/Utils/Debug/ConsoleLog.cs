using UnityEngine;
using System.Collections;

namespace Nettle {

public class ConsoleLog : MonoBehaviour {
    public void WriteToConsole(string text) {
        if (text != null) {
            Debug.Log(text,gameObject);
        }
    }
}
}
