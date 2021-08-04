using System;
using UnityEngine;

namespace Nettle {

[Serializable]
public class NTransform {
    public Vector3 Position;
    public Quaternion Rotation;
    public Vector3 Scale;

    public NTransform() {
        Position = Vector3.zero;
        Rotation = Quaternion.identity;
        Scale = Vector3.one;
    }

    public NTransform(Transform src, float scaleFactor = 1) {
        Position = src.position;
        Rotation = src.rotation;
        Scale = src.lossyScale * scaleFactor;
    }

    public void Apply(Transform target) {
        if (target != null) {
            target.position = Position;
            target.rotation = Rotation;
            target.localScale = Scale;
        }
    }

    public static NTransform Lerp(NTransform from, NTransform to, float t) {
        return new NTransform() {
            Position = Vector3.Lerp(from.Position, to.Position, t),
            Rotation = Quaternion.Lerp(from.Rotation, to.Rotation, t),
            Scale = Vector3.Lerp(from.Scale, to.Scale, t)
        };
    }

        public Vector3 TransformPoint(Vector3 point) {
            return Rotation*Vector3.Scale(point,Scale) + Position;
        }

        public Vector3 TransformVector(Vector3 vector) {
            return Rotation * Vector3.Scale(vector, Scale);
        }

        public Vector3 InverseTransformPoint(Vector3 point) {
            Vector3 result = point;
            result -= Position;
            result = Quaternion.Inverse(Rotation) * result;
            result = new Vector3(result.x / Scale.x, result.y /Scale.y, result.z/Scale.z);
            return result;
        }

        public Vector3 InverseTransformVector(Vector3 vector) {
            Vector3 result = vector;
            result = Quaternion.Inverse(Rotation) * result;
            result = new Vector3(result.x / Scale.x, result.y / Scale.y, result.z / Scale.z);
            return result;
        }
    }
}
