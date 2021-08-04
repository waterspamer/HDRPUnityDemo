using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nettle {

public class LocalizedInfoObject : MonoBehaviour {
        public LanguageInfoController.LangugaeInfoParent InfoParent;
        private void Reset() {
            InfoParent = new LanguageInfoController.LangugaeInfoParent();
            InfoParent.InfoParent = gameObject;
        }
    }
}
