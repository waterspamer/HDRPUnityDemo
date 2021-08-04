using UnityEditor;
using UnityEngine;

namespace Nettle {
    public class ObjectPlacerCarWindow : EditorWindow {

        public GameObject TargetCar;
        public ObjectPlacerCarData CarData;
        public bool RandomTone = true;

        [MenuItem("Nettle/Object Placer/Car", false, 1)]
        public static void ShowWindow() {
            ShowWindow(null);
        }

        public static ObjectPlacerCarWindow ShowWindow(ObjectPlacerCarData carData) {
            EditorWindow window = GetWindow(typeof(ObjectPlacerCarWindow));
            window.autoRepaintOnSceneChange = true;
            window.titleContent = new GUIContent("Object Placer Car");
            ObjectPlacerCarWindow carWindow = ((ObjectPlacerCarWindow)window);
            carWindow.CarData = carData;
            return carWindow;
        }

        void OnGUI() {
            TargetCar = (GameObject)EditorGUILayout.ObjectField("Target car", TargetCar, typeof(GameObject), true);
            CarData = (ObjectPlacerCarData)EditorGUILayout.ObjectField("Car data", CarData, typeof(ObjectPlacerCarData), false);
            RandomTone = EditorGUILayout.Toggle("Random tone", RandomTone);

            if (CarData) {
                foreach (var colorData in CarData.ColorsData) {
                    EditorGUILayout.BeginHorizontal();

                    if (GUILayout.Button("Set")) {
                        Color color;
                        if (RandomTone) {
                            color = colorData.GetRandomTonedColor();
                        } else {
                            color = colorData.Color;
                        }
                        CarData.PaintCar(TargetCar, color);
                    }
                    EditorGUILayout.ColorField(colorData.Color);
                    EditorGUILayout.EndHorizontal();
                }
            }
        }
    }
}
