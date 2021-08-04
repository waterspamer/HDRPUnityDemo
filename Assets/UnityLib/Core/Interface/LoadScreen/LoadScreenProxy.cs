using UnityEngine;

namespace Nettle {

public class LoadScreenProxy : MonoBehaviour {

    public void LoadScene(string sceneName) {
        
        LoadScreenManager manager = FindObjectOfType<LoadScreenManager>();
        if (manager != null) {
            manager.LoadScene(sceneName);            
        }else{
            Debug.Log("Note: LoadScreenManager is not found.");
        }
    }
}
}
