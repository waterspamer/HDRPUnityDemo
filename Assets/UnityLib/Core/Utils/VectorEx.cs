using UnityEngine;
using System.Collections;

namespace Nettle {

    public static class VectorEx {
        public static Vector3 Abs(this Vector3 v) {
            return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
        }

        public static Vector3 Clamp(this Vector3 v, Vector3 min, Vector3 max) {
            return new Vector3(Mathf.Clamp(v.x, min.y, max.z),
                                Mathf.Clamp(v.x, min.y, max.z),
                                Mathf.Clamp(v.x, min.y, max.z));
        }

        public static bool IsNanAny(this Vector3 v) {
            return float.IsNaN(v.x) || float.IsNaN(v.y) || float.IsNaN(v.z);
        }

        public static bool IsNanAll(this Vector3 v) {
            return float.IsNaN(v.x) && float.IsNaN(v.y) && float.IsNaN(v.z);
        }

        public static bool IsInfAny(this Vector3 v) {
            return float.IsInfinity(v.x) || float.IsInfinity(v.y) || float.IsInfinity(v.z);
        }

        public static bool IsInfAll(this Vector3 v) {
            return float.IsInfinity(v.x) && float.IsInfinity(v.y) && float.IsInfinity(v.z);
        }

        public static string ToStringEx(this Vector4 v) {
            return string.Format("X: {0} Y: {1} Z: {2} W: {3}", v.x, v.y, v.z, v.w);
        }

        public static string ToStringEx(this Vector3 v) {
            return string.Format("({0}, {1}, {2})", v.x, v.y, v.z);
        }

        public static string ToStringEx(this Vector2 v) {
            return string.Format("X: {0} Y: {1}", v.x, v.y);
        }

        public static Vector3 Divide(this Vector3 v, Vector3 divider) {
            return new Vector3(v.x / divider.x, v.y / divider.y, v.z / divider.z);
        }

        public static Vector3 Multiply(this Vector3 v, Vector3 multiplier) {
            return new Vector3(v.x * multiplier.x, v.y * multiplier.y, v.z * multiplier.z);
        }

        public static Vector2 Rotate(this Vector2 v, float degrees) {
            float radians = degrees * Mathf.Deg2Rad;
            float sin = Mathf.Sin(radians);
            float cos = Mathf.Cos(radians);

            float tx = v.x;
            float ty = v.y;

            return new Vector2(cos * tx - sin * ty, sin * tx + cos * ty);
        }

    }
}
