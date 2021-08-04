using System;
using UnityEngine;

namespace Nettle {

public class VisibilityControlAlways : VisibilityControl {

    private VisibilityManagerStreaming _manager;
    private bool _isStreamingStarted;

    public bool PreviewLoaded;

    public override bool HasTag(string newTag) {
        return true;
    }

    void Reset() {
        ObjectName = name;
    }

    /*public override void EnableVisibilityFactor(string name) {
        base.EnableVisibilityFactor(name);
        _isStreamingStarted = true;
    }

   void OnEnable() {
        _manager = FindObjectOfType<VisibilityManagerStreaming>();
        if (_manager != null) {
            _manager.TxStreaming.StageChanged += TxStreamingOnStageChanged;
        }
    }

    void OnDisable() {
        if (_manager != null) {
            _manager.TxStreaming.StageChanged -= TxStreamingOnStageChanged;
        }
    }

    private void TxStreamingOnStageChanged(TexturesStreaming texturesStreaming) {
        if (texturesStreaming.Stage == TexturesStreaming.StreamStage.Idle && _isStreamingStarted) {
            _manager.Controls.Remove(this);
            Destroy(this);
        }
    }*/
}
}
