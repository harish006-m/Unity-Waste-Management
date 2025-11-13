using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ColorDisplay : MonoBehaviour
{
    public Image colorImage;      // UI Image component to show the color
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI hexText;

    void Start()
    {
        if (colorImage == null) Debug.LogError("Assign colorImage in ColorDisplay.");
    }

    void Update()
    {
        var store = DetectionStore.Instance;
       var c = (store != null) ? store.currentColor : null;
        if (c == null) return;

        // For this code to work, you must have r/g/b fields in your ColorInfo!
        colorImage.color = new Color(c.r / 255f, c.g / 255f, c.b / 255f);

        // If you have name/hex in your ColorInfo, show them; else skip these assignments
        if (nameText != null) nameText.text = $" {((c.GetType().GetField("name") != null) ? c.GetType().GetField("name").GetValue(c) : "")}";
        if (hexText != null) hexText.text = $" {((c.GetType().GetField("hex") != null) ? c.GetType().GetField("hex").GetValue(c) : "")}";
    }
}
