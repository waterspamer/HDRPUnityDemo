using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nettle {
    public class SpawnableLocalizedInfoObject : LocalizedInfoObject {
        private void Start() {
            LanguageInfoController.Instance.AddLocalizedInfoObject(InfoParent);
        }
    }
}
