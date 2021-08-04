using UnityEngine;

namespace Nettle {

public static class Matrix4x4Extensions  {

	public static Vector3 GetPosition(this Matrix4x4 m) { 
		return (Vector3)m.GetColumn(3);
	}

	public static Quaternion GetRotation(this Matrix4x4 m) { 
		return Quaternion.LookRotation(m.GetColumn(2), m.GetColumn(1));
	}
	public static Vector3 GetScale(this Matrix4x4 m) { 
		return new Vector3(m.GetColumn(0).magnitude, m.GetColumn(1).magnitude, m.GetColumn(2).magnitude);
	}

    public static string ToStringEx(this Matrix4x4 m) {
        var r0 = string.Format("m00: {0} m01: {0} m02: {0} m03: {0}", m.m00, m.m01, m.m02, m.m03);
        var r1 = string.Format("m10: {0} m11: {0} m12: {0} m13: {0}", m.m10, m.m11, m.m12, m.m13);
        var r2 = string.Format("m20: {0} m21: {0} m22: {0} m23: {0}", m.m20, m.m21, m.m22, m.m23);
        var r3 = string.Format("m30: {0} m31: {0} m32: {0} m33: {0}", m.m30, m.m31, m.m32, m.m33);
        return r0 + "\n" + r1 + "\n" + r2 + "\n" + r3;
    }

}
}
