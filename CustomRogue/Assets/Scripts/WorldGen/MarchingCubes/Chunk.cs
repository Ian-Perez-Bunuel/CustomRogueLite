using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public enum PointMaterial
{
    Stone, // Default
    Grass,
}

[StructLayout(LayoutKind.Sequential)]
public struct Point
{
    public Vector3 position;
    public float density;
    public int material;
}

public class Chunk : MonoBehaviour
{
    // Mesh
    MeshFilter meshFilter;
    MeshRenderer meshRenderer;
    MeshCollider meshCollider;
    List<int>[] submeshTriangles;

    public ComputeBuffer originalPointsBuffer = null;
    public ComputeBuffer pointsBuffer;
    int numPoints;

    Vector3Int coords;

    bool colliderUpdated = false;

    //[Header("Regen")]
    //static ComputeShader regenComputeShader;
    //static float regenAmount = 0.01f;
    //public int timeTillRegen = 2; // Seconds
    //public float regenSpeed = 0.5f; // Time between regen changes

    //public static void SetRegenCompute(ComputeShader shader)
    //{
    //    regenComputeShader = shader;
    //    regenComputeShader.SetFloat("regenAmount", regenAmount);
    //}

    public void SetSubmeshAmount(int amount)
    {
        submeshTriangles = new List<int>[amount];

        for (int i = 0; i < amount; i++)
        {
            submeshTriangles[i] = new List<int>();
        }
    }
    public void AddVertexIndexTo(int matIndex, int vertIndex)
    {
        submeshTriangles[matIndex].Add(vertIndex);
    }
    public List<int> GetTris(int i)
    {
        return submeshTriangles[i];
    }
    public void ClearTris()
    {
        for (int i = 0; i < submeshTriangles.Length; i++)
        {
            submeshTriangles[i].Clear();
        }
    }

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

        //regenComputeShader.SetBuffer(0, "points", pointsBuffer);
        //regenComputeShader.SetBuffer(0, "originalPoints", originalPointsBuffer);

        gameObject.SetActive(false);
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

    public void ColliderChanged()
    {
        colliderUpdated = false;
    }
    public bool IsColliderUpdated()
    {
        return colliderUpdated;
    }
    public void SetCollider()
    {
        meshCollider.sharedMesh = meshFilter.sharedMesh;
        colliderUpdated = true;
    }

    // Regen
    public void HasChanged()
    {
        MarchingCubesCompute.dirtyChunks.Enqueue(this);
        //StopCoroutine(RegenTimer()); // Reset timer

        //StartCoroutine(RegenTimer());
    }
    //IEnumerator RegenTimer()
    //{
    //    yield return new WaitForSeconds(timeTillRegen);

    //    StartCoroutine(Regenerate());
    //}
    //IEnumerator Regenerate()
    //{
    //    yield return new WaitForSeconds(regenSpeed);

    //    regenComputeShader.Dispatch(0, MarchingCubesCompute.numThreadsPerAxis, MarchingCubesCompute.numThreadsPerAxis, MarchingCubesCompute.numThreadsPerAxis);
    //    MarchingCubesCompute.dirtyChunks.Enqueue(this);
    //}

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