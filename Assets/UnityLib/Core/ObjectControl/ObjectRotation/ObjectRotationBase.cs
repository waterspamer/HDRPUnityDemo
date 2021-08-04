using UnityEngine;
using System.Collections;

namespace Nettle {

public abstract class ObjectRotationBase : MonoBehaviour {

    public abstract void ResetRotation();

    public abstract void Rotate(Vector2 delta);
}
}
