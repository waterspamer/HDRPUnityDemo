using UnityEngine;
using System.Collections;

namespace Nettle {

public class TimelapseProgressViewController : MonoBehaviour {

    public RectTransform Progress;

    public Timelapse Tl;

    private void Update() {
        if (Tl != null && Progress != null) {
            float scale = (float) ((Tl.CurrenTime - Tl.StartDate.ToDateTime()).Ticks) /
                          (float) ((Tl.EndDate.ToDateTime() - Tl.StartDate.ToDateTime()).Ticks);
            Progress.localScale = new Vector3(scale, Progress.localScale.y, Progress.localScale.z);
        }
    }
}
}
