using UnityEngine;
using System.Collections;

namespace Nettle {

public class FindMissingComponents : MonoBehaviour {

    private static int countGameObjects = 0;
    private static int countComponents = 0;
    private static int countMissing = 0;

    public static void Find(GameObject[] gameObjects) {
        countGameObjects = 0;
        countComponents = 0;
        countMissing = 0;
        foreach (GameObject gameObj in gameObjects) {
            Find(gameObj);
        }
        Debug.Log(string.Format("Searched {0} GameObjects, {1} components, found {2} missing", countGameObjects, countComponents, countMissing));
    }

    private static void Find(GameObject gameObj) {
        countGameObjects++;
        string sName;
        Transform transf;
        Component[] components = gameObj.GetComponents<Component>();
        for (int i = 0; i < components.Length; i++) {
            countComponents++;

            if (components[i] == null) {

                countMissing++;
                sName = gameObj.name;
                transf = gameObj.transform;
                while (transf.parent != null) {
                    sName = transf.parent.name + "/" + sName;
                    transf = transf.parent;
                }
                //Debug.Log(s + " has an empty script attached in position: " + i, g);
                Debug.LogError(sName + " has an empty script attached in position: " + i, gameObj);
            }
        }

        // Now recurse through each child
        foreach (Transform childT in gameObj.transform) {
            //Debug.Log("Searching " + childT.name  + " " );
            Find(childT.gameObject);
        }
    }
}
}
