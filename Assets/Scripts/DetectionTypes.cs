using System;

[Serializable]
public class YOLODetection
{
    public float[] bbox;   // [x1, y1, x2, y2] OR normalized [0-1]
    public int cls;        // class id (0=plastic, 1=iron, etc.)
    public float conf;     // confidence, e.g. 0.98
    public string label;   // optional class label (e.g. "plastic")
}

[Serializable]
public class ColorInfo
{
    public float r, g, b;  // color values (optionally int, should match your usage)
    // OR, if you use rgb array in your UI code:
    // public int[] rgb;
    // public string hex;
    // public string name;
    // public float ratio;
}

[Serializable]
public class IncomingMessage
{
    public YOLODetection[] objects;
    public ColorInfo color;
    public int[] img_size; // [width, height]
    public string frame;   // base64 encoded JPEG
}
