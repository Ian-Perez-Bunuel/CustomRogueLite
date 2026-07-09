using UnityEngine;

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
        Point[] points = new Point[numPoints];
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
                    Vector3 pos = points[i].position;
                    Vector3 closestPoint = col.ClosestPoint(pos);
                    float dist = Vector3.Distance(closestPoint, pos);

                    bool insideCol = dist < 0.000001f;

                    float newValue = points[i].density;

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
                    points[i].density = Mathf.Max(points[i].density, newValue);
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
        chunk.HasChanged();
    }

    public void DistortChunk()
    {
        int numPoints = chunk.GetNumberOfPoints();
        Point[] points = new Point[numPoints];
        chunk.pointsBuffer.GetData(points);

        for (int i = 0; i < points.Length; i++)
        {
            Vector3 pos = points[i].position;

            // Only distort near the surface
            if (points[i].density > 0.1f && points[i].density < 0.8f && !IsEdgePoint(i))
            {
                float noise =
                Mathf.PerlinNoise(pos.x * distortionScale, pos.y * distortionScale) +
                Mathf.PerlinNoise(pos.y * distortionScale, pos.z * distortionScale) +
                Mathf.PerlinNoise(pos.x * distortionScale, pos.z * distortionScale);

                noise = noise / 3f * 2f - 1f;
                points[i].density += noise * distortionAmount;
            }
        }

        chunk.pointsBuffer.SetData(points);
        chunk.HasChanged();
    }

    void SetToNearestPoint(Transform objTransform, Point[] points)
    {
        float nearestDistSqr = float.MaxValue;
        Vector3 nearestPoint = objTransform.position;

        // Find nearest point
        for (int i = 0; i < points.Length; i++)
        {
            Vector3 pointPos = points[i].position;
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

    public void SaveAsScriptableObject()
    {
        DestructableSO newSO = ScriptableObject.CreateInstance<DestructableSO>();
        int numPoints = chunk.GetNumberOfPoints();
        Point[] points = new Point[numPoints];
        chunk.pointsBuffer.GetData(points); // Set point data

        // Get center position of object
        float lowestYPos = 99999f;
        float objectRadius = 0f;
        Vector3 totalPosition = Vector3.zero;
        int pointCount = 0;

        // Loop through all points to get lowestYPos, totalPos, objectRadius and pointCount;
        for (int i = 0; i < points.Length; i++)
        {
            if (points[i].density >= 0.9f)
            {
                // Total Pos
                totalPosition += points[i].position;
                pointCount++;

                // Lowest Point
                if (points[i].position.y < lowestYPos)
                {
                    lowestYPos = points[i].position.y;
                }
            }
        }

        Vector3 centerPos = Vector3.zero;
        if (pointCount > 0)
            centerPos = totalPosition / pointCount;

        // Object radius / farthest distance
        for (int i = 0; i < points.Length; i++)
        {
            if (points[i].density > 0f)
            {
                float dist = Vector3.Distance(centerPos, points[i].position);

                if (dist > objectRadius)
                {
                    objectRadius = dist;
                }
            }
        }

        // Spawn position
        Vector3 spawnPos = new Vector3(centerPos.x, lowestYPos, centerPos.z);

        // Set destructable data
        newSO.points = points;
        newSO.localSpawnPos = spawnPos;
        newSO.radius = objectRadius;

        // Save to a folder
    }
}
