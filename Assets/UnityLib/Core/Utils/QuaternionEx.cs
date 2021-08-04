using UnityEngine;
using System.Collections;

namespace Nettle {

public static class QuaternionEx {

    public static string ToStringEx(this Quaternion v) {
        return string.Format("({0},{1},{2},{3})", v.x, v.y, v.z, v.w);
    }
}
}
