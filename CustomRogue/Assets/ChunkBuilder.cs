using System.Drawing;
using System.Runtime.InteropServices;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class ChunkBuilder : MonoBehaviour
{
    [SerializeField] MarchingCubesCompute chunkCanvas;
    [SerializeField] WorldSettings worldSettings;
    Chunk chunk;

    [Header("Object settings")]
    [SerializeField] GameObject objectHolder;
    public float objectSmoothing = 2.0f;
    public float distortionAmount = 0.15f;
    public float distortionScale = 0.25f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // The only one
        chunk = chunkCanvas.GetChunkFromCoord(new Vector3Int(0, 0, 0));
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            UpdateChunkPoints();
        }
    }

    // Set points within an object to 1
    public void UpdateChunkPoints()
    {
        // Get the points
        int numPoints = chunk.GetNumberOfPoints();
        Vector4[] points = new Vector4[numPoints];
        chunk.pointsBuffer.GetData(points);

        // Check points within each object
        foreach (Transform child in objectHolder.transform)
        {
            SetToNearestPoint(child, points);

            // Check if it has a collider
            if (child.TryGetComponent(out Collider col))
            {
                // Check all points
                for (int i = 0; i < points.Length; i++)
                {
                    Vector3 pos = new Vector3(points[i].x, points[i].y, points[i].z);
                    Vector3 closestPoint = col.ClosestPoint(pos);
                    float dist = Vector3.Distance(closestPoint, pos);

                    bool insideCol = dist < 0.000001f;

                    float newValue = points[i].w;

                    // Check if point is inside object
                    if (insideCol)
                    {
                        Debug.Log("Point was within collider");
                        newValue = 1f; // Set density
                    }
                    else if (dist < objectSmoothing)
                    {
                        float invertedDist = 1f - (dist / objectSmoothing);
                        newValue = Mathf.SmoothStep(0f, 1f, invertedDist);
                    }

                    // Only change if the value is greater than the current
                    points[i].w = Mathf.Max(points[i].w, newValue);
                }
            }
            else
            {
                Debug.LogError("No collider found in: " + child.name);
            }
        }

        // Apply point info
        chunk.pointsBuffer.SetData(points);
        // Set chunk as changed
        chunk.valuesChanged = true;
    }

    void DistortPoint(Vector4 point)
    {
        float distortion = Random.Range(-0.45f, 0.5f);

        point.w += distortion;
    }

    public void DistortChunk()
    {
        int numPoints = chunk.GetNumberOfPoints();
        Vector4[] points = new Vector4[numPoints];
        chunk.pointsBuffer.GetData(points);

        for (int i = 0; i < points.Length; i++)
        {
            Vector3 pos = new Vector3(points[i].x, points[i].y, points[i].z);

            // Only distort near the surface
            if (points[i].w > -0.2f && points[i].w < 0.8f && !IsEdgePoint(i))
            {
                float noise =
                Mathf.PerlinNoise(pos.x * distortionScale, pos.y * distortionScale) +
                Mathf.PerlinNoise(pos.y * distortionScale, pos.z * distortionScale) +
                Mathf.PerlinNoise(pos.x * distortionScale, pos.z * distortionScale);

                noise = noise / 3f * 2f - 1f;
                points[i].w += noise * distortionAmount;
            }
        }

        chunk.pointsBuffer.SetData(points);
        chunk.valuesChanged = true;
    }

    void SetToNearestPoint(Transform objTransform, Vector4[] points)
    {
        float nearestDistSqr = float.MaxValue;
        Vector3 nearestPoint = objTransform.position;

        // Find nearest point
        for (int i = 0; i < points.Length; i++)
        {
            Vector3 pointPos = new Vector3(points[i].x, points[i].y, points[i].z);
            float distSqr = (objTransform.position - pointPos).sqrMagnitude;

            if (distSqr < nearestDistSqr)
            {
                nearestDistSqr = distSqr;
                nearestPoint = pointPos;
            }
        }

        objTransform.position = nearestPoint;
    }

    public void ClearObjects()
    {
        foreach (Transform child in objectHolder.transform)
        {
            Destroy(child.gameObject);
        }
    }

    bool IsEdgePoint(int i)
    {
        int x = i % worldSettings.numPointsPerAxis;
        int y = (i / worldSettings.numPointsPerAxis) % worldSettings.numPointsPerAxis;
        int z = i / (worldSettings.numPointsPerAxis * worldSettings.numPointsPerAxis);

        return x == 0 ||
               y == 0 ||
               z == 0 ||
               x == worldSettings.numPointsPerAxis - 1 ||
               y == worldSettings.numPointsPerAxis - 1 ||
               z == worldSettings.numPointsPerAxis - 1;
    }
}
