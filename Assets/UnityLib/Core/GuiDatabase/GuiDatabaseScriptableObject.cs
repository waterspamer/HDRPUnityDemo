using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nettle {


[Serializable]
public class GuiDatabaseScriptableObject : ScriptableObject {
    [SerializeField]
    public List<GuiDatabaseItem> Items;
}

}
