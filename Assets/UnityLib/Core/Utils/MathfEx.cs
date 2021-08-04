using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nettle {

public class MathfEx  {

    public static float Round(float value, int digits) {
        float mult = Mathf.Pow(10.0f, digits);
        return Mathf.Round(value * mult) / mult;
    }
}
}
