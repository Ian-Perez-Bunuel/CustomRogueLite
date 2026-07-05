using System.Runtime.InteropServices;
using UnityEngine;

public enum PointMaterial
{
    Stone, // Default
    Grass,
}

public class Chunk : MonoBehaviour
{
    [StructLayout(LayoutKind.Sequential)]
    struct Point
    {
        public Vector3 position;
        public float density;
        public int material;
    }

    // Mesh
    MeshFilter meshFilter;
    MeshRenderer meshRenderer;
    MeshCollider meshCollider;

    public ComputeBuffer originalPointsBuffer = null;
    public ComputeBuffer pointsBuffer;
    int numPoints;

    Vector3Int coords;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Setup(Material[] mats, int t_numPointsPerAxis)
    {
        if (GetComponent<MeshFilter>() == null)
        {
            meshFilter = gameObject.AddComponent<MeshFilter>();
        }
        if (GetComponent<MeshCollider>() == null)
        {
            meshCollider = gameObject.AddComponent<MeshCollider>();
        }
        if (GetComponent<MeshRenderer>() == null)
        {
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
        }

        meshRenderer.sharedMaterials = mats;

        // Buffer
        numPoints = t_numPointsPerAxis * t_numPointsPerAxis * t_numPointsPerAxis;
        pointsBuffer = new ComputeBuffer(numPoints, sizeof(float) * 4 + sizeof(int)); // float3 + float + int
        originalPointsBuffer = new ComputeBuffer(numPoints, sizeof(float) * 4 + sizeof(int)); // float3 + float + int
    }

    public void SetOriginalPoints()
    {
        if (originalPointsBuffer == null)
            originalPointsBuffer = pointsBuffer;
    }

    public int GetNumberOfPoints() { return numPoints; }

    public void SetCoords(Vector3Int c)
    {
        coords = c;
    }
    public Vector3Int GetCoords()
    { return coords; }

    public void SetMesh(Mesh mesh)
    {
        meshFilter.sharedMesh = mesh;
        meshCollider.sharedMesh = meshFilter.sharedMesh;
    }

    public Mesh GetMesh()
    {
        return meshFilter.sharedMesh;
    }

    public Vector3 GetOrigin(float boundsSize)
    {
        return new Vector3(coords.x, coords.y, coords.z) * boundsSize;
    }

    public void SetCollider()
    {
        meshCollider.sharedMesh = meshFilter.sharedMesh;
    }

    public void ReleaseBuffers()
    {
        if (pointsBuffer != null)
        {
            pointsBuffer.Release();
            pointsBuffer = null;
        }

        if (originalPointsBuffer != null)
        {
            originalPointsBuffer.Release();
            originalPointsBuffer = null;
        }
    }
}