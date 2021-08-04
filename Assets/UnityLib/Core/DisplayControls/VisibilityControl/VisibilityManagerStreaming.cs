using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Nettle {

public class VisibilityManagerStreaming : VisibilityManager {
    public TexturesStreaming TxStreaming;

    private void Reset() {
        //SceneUtils.FindObjectIfSingle(ref mp3d);
        //SceneUtils.FindObjectIfSingle(ref streamer);
    }

    public override bool BeginSwitch(string newTag,bool forced = false) {
        if (!base.BeginSwitch(newTag,forced)) {
            return false;
        }
        if (TxStreaming != null) {
            TxStreaming.ReloadTextures(true);
        }
        return true;
    }

    protected override bool CanSwitchTag() {
        return TxStreaming==null || TxStreaming.Stage != TexturesStreaming.StreamStage.LoResStreaming;
    }

    protected override void ApplyHideList() {
        base.ApplyHideList();
        if (TxStreaming != null) {
            TxStreaming.Release(HideList.Select(s => s.GetComponent<MeshRenderer>()).ToList());
        } 
    }
}
}
