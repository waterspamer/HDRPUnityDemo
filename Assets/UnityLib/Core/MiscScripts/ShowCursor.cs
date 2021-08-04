using System.Linq;
using UnityEngine;

namespace Nettle {

public class ShowCursor : MonoBehaviour {
    [System.Serializable]
    public class CustomCursor {
        public Texture2D Texture;
        public string Name;
        public Vector2 Hotspot;
    }

	[ConfigField("Show")]
	public bool Show = false;

    [Tooltip("Disable ZoomPan when cursor visible")]
    public bool DisableZoomPan = true;

    [SerializeField]
    private CustomCursor[] _cursors;
    [SerializeField]
    private string _defaultCursorName;

    private static ShowCursor _instance;
    public static ShowCursor Instance {
        get {
            if (_instance == null) {
                _instance = FindObjectOfType<ShowCursor>();
            }
            return _instance;
        }
    }

    public CustomCursor CurrentCursor { get; private set; }

    private void Start () {

#if UNITY_EDITOR
        Cursor.visible = true;
#else
        Cursor.visible = Show;
#endif
        if (!string.IsNullOrEmpty(_defaultCursorName)) {
            SetCursor(_defaultCursorName);
        }
    }

    public void CursorVisitiliyToggle() {
        Cursor.visible = !Cursor.visible;
        ZoomPan zoomPan = SceneUtils.FindObjectsOfType<ZoomPan>(true).First();
        if (zoomPan != null) {
            zoomPan.gameObject.SetActive(!Cursor.visible);
        }
    }

    public void SetCursor(string name) {
        CustomCursor cursor = _cursors.Where(x => x.Name == name).FirstOrDefault();
        SetCursor(cursor);
    }

    public void SetCursor(int id) {
        if (id>= 0 && id< _cursors.Length) {
            SetCursor(_cursors[id]);
       }
    }

    private void SetCursor(CustomCursor cursor) {
        if (cursor != null) {
            CurrentCursor = cursor;
            Cursor.SetCursor(cursor.Texture, cursor.Hotspot, CursorMode.Auto);
        }
    }
}
}
