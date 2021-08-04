﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MatchAnchorsWithRect
{
    [MenuItem("Nettle/Match Anchors To Rect")]
    static void MatchAnchors()
    {
        foreach (Transform tr in Selection.transforms)
        {
            if (tr is RectTransform)
            {
                var r = tr as RectTransform;
                var p = tr.parent.GetComponent<RectTransform>();
                if (p == null)
                {
                    continue;
                }
                var offsetMin = r.offsetMin;
                var offsetMax = r.offsetMax;
                var _anchorMin = r.anchorMin;
                var _anchorMax = r.anchorMax;

                var parent_width = p.rect.width;
                var parent_height = p.rect.height;

                var anchorMin = new Vector2(_anchorMin.x + (offsetMin.x / parent_width),
                                            _anchorMin.y + (offsetMin.y / parent_height));
                var anchorMax = new Vector2(_anchorMax.x + (offsetMax.x / parent_width),
                                            _anchorMax.y + (offsetMax.y / parent_height));

                r.anchorMin = anchorMin;
                r.anchorMax = anchorMax;

                r.offsetMin = new Vector2(0, 0);
                r.offsetMax = new Vector2(1, 1);
                r.pivot = new Vector2(0.5f, 0.5f);

            }
        }
    }
}