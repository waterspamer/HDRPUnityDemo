using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

namespace Nettle
{
    public class DummyInfoCreator : DynamicInfoCreator
    {
        [SerializeField]
        private string _numberRegex = @"_(\d+)$";
        [SerializeField]
        private Transform _dummiesRoot;

        string  DummyIdFromName(string dummyName)
        {
            Match m = Regex.Match(dummyName, _numberRegex);
            if (m.Success && m.Groups.Count > 0)
            {
                string result = m.Groups[1].Value.TrimStart('0');
                return result;
            }
            else
            {
                return "";
            }
        }

        public override void CreateInfo()
        {
            foreach (Transform dummy in _dummiesRoot)
            {
                string id = DummyIdFromName(dummy.gameObject.name);
                GuiDatabaseItem info = _info.FindFirst(x => x["id"] == id);
                if (info != null)
                {
                    CreateLabel(info, dummy.position, dummy.rotation);
                }                
            }
        }
    }
}