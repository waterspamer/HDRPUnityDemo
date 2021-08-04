using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Nettle
{
    public class EventByNetworkState : MonoBehaviour
    {
       public UnityEvent OnServer;
        public UnityEvent OnClient;
        // Start is called before the first frame update
        void Start()
        {
        }
    }
}