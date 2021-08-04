using System;
using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

namespace Nettle {

public class MipGeneratorEditor : AssetPostprocessor {
    /// <summary>
    /// regex для выборки всех файлов соотвествующих паттерну
    /// </summary>
    private static string pattern = @"\w*forest_slice\d_id.\w*";
    private static Regex regex = new Regex(pattern);


    void OnPostprocessTexture(Texture2D texture) {

        /*
            постпроцессинг сработает только лишь для тех текстур,
            которые соотвествуют регулярному выражению pattern
        */
        string lowerCaseAssetPath = assetPath.ToLower();
        if (!regex.IsMatch(lowerCaseAssetPath)) return;

        for (int m = 0; m < texture.mipmapCount; m++) {
            Color[] currentMip = texture.GetPixels(m);
            if (currentMip.Length == 1) break;

            Color[] nextMip = texture.GetPixels(m + 1);

            var nextMipCounter = 0;

            var lineLength = (int)Mathf.Sqrt(currentMip.Length);

            for (int n = 0; n < currentMip.Length; n += lineLength*2) {
                for (int j = n; j < lineLength + n; j += 2) {
                    var list = new List<Color>();
                    list.Add(currentMip[j]);
                    list.Add(currentMip[j + 1]);
                    list.Add(currentMip[j + lineLength]);
                    list.Add(currentMip[j + 1 + lineLength]);

                    var result = Compare(list);
                    nextMip[nextMipCounter] = result;
                    nextMipCounter++;
                }
            }
            texture.SetPixels(nextMip, m + 1);
        }
        texture.Apply(false);
    }

    /// <summary>
    /// поиск одинаковых элементов в списке из 4 элементов
    /// </summary>
    /// <param name="list">список</param>
    /// <returns>элемент, встречающийся в списке >= 2 раз </returns>
    /// <remarks>если совпадений нет берется первый элемент</remarks>
    static Color Compare(List<Color> list) {
       return list.GroupBy(v => v).OrderByDescending(v => v.Count()).First().Key;
    }
}
}
