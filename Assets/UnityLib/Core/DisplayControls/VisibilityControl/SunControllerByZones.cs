using UnityEngine;
using System.Collections;

namespace Nettle {

public class SunControllerByZones : MonoBehaviour
{
    public enum TypeUsingController
    {
        BySwitchZone,
        ByKey
    }

    public Light Sun;

    public float ShadowStrength = 0.3f;

    public VisibilityZoneViewer Viewer;
    public NEventKey ChangeKey;

    public TypeUsingController UsingController = TypeUsingController.ByKey;

    public VisibilityZone[] Zones;

    private float _oldShadowStrength;
    private bool _clicked;

    void Start()
    {
        _oldShadowStrength = Sun.shadowStrength;

        Viewer.OnShowZone.AddListener((zone) =>
        {
            if (UsingController == TypeUsingController.BySwitchZone)
            {
                foreach (var z in Zones)
                {
                    if (z.name == zone.name)
                    {
                        ChangeLight();
                        break;
                    }
                    else
                    {
                        ResetLight();
                    }
                }
            }
        });
    }

    public void ChangeLight()
    {
        if (Sun.shadowStrength != ShadowStrength)
        {
            Sun.shadowStrength = ShadowStrength;
        }
    }

    public void ResetLight()
    {
        Sun.shadowStrength = _oldShadowStrength;
    }

    void Update()
    {
        if(ChangeKey != null && ChangeKey && UsingController == TypeUsingController.ByKey)
        {
            foreach(var z in Zones)
            {
                if (Viewer.ActiveZone.name == z.name && !_clicked)
                {
                    ChangeLight();
                }
                else if (Viewer.ActiveZone.name == z.name && _clicked)
                {
                    ResetLight();
                }
            }

            _clicked = !_clicked;
        }
    }
}
}
