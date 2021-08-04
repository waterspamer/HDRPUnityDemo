using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Nettle
{
    public class DynamicParkingPlaceLabel : DynamicInfoLabel
    {
        [SerializeField]
        private GameObjectList _carsList;
        [SerializeField]
        private string _availableStatus = "Свободно";
        [SerializeField]
        private GameObject _carPlaceholder;
        [SerializeField]
        private TextMeshPro _priceOutput;
        [SerializeField]
        private string _pricePostfix = " р.";
        [SerializeField]
        private CarPlacerPostprocessor _postprocessor;

        public override void OutputInfo(GuiDatabaseItem info)
        {
            bool taken = info["status"] != _availableStatus;
            _carPlaceholder.SetActive(!taken);
            if (taken)
            {
                GameObject car = Instantiate(_carsList.GetRandomObject());
                car.transform.SetParent(transform, false);
                if (_postprocessor != null)
                {
                    _postprocessor.PostprocessObject(car);
                }
            }else
            {
                string price;
                if (info.GetFieldByHeader("price",out price))
                {
                    _priceOutput.text = price + " ";
                }
            }            
        }
    }
}
