using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Linq;

namespace Nettle
{

    public class ZoneBordersImporter : NettleObjectImporter
    {
        private const float _minBorderAngle = 0.1f;
        private const string _materialName = "ZoneBorderMaterial";
        private const float _borderWidth = 0.1f;
        private const float _borderHeight = 0.03f;

        private static Material _sharedMaterial = null;


        private enum BorderWidthDirections { Inside, Outside, Bothside };

        private const string _borderShaderName = "Diffuse";
        private BorderWidthDirections BorderWidthDirection = BorderWidthDirections.Inside;

        protected override string TargetProperyName
        {
            get
            {
                return "BorderPoints";
            }
        }

        protected override void ProcessObject(GameObject gameObject, string propertyValue, string assetPath)
        {
            if (!gameObject.name.ToLower().StartsWith("zone_"))
            {
                return;
            }
            JArray sourcePoints = JArray.Parse(propertyValue);
            if (sourcePoints== null || sourcePoints.Count == 0)
            {
                return;
            }
            Vector3[] linePoints = new Vector3[sourcePoints.Count];
            float averageHeight = 0;
            for (int i = 0; i < sourcePoints.Count; i++)
            {
                averageHeight += (float)sourcePoints[i][2];
                linePoints[i] = new Vector3((float)sourcePoints[i][0], (float)sourcePoints[i][2] + _borderHeight / gameObject.transform.lossyScale.y, (float)sourcePoints[i][1]);//fix coordinates
            }
            averageHeight /= sourcePoints.Count;
            List<Vector3> linePointsFiltered = new List<Vector3>();
            for (int i = 0; i < linePoints.Length; i++)
            {
                Vector3 prevPoint = i>0?linePoints[i-1]:linePoints[linePoints.Length-1];
                Vector3 nextPoint = i < linePoints.Length - 1 ? linePoints[i + 1] : linePoints[0];       
                if (Vector3.Angle(linePoints[i] - prevPoint, nextPoint - linePoints[i]) >= _minBorderAngle)
                {
                    linePointsFiltered.Add(linePoints[i]);
                }
            }
            linePoints = linePointsFiltered.ToArray();

            VisibilityZone zone = gameObject.GetComponent<VisibilityZone>();
            if (zone == null)
            {
                zone = gameObject.AddComponent<VisibilityZone>();
            }
            zone.VisualCenter = Polylabel.FindPolygonCenter(linePoints, true);
            zone.VisualCenter.y = averageHeight;
            Mesh mesh = CreateBorderMesh(gameObject, linePoints);
            string generatedAssetsDir = Path.GetDirectoryName(assetPath) + "/Materials/Generated";
            string meshPath = generatedAssetsDir + "/" + GameObjectEx.GetGameObjectPath(gameObject) + ".asset";
            Directory.CreateDirectory(Path.GetDirectoryName(meshPath));
            AssetDatabase.CreateAsset(mesh, meshPath);
            gameObject.AddComponent<MeshFilter>().mesh = mesh;

            //Material material = AssetDatabase.LoadAssetAtPath<Material>(generatedAssetsDir + "/Material.mat");
            Material material = _sharedMaterial;
            if (material == null)
            {
                string[] materials = AssetDatabase.FindAssets("t:Material " + _materialName);
                if (materials.Length > 0)
                {
                    material = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(materials[0])) as Material;
                }
                if (material == null)
                {
                    material = new Material(Shader.Find(_borderShaderName));
                    AssetDatabase.CreateAsset(material, generatedAssetsDir + "/" + _materialName + ".mat");
                }
                _sharedMaterial = material;
            }
            MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshRenderer.receiveShadows = false;
            meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            meshRenderer.sharedMaterial = material;
            meshRenderer.enabled = false;
        }

        public Mesh CreateBorderMesh(GameObject gameObject, Vector3[] linePoints)
        {
            for (int i = 0; i < linePoints.Length; i++)
            {
                Vector3 p0 = linePoints[Convert.ToInt32(Mathf.Repeat(i - 1, linePoints.Length - 1))];
                Vector3 p1 = linePoints[Convert.ToInt32(Mathf.Repeat(i, linePoints.Length - 1))];
                Vector3 p2 = linePoints[Convert.ToInt32(Mathf.Repeat(i + 1, linePoints.Length - 1))];
                if (Vector3.Angle(p1 - p0, p2 - p1) < 0.001f)
                {
                    linePoints = linePoints.Where((val, idx) => idx != i).ToArray();
                }
                if (Vector3.Distance(p1, p2) < 0.0001f)
                {
                    Debug.LogWarning(gameObject.name + "::Very short distance(" + Vector3.Distance(p1, p2) + ") between points " + Convert.ToInt32(Mathf.Repeat(i, linePoints.Length - 1)) + " and "
                        + Convert.ToInt32(Mathf.Repeat(i + 1, linePoints.Length - 1)) + ". Point " + Convert.ToInt32(Mathf.Repeat(i, linePoints.Length - 1)) + " will be removed.");

                    linePoints = linePoints.Where((val, idx) => idx != i).ToArray();
                }
            }

            Vector3[] extendedPoints = new Vector3[linePoints.Length + 2];
            extendedPoints[0] = linePoints[linePoints.Length - 1];
            extendedPoints[extendedPoints.Length - 1] = linePoints[0];
            linePoints.CopyTo(extendedPoints, 1);

            float totalAngel = 0f;
            for (int i = 1; i < extendedPoints.Length - 1; i++)
            {

                Vector3 v1 = Vector3.Normalize(extendedPoints[i] - extendedPoints[i - 1]);
                Vector3 v2 = Vector3.Normalize(extendedPoints[i + 1] - extendedPoints[i]);
                totalAngel += Mathf.Atan2(Vector3.Dot(Vector3.up, Vector3.Cross(v1, v2)), Vector3.Dot(v1, v2));
            }
            bool isClockwise = totalAngel > 0 ? true : false;
            //calculate 2 border points for every spline point
            Vector3[] borderPoints = new Vector3[linePoints.Length * 2 + 2];
            for (int i = 1; i < extendedPoints.Length - 1; i++)
            {
                Vector3 v1 = Vector3.Normalize(extendedPoints[i - 1] - extendedPoints[i]);
                Vector3 v2 = Vector3.Normalize(extendedPoints[i + 1] - extendedPoints[i]);
                Vector3 offsetDirection = Vector3.Normalize(v1 + v2);
                Vector3 borderPoint = extendedPoints[i] - offsetDirection;
                Vector3 v3 = extendedPoints[i] - extendedPoints[i - 1];
                Vector3 v4 = borderPoint - extendedPoints[i - 1];
                if (v4.z * v3.x - v3.z * v4.x < 0) //if clockwise
                {
                    offsetDirection = -offsetDirection;
                }

                float sinA = Vector3.Cross(offsetDirection, v2).magnitude / (offsetDirection.magnitude * v2.magnitude);
                float width = _borderWidth / sinA / gameObject.transform.lossyScale.x;

                float widthInside = 0;
                float widthOutside = 0;
                switch (BorderWidthDirection)
                {
                    case BorderWidthDirections.Inside:
                        if (isClockwise)
                        {
                            widthInside = width;
                        }
                        else
                        {
                            widthOutside = width;
                        }
                        break;
                    case BorderWidthDirections.Outside:
                        if (isClockwise)
                        {
                            widthOutside = width;
                        }
                        else
                        {
                            widthInside = width;
                        }
                        break;
                    case BorderWidthDirections.Bothside:
                        widthInside = width / 2;
                        widthOutside = width / 2;
                        break;
                }

                //widthOutside *= i;
                //widthInside *= i;

                borderPoints[(i - 1) * 2 + 0] = extendedPoints[i] - offsetDirection * widthOutside;
                borderPoints[(i - 1) * 2 + 1] = extendedPoints[i] + offsetDirection * widthInside;
            }

            Vector3[] meshVertices = new Vector3[linePoints.Length * 6];
            int[] meshIndices = new int[linePoints.Length * 6];
            borderPoints[borderPoints.Length - 2] = borderPoints[0];
            borderPoints[borderPoints.Length - 1] = borderPoints[1];

            var idMap = new[] { 0, 2, 3, 0, 3, 1 };
            for (int i = 0; i < borderPoints.Length - 3; i += 2)
            {
                for (int j = 0; j < 6; j++)
                {
                    meshVertices[i * 3 + j] = borderPoints[i + idMap[j]];
                    meshIndices[i * 3 + j] = i * 3 + j;
                }
            }

            Mesh mesh = new Mesh
            {
                name = gameObject.name,
                vertices = meshVertices,
                triangles = meshIndices
            };

            mesh.RecalculateNormals();
            return mesh;
        }
    }
}