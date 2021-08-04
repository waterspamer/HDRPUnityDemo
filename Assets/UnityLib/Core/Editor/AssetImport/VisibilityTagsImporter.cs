using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace Nettle
{

    public class VisibilityTagsImporter : NettleObjectImporter
    {

        protected override string TargetProperyName
        {
            get
            {
                return "VisibilityTag";
            }
        }

        protected override void ProcessObject(GameObject go, string propertyValue, string assetPath)
        {
            string[] values = JArray.Parse(propertyValue).ToObject<string[]>();
            if (go.name.ToLower().StartsWith("border_"))
            {

                var hideByTag = go.AddComponent<HideByTag>();
                if (values != null)
                {
                    hideByTag.Tags = values.ToList();
                    hideByTag.Action = HideByTag.TagAction.Show;
                }
            }
            else if (go.name.ToLower().StartsWith("zone_") || go.name.ToLower() == "start")
            {
                var control = go.GetComponent<VisibilityZone>();
                if (control == null)
                {
                    control = go.AddComponent<VisibilityZone>();
                }

                control.VisibilityTag = values[0];
                if (go.transform.parent == null) //|| go.transform.parent.GetComponentInParent<VisibilityZone>() == null)
                {
                    var rotation = go.transform.rotation.eulerAngles;
                    rotation.x += 90.0f;
                    rotation.y += 180.0f;
                    go.transform.rotation = Quaternion.Euler(rotation);
                }
                else
                {
                    //z rotate -180 degree
                    float x = go.transform.position.x - 2 * (go.transform.position.x - go.transform.parent.transform.position.x);
                    float y = go.transform.position.y - 2 * (go.transform.position.y - go.transform.parent.transform.position.y);
                    float z = go.transform.position.z;
                    go.transform.position = new Vector3(x, y, z);

                    //x rotate -90 degree
                    x = go.transform.position.x;
                    y = go.transform.parent.transform.position.y + (go.transform.parent.transform.position.z - go.transform.position.z);
                    z = go.transform.parent.transform.position.z - (go.transform.parent.transform.position.y - go.transform.position.y);
                    go.transform.position = new Vector3(x, y, z);

                }

            }
            else if (go.transform.childCount > 0) //Такое возможно? 
            {
                //mesh splitted
                for (int i = 0; i < go.transform.childCount; ++i)
                {
                    var control = go.transform.GetChild(i).gameObject.AddComponent<VisibilityControl>();
                    control.Tags = values;
                    control.ObjectName = go.name;
                }
            }
            else
            {
                var control = go.AddComponent<VisibilityControl>();
                control.Tags = values;
                control.ObjectName = go.name;
            }
        }
    }
}
