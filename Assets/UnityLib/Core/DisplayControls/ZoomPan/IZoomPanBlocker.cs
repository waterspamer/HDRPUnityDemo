using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IZoomPanBlocker
{
    bool IsZoomBlocked();
    bool IsPanBlocked();
}
