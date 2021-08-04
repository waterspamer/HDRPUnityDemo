using UnityEngine;
using System.Collections;

namespace Nettle {

public class NEventStreamingComplete : NEvent {

    private TexturesStreaming _streaming;

    private void Start() {
        _streaming = SceneUtils.FindObjectIfSingle<TexturesStreaming>();
    }

    protected override bool Get() {
        if (_streaming == null) {
            Debug.Log("TextureStreaming is null!");
            return true;
        }

        var state = _streaming.Stage;
        return state == TexturesStreaming.StreamStage.Idle;
    }
}
}
