using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nettle
{
    public abstract class DynamicInfoLabel:MonoBehaviour
    {
       public abstract void OutputInfo(GuiDatabaseItem info);
       /// <summary>
       /// VisibilityZone this label is attached to (optional) 
       /// </summary>
       public VisibilityZone Zone{ get; set; }
    }
}