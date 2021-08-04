using UnityEngine;
using System.Collections;

namespace Nettle {

public class AnimationSwitchController : MonoBehaviour
{
    public Animator[] Targets;

    public void SwitchTrigger(string trigger)
    {
        var index = int.Parse(trigger.Split('/')[1]);
        var triggerName = trigger.Split('/')[0];

        if (Targets != null && Targets.Length > index && Targets[index].gameObject.activeInHierarchy)
            Targets[index].SetTrigger(triggerName);
    }
}
}
