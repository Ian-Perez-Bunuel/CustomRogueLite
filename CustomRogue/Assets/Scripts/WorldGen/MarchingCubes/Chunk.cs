using UnityEngine;
using static UnityEditor.PlayerSettings;

public class Chunk : MonoBehaviour
{
    // Mesh
    MeshFilter meshFilter;
    MeshRenderer meshRenderer;
    MeshCollider meshCollider;

    public ComputeBuffer pointsBuffer;
    int numPoints;

    [HideInInspector] public bool valuesChanged = false;

    Vector3Int coords;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Setup(Material mat, int t_numPointsPerAxis)
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

        meshRenderer.material = mat;

        // Buffer
        numPoints = t_numPointsPerAxis * t_numPointsPerAxis * t_numPointsPerAxis;
        pointsBuffer = new ComputeBuffer(numPoints, sizeof(float) * 4);
    }


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

    public Mesh GetMesh() { return meshFilter.sharedMesh; }

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
    }
}
