using UnityEngine;

public class DetectionStore : MonoBehaviour
{
    public static DetectionStore Instance { get; private set; }

    [Header("Latest Data (read-only)")]
    public YOLODetection currentObject = null;
    public ColorInfo currentColor = null;
    public double lastUpdateTime = 0;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    public void SetObject(YOLODetection det)
    {
        currentObject = det;
        lastUpdateTime = Time.timeAsDouble;
    }

    public void SetColor(ColorInfo col)
    {
        currentColor = col;
        lastUpdateTime = Time.timeAsDouble;
    }
}

