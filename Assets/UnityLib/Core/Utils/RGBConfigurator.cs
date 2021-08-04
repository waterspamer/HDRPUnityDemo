using UnityEngine;
using UnityEngine.UI;

public class RGBConfigurator : MonoBehaviour {

    public Camera Camera;
    public Slider R;
    public Slider G;
    public Slider B;
    public Text RText;
    public Text GText;
    public Text BText;

    public Material LabelMaterial;
    public Slider RLabel;
    public Slider GLabel;
    public Slider BLabel;
    public Text RLabelText;
    public Text GLabelText;
    public Text BLabelText;

    void Start() {
        R.value = Camera.backgroundColor.r;
        G.value = Camera.backgroundColor.g;
        B.value = Camera.backgroundColor.b;

        RLabel.value = LabelMaterial.color.r;
        GLabel.value = LabelMaterial.color.g;
        BLabel.value = LabelMaterial.color.b;
    }

    void Update() {
        Camera.backgroundColor = new Color(R.value, G.value, B.value);
        RText.text = ConverTo255(R.value).ToString();
        GText.text = ConverTo255(G.value).ToString();
        BText.text = ConverTo255(B.value).ToString();

        LabelMaterial.color = new Color(RLabel.value, GLabel.value, BLabel.value);
        RLabelText.text = ConverTo255(RLabel.value).ToString();
        GLabelText.text = ConverTo255(GLabel.value).ToString();
        BLabelText.text = ConverTo255(BLabel.value).ToString();
    }

    int ConverTo255(float value) {
        return (int)(value * 256);
    }
}
