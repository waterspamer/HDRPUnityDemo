using System;
using System.Collections.Generic;
using System.Linq;
using Nettle;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Random = UnityEngine.Random;
namespace Nettle {   

    [CreateAssetMenu(fileName = "ObjectPlacerCarData", menuName = "ObjectPlacer/CarData")]
    public class ObjectPlacerCarData : ObjectPlacerData {

        [System.Serializable]
        public class MaterialMappingPair
        {
            public Material DynamicMaterial;
            public Material StaticMaterial;
        }

        private const float H_SHIFT = 0f;
        private const float S_SHIFT = 0.1f;
        private const float V_SHIFT = 0.1f;

        /*#(color 15 15 15, 27), -- black from Artem Basnev
        #(color 192 192 192, 16), \ -- silver
        #(color 255 250 250, 15), \ -- white
        #(color 70 70 70, 13), \ -- grey from Artem Basnev
        #(color 14 14 66, 8.0), \ -- deep blue from Artem Basnev
        #(color 150 6 0, 6), \ -- red from Artem Basnev
        #(color 19 53 15, 3.2), \ -- green  from Artem Basnev
        #(color 150 75 0,  2.8), \ -- brown
        #(color 215 216 223, 2.7), \ -- beige from Artem Basnev
        #(color 131 174 220, 2), \ -- blue from Artem Basnev
        #(color 255 215 0, 1.4), \ -- gold
        #(color 255 165 0, 1), \-- orange
        #(color 118 173 72, 1), \ -- salad from Artem Basnev
        #(color 176 123 64, .9) \-- light brown*/

        /*fn Generate5Tones CarColor i = (
            hues = #()
            -- create color different from original up to 10% of 255
            a = CarColor
            -- a.hue
            minSat = CarColor.saturation - shift
            if minSat<=0 then minSat = 0
            maxSat = CarColor.saturation + shift
            if maxSat>255 then maxSat = 255
            minVal = CarColor.value - shift
            if minVal<=0 then minVal = 0
            maxVal = CarColor.value + shift
            if maxVal>255 then maxVal = 255

            for i=1 to 5 do (
                a.saturation = random minSat maxSat
                a.value = random minVal maxVal
                if (a.r==a.g and a.g==a.b) then (
                    a.hue = hue_arr[(random 1 hue_arr.count) as integer]
                )
                append hues (copy a)
                a = CarColor
            )
            CarsColorsTones[i] = hues
    )*/

        [Serializable]
        public class ColorData {
            public Color Color;
            public float Weight;

            public Color GetRandomTonedColor() {
                float h;
                float s;
                float v;
                Color.RGBToHSV(Color, out h, out s, out v);

                h = HSVRandomShift(h, H_SHIFT);
                s = HSVRandomShift(s, S_SHIFT);
                v = HSVRandomShift(v, V_SHIFT);
                return Color.HSVToRGB(h, s, v);
            }

            private float HSVRandomShift(float value, float shift) {
                return Mathf.Clamp01(value + shift * Random.Range(-1f, 1f));
            }
        }

        public List<ColorData> ColorsData;
        public List<MaterialMappingPair> MaterialMapping;
        /*
                private ObjectPlacerCarWindow _carWindow;

                public override GameObject Generate(List<ObjectToPlace> objects) {
                    GameObject go = base.Generate(objects);
                    PaintCar(go, GetRandomColor());

                    if (!_carWindow) {
                        _carWindow = ObjectPlacerCarWindow.ShowWindow(this);
                    }

                    _carWindow.TargetCar = go;

                    return go;
                }
        */
        public Color GetRandomColor() {
            return ColorsData[RandomEx.Weight(ColorsData.Select(v => v.Weight))].GetRandomTonedColor();
        }

        public void PaintCar(GameObject carObject, Color color, bool isStatic = true) {
            List<SkinnedMeshRenderer> skinnedMeshRenderers = carObject.GetComponentsInChildren<SkinnedMeshRenderer>().ToList();
            foreach (var skinnedMeshRenderer in skinnedMeshRenderers) {
                if (isStatic)
                {
                    Material[] materials = skinnedMeshRenderer.sharedMaterials;
                    for (int i = 0; i < materials.Length; i++)
                    {
                        materials[i] = GetStaticMaterialForDynamic(materials[i]);
                    }
                    skinnedMeshRenderer.sharedMaterials = materials;
                    Mesh mesh = Instantiate(skinnedMeshRenderer.sharedMesh);
                    mesh.SetVerticesColor(color);
                    skinnedMeshRenderer.sharedMesh = mesh;
                }
                else
                {
                    MaterialPropertyBlock properties = new MaterialPropertyBlock();
                    skinnedMeshRenderer.GetPropertyBlock(properties);
                    properties.SetColor("_Color", color);
                    skinnedMeshRenderer.SetPropertyBlock(properties);
                }
            }
        }

        public Material GetStaticMaterialForDynamic(Material dynamicMaterial){
            MaterialMappingPair pair = MaterialMapping.Find(x => x.DynamicMaterial == dynamicMaterial);
            if (pair != null)
            {
                return pair.StaticMaterial;
            }
            else
            {
                return dynamicMaterial;
            }
        }

    }


#if UNITY_EDITOR
    [CustomEditor(typeof(ObjectPlacerCarData))]
    public class ObjectPlacerCarDataEditor : ObjectPlacerDataEditor {

    }
#endif
}