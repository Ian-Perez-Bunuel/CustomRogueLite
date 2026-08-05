using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

public enum PointMaterial
{
    Stone, // Default
    Grass,
}

[StructLayout(LayoutKind.Sequential)]
public struct Point
{
    public float3 position;
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

    Vector3Int coords;

    NativeArray<Point> points;
    int numPoints;

    [Header("Job Handling")]
    // Tracks the most recent job using this chunk's points.
    private JobHandle pointDependency;
    // Stops the same chunk being placed into the dirty queue repeatedly.
    private bool queuedForRebuild;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Setup(Material[] t_mats, int t_numPointsPerAxis)
    {
        SetOrCreateComponents();

        meshRenderer.sharedMaterials = t_mats;

        SetSubmeshAmount(t_mats.Length);

        AllocatePointData(t_numPointsPerAxis);

        CreateMeshIfRequired();
    }

    void SetOrCreateComponents()
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
    }

    private void CreateMeshIfRequired()
    {
        if (meshFilter.sharedMesh != null)
            return;

        Mesh mesh = new Mesh
        {
            name = $"Marching Cubes: " + gameObject.name,
            indexFormat = IndexFormat.UInt32
        };

        // Since the mesh will be editted
        mesh.MarkDynamic();

        meshFilter.sharedMesh = mesh;
        meshCollider.sharedMesh = mesh;
    }

    private void AllocatePointData(int numPointsPerAxis)
    {
        CompletePointJobs();
        DisposePointData();

        numPoints =
            numPointsPerAxis *
            numPointsPerAxis *
            numPointsPerAxis;

        points = new NativeArray<Point>(
            numPoints,
            Allocator.Persistent,
            NativeArrayOptions.UninitializedMemory);
    }

    #region Point-data Job Handling
    public int GetNumberOfPoints() { return numPoints; }

    public void SetPointDependency(JobHandle handle)
    {
        pointDependency = handle;
    }

    public JobHandle GetPointDependency()
    {
        return pointDependency;
    }
    public void CompletePointJobs()
    {
        pointDependency.Complete();
        pointDependency = default;
    }

    public NativeArray<Point> GetPoints()
    {
        return points;
    }
    #endregion

    #region Submeshes
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
    #endregion

    #region Coordinates
    public void SetCoords(Vector3Int c)
    {
        coords = c;
    }

    public Vector3Int GetCoords()
    { 
        return coords; 
    }

    public Vector3 GetOrigin(float boundsSize)
    {
        return new Vector3(coords.x, coords.y, coords.z) * boundsSize;
    }
    #endregion

    #region Mesh
    public void SetMesh(Mesh mesh)
    {
        meshFilter.sharedMesh = mesh;

        meshCollider.sharedMesh = null;
        meshCollider.sharedMesh = mesh;
    }

    public Mesh GetMesh()
    {
        return meshFilter.sharedMesh;
    }

    public void SetCollider()
    {
        // Assigning null first forces Unity to refresh the collider.
        meshCollider.sharedMesh = null;
        meshCollider.sharedMesh = meshFilter.sharedMesh;
    }
    #endregion

    #region Dirty State
    public void HasChanged()
    {
        if (queuedForRebuild)
            return;

        queuedForRebuild = true;

        MarchingCubesCompute.dirtyChunks.Enqueue(this);
    }

    public void MarkRebuilt()
    {
        queuedForRebuild = false;
    }

    #endregion

    #region Cleanup
    public void ReleaseBuffers()
    {
        CompletePointJobs();
        DisposePointData();
    }

    private void DisposePointData()
    {
        if (points.IsCreated)
            points.Dispose();
    }

    private void OnDestroy()
    {
        ReleaseBuffers();
    }
    #endregion
}