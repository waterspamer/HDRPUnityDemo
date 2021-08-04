using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nettle {
    public interface IDynamicInfoDataProvider {
        void GetItems (DatabaseItemsLoadedCallback callback);
    }
    public delegate void DatabaseItemsLoadedCallback (DataLoadResult result);
    public struct DataLoadResult {
        public GuiDatabaseItem[] Items;
        public bool Error;
    }
}