using Nettle;

public abstract class ObjectRotationZoneViewerBase : ObjectRotationBase {

    public bool ResetRotationOnZoneSwitch = true;
    private VisibilityZone _oldVisibilityZone;

    protected virtual void Awake() {
        VisibilityZoneViewer zv = FindObjectOfType<VisibilityZoneViewer>();
        if (zv != null) {
            _oldVisibilityZone = zv.ActiveZone;
            zv.OnShowZone.AddListener(HandleZoneSwitch);
        }
    }

    private void HandleZoneSwitch(VisibilityZone zone) {
        bool resetRequired = ResetRotationOnZoneSwitch;
        if (_oldVisibilityZone != null && zone.Group != null && _oldVisibilityZone.Group == zone.Group && zone.Group.SwitchWithoutTransition) {
            resetRequired = false;
        }
        if (resetRequired) {
            ResetRotation();
        }
        _oldVisibilityZone = zone;
    }

}
