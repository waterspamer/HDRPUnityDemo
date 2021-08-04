using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Selects the widest sprite that fits current screen resolution and assigns it to given GUI image
public class ImageByScreenSize : MonoBehaviour
{
    [SerializeField]
    private Sprite[] _sprites;
    [SerializeField]
    private Image _image;

    private void Start()
    {
        AssignSprite();
    }

    public void AssignSprite()
    {
        Sprite selectedSprite = null;
        foreach (Sprite s in _sprites)
        {
            if (s.rect.width <= Screen.width && (selectedSprite == null || s.rect.width > selectedSprite.rect.width))
            {
                selectedSprite = s;
            }
        }
        Debug.Log(selectedSprite.name + " " + Screen.width + " " + selectedSprite.rect.width);
        if (selectedSprite != null)
        {
            _image.sprite = selectedSprite;
            _image.SetNativeSize();
        }
    }
}
