using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nettle
{
    public class DynamicFlatInfo : FileDataProvider
    {
        public int Area;
        public int Building;

        protected override GUIDatabaseFromFile CreateDatabase()
        {
            return new GUIDatabaseFromXML();
        }

        protected override GuiDatabaseItem[] LoadItemsFromFile()
        {
            return (_fileDatabase as GUIDatabaseFromXML) .GetItemsWithPath("//Flats[@a = '"+Area +"' and @b = '"+ Building +"']/Flat");
        }
    }
}