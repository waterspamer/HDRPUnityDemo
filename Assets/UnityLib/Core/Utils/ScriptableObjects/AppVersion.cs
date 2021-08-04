using UnityEngine;

namespace Nettle {

public class AppVersion : ScriptableObject {
    public int MajorVersion = 1;
    public int MinorVersion = 0;
    public int Build = 0;

    public override string ToString() {
        return "v" + MajorVersion + "." + MinorVersion + "." + Build;
    }

}
}
