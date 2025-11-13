using UnityEngine;
using TMPro;

public class ObjectLabelManager : MonoBehaviour
{
    public GameObject labelPrefab;   // 3D TextMeshPro prefab
    public GameObject webcamPlane;   // plane object showing camera feed
    public float labelScale = 0.12f;
    public float labelYOffset = 0.05f; // how far above the object the label appears

    private GameObject currentLabel;

    void Start()
    {
        if (labelPrefab == null) Debug.LogError("Assign labelPrefab in ObjectLabelManager.");
        if (webcamPlane == null) Debug.LogError("Assign webcamPlane in ObjectLabelManager.");
    }

    void Update()
    {
        var store = DetectionStore.Instance;
        if (store == null) return;

        var det = store.currentObject;
        if (det == null)
        {
            if (currentLabel != null)
            {
                Destroy(currentLabel);
                currentLabel = null;
            }
            return;
        }

        // Get top-center point of bounding box instead of center
        float xCenter = (det.bbox[0] + det.bbox[2]) / 2f;
        float yTop = det.bbox[1]; // y1 is top of bbox
        float normX = xCenter / 640f;
        float normY = yTop / 480f;

        // Plane dimensions
        MeshRenderer mr = webcamPlane.GetComponent<MeshRenderer>();
        float planeWidth = mr.bounds.size.x;
        float planeHeight = mr.bounds.size.y;

        // World position mapping
        float worldX = -(normX - 0.5f) * planeWidth;
        float worldY = (0.5f - normY) * planeHeight + labelYOffset; // offset above object

        Vector3 labelPos = webcamPlane.transform.position
                         + webcamPlane.transform.right * worldX
                         + webcamPlane.transform.up * worldY
                         + webcamPlane.transform.forward * -0.02f;

        // Create or move label
        if (currentLabel == null)
        {
            currentLabel = Instantiate(labelPrefab, labelPos, Quaternion.identity, this.transform);
            currentLabel.SetActive(true);
            currentLabel.transform.localScale = Vector3.one * labelScale;
        }
        else
        {
            currentLabel.transform.position = labelPos;
        }

        // Set text
        TextMeshPro tmp = currentLabel.GetComponent<TextMeshPro>();
        if (tmp != null)
        {
            tmp.text = det.label;

            // Billboard to face the camera
            currentLabel.transform.LookAt(Camera.main.transform);
            currentLabel.transform.Rotate(0, 180, 0);
        }
    }
}


