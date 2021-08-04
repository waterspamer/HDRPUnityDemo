using UnityEngine;

namespace Nettle
{
    public interface IObjectPlacerPostprocessor
    {
        bool IsEnabled();
        void PostprocessObject(GameObject newObject);
    }
}
