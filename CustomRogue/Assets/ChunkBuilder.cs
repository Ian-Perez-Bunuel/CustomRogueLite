using System.Drawing;
using System.Runtime.InteropServices;
using UnityEngine;

public class ChunkBuilder : MonoBehaviour
{
    [SerializeField] MarchingCubesCompute chunkCanvas;
    [SerializeField] WorldSettings worldSettings;
    Chunk chunk;

    [Header("Object settings")]
    [SerializeField] GameObject objectHolder;

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
            // Check if it has a collider
            if (child.TryGetComponent(out Collider col))
            {
                // Check all points
                for (int i = 0; i < points.Length; i++)
                {
                    // Don't change if already set
                    if (points[i].w > 0)
                        continue;

                    Vector3 pos = new Vector3(points[i].x, points[i].y, points[i].z);

                    if ((col.ClosestPoint(pos) - pos).sqrMagnitude < 0.000001f)
                    {
                        Debug.Log("Point was within collider");
                        points[i].w = 1f; // Set density
                    }
                }

                chunk.pointsBuffer.SetData(points);
            }
            else
            {
                Debug.LogError("No collider found in: " + child.name);
            }
        }

        // Set chunk as changed
        chunk.valuesChanged = true;
    }
}
