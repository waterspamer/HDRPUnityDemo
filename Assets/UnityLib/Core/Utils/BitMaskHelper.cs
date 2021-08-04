using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nettle {

public class BitMaskHelper  {

    private static int bitMaskSize = 32;

    public static List<int> ToIndexList(int areaMask) {
        List<int> areaIds = new List<int>();
        int areaId = 0;
        for (int bit = 0; bit < bitMaskSize; bit++) {
            if ((areaMask & 1) == 1) {
                areaIds.Add(areaId);
            }
            areaMask = areaMask >> 1;
            areaId++;
        }
        return areaIds;
    }

}
}
