using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nettle
{
    public abstract class GUIDatabaseFromFile
    {

        public DataConversionRules ConversionRules { get; set; }

        public abstract void Load(string fileName);
        public abstract void LoadText(string text);
    }
}