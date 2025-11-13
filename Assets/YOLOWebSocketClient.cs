using System;
using System.Threading.Tasks;
using UnityEngine;
using NativeWebSocket;
using System.Text;

public class YOLOWebSocketClient : MonoBehaviour
{
    public string url = "ws://localhost:8765";
    WebSocket websocket;

    private Texture2D videoTexture;
    private Renderer targetRenderer;

    async void Awake()
    {
        // ensure DetectionStore exists (create if missing)
        if (DetectionStore.Instance == null)
        {
            GameObject store = new GameObject("DetectionStore");
            store.AddComponent<DetectionStore>();
        }

        // get renderer on same GameObject (e.g. Quad/Plane)
        targetRenderer = GetComponent<Renderer>();
        videoTexture = new Texture2D(2, 2, TextureFormat.RGB24, false);

        DontDestroyOnLoad(this.gameObject); // persist through scene switches
        await Connect();
    }

    public async Task Connect()
    {
        websocket = new WebSocket(url);

        websocket.OnOpen += () =>
        {
            Debug.Log("✅ Connected to YOLO WebSocket Server!");
        };

        websocket.OnError += (e) =>
        {
            Debug.LogError("❌ WebSocket Error: " + e);
        };

        websocket.OnClose += (e) =>
        {
            Debug.Log("⚠️ WebSocket Closed: " + e);
        };

        websocket.OnMessage += (bytes) =>
        {
            string json = Encoding.UTF8.GetString(bytes);
            try
            {
                IncomingMessage msg = JsonUtility.FromJson<IncomingMessage>(json);

                // ----- update video frame -----
                if (!string.IsNullOrEmpty(msg.frame))
                {
                    byte[] imageBytes = Convert.FromBase64String(msg.frame);
                    videoTexture.LoadImage(imageBytes);
                    targetRenderer.material.mainTexture = videoTexture;
                }

                // ----- update detections -----
                int imgW = 640, imgH = 480;
                if (msg.img_size != null && msg.img_size.Length >= 2)
                {
                    imgW = msg.img_size[0];
                    imgH = msg.img_size[1];
                }

                if (msg.objects != null && msg.objects.Length > 0 && msg.objects[0] != null)
                {
                    YOLODetection raw = msg.objects[0];
                    if (raw.bbox != null && raw.bbox.Length == 4)
                    {
                        bool looksNormalized = (raw.bbox[0] <= 1.01f && raw.bbox[1] <= 1.01f &&
                                               raw.bbox[2] <= 1.01f && raw.bbox[3] <= 1.01f);

                        if (looksNormalized)
                        {
                            float x1 = raw.bbox[0] * imgW;
                            float y1 = raw.bbox[1] * imgH;
                            float x2 = raw.bbox[2] * imgW;
                            float y2 = raw.bbox[3] * imgH;

                            raw.bbox = new float[] { x1, y1, x2, y2 };
                            Debug.Log($"Converted normalized bbox -> pixels: [{x1:F1},{y1:F1},{x2:F1},{y2:F1}] using img_size {imgW}x{imgH}");
                        }
                    }
                    DetectionStore.Instance.SetObject(raw);
                }
                else
                {
                    DetectionStore.Instance.SetObject(null);
                }

                if (msg.color != null)
                    DetectionStore.Instance.SetColor(msg.color);
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSON parse error: {ex}\nJSON: {json}");
            }
        };

        await websocket.Connect();
    }

    void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        if (websocket != null) websocket.DispatchMessageQueue();
#endif
    }

    private async void OnDestroy()
    {
        try
        {
            if (websocket != null) await websocket.Close();
        }
        catch { }
    }
}
