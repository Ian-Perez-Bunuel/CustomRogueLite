using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class MarchingCubesCompute : MonoBehaviour
{
    [StructLayout(LayoutKind.Sequential)]
    struct Triangle
    {
#pragma warning disable 649 // disable unassigned variable warning
        public Vector3 a;
        public Vector3 b;
        public Vector3 c;

        // Allows indexing
        public Vector3 this[int i]
        {
            get
            {
                switch (i)
                {
                    case 0:
                        return a;
                    case 1:
                        return b;
                    default:
                        return c;
                }
            }
        }
    }

    [Header("Compute Shader")]
    public ComputeShader computeShader;

    [Header("Generation Data")]
    public DensityGenerator densityGenerator;
    // Amount of boxes per axis
    public Vector3 worldBounds;
    [SerializeField] GameObject chunkHolder;
    List<Chunk> chunks;

    [Header("Voxel Settings")]
    public Material material;
    public float surfaceLevel; // If value > surfaceLevel then it's solid
    public float boundsSize = 1;
    public Vector3 offset = Vector3.zero;

    [Range(2, 100)]
    public int numPointsPerAxis = 30;

    // Buffers
    int kernel;
    ComputeBuffer triangleBuffer;
    ComputeBuffer triCountBuffer;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CreateBuffers();
        kernel = computeShader.FindKernel("March");
        
        computeShader.SetFloat("surfaceLevel", surfaceLevel);
        computeShader.SetInt("numPointsPerAxis", numPointsPerAxis);

        chunks = new List<Chunk>();
    }

    Vector3 CentreFromCoord(Vector3Int coord)
    {
        return new Vector3(coord.x, coord.y, coord.z) * boundsSize;
    }

    // Builds the mesh without re-generating it's noise. Instead goes off of it's current point values
    // Could call this when an event is called (Invoke). So when ground is broken or built on
    void RebuildMesh(Chunk chunk)
    {
        int numVoxelsPerAxis = numPointsPerAxis - 1;
        int numThreadsPerAxis = Mathf.CeilToInt(numVoxelsPerAxis / 8f);

        triangleBuffer.SetCounterValue(0);
        computeShader.SetBuffer(kernel, "points", chunk.pointsBuffer);
        computeShader.SetBuffer(kernel, "triangles", triangleBuffer);
        computeShader.SetInt("numPointsPerAxis", numPointsPerAxis);
        computeShader.SetFloat("surfaceLevel", surfaceLevel);

        computeShader.Dispatch(kernel, numThreadsPerAxis, numThreadsPerAxis, numThreadsPerAxis);

        // Get number of triangles in the triangle buffer
        ComputeBuffer.CopyCount(triangleBuffer, triCountBuffer, 0);
        int[] triCountArray = { 0 };
        triCountBuffer.GetData(triCountArray);
        int numTris = triCountArray[0];

        // Get triangle data from shader
        Triangle[] tris = new Triangle[numTris];
        triangleBuffer.GetData(tris, 0, 0, numTris);

        var meshVertices = new Vector3[numTris * 3];
        var meshTriangles = new int[numTris * 3];

        for (int i = 0; i < numTris; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                meshTriangles[i * 3 + j] = i * 3 + j;
                meshVertices[i * 3 + j] = tris[i][j];
            }
        }

        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = meshVertices;
        mesh.triangles = meshTriangles;
        mesh.RecalculateNormals();
        chunk.SetMesh(mesh);
        chunk.valuesChanged = false;
    }


    void GenerateMesh(Chunk chunk)
    {
        int numVoxelsPerAxis = numPointsPerAxis - 1;
        int numThreadsPerAxis = Mathf.CeilToInt(numVoxelsPerAxis / 8f);

        Vector3Int coord = chunk.GetCoords();
        Vector3 centre = CentreFromCoord(coord);

        // Put in build chunk
        float pointSpacing = boundsSize / (numPointsPerAxis - 1);
        densityGenerator.Generate(chunk.pointsBuffer, numPointsPerAxis, boundsSize, worldBounds, centre, offset, pointSpacing);

        triangleBuffer.SetCounterValue(0);
        computeShader.SetBuffer(kernel, "points", chunk.pointsBuffer);
        computeShader.SetBuffer(kernel, "triangles", triangleBuffer);
        computeShader.SetInt("numPointsPerAxis", numPointsPerAxis);
        computeShader.SetFloat("surfaceLevel", surfaceLevel);

        computeShader.Dispatch(kernel, numThreadsPerAxis, numThreadsPerAxis, numThreadsPerAxis);

        // Get number of triangles in the triangle buffer
        ComputeBuffer.CopyCount(triangleBuffer, triCountBuffer, 0);
        int[] triCountArray = { 0 };
        triCountBuffer.GetData(triCountArray);
        int numTris = triCountArray[0];

        // Get triangle data from shader
        Triangle[] tris = new Triangle[numTris];
        triangleBuffer.GetData(tris, 0, 0, numTris);

        var meshVertices = new Vector3[numTris * 3];
        var meshTriangles = new int[numTris * 3];

        for (int i = 0; i < numTris; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                meshTriangles[i * 3 + j] = i * 3 + j;
                meshVertices[i * 3 + j] = tris[i][j];
            }
        }

        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = meshVertices;
        mesh.triangles = meshTriangles;
        mesh.RecalculateNormals();
        chunk.SetMesh(mesh);
    }

    void CreateBuffers()
    {
        int numPoints = numPointsPerAxis * numPointsPerAxis * numPointsPerAxis;
        int numVoxelsPerAxis = numPointsPerAxis - 1;
        int numVoxels = numVoxelsPerAxis * numVoxelsPerAxis * numVoxelsPerAxis;
        int maxTriangleCount = numVoxels * 5;

        triangleBuffer = new ComputeBuffer(maxTriangleCount, sizeof(float) * 3 * 3, ComputeBufferType.Append);
        triCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
    }

    void ReleaseBuffers()
    {
        if (triangleBuffer != null)
        {
            triangleBuffer.Release();
            triangleBuffer = null;
        }

        if (triCountBuffer != null)
        {
            triCountBuffer.Release();
            triCountBuffer = null;
        }
    }

    Vector3Int WorldToChunkCoord(Vector3 pos)
    {
        float s = boundsSize;

        int x = Mathf.FloorToInt((pos.x + s * 0.5f) / s);
        int y = Mathf.FloorToInt((pos.y + s * 0.5f) / s);
        int z = Mathf.FloorToInt((pos.z + s * 0.5f) / s);

        return new Vector3Int(x, y, z);
    }
    public Chunk GetChunkFromWorldPos(Vector3 worldPos)
    {
        Vector3Int coord = WorldToChunkCoord(worldPos);

        if (coord.x < 0 || coord.x >= worldBounds.x ||
            coord.y < 0 || coord.y >= worldBounds.y ||
            coord.z < 0 || coord.z >= worldBounds.z)
        {
            return null; // Out of the world
        }

        int index = coord.x * ((int)worldBounds.y * (int)worldBounds.z) + coord.y * (int)worldBounds.z + coord.z;

        return chunks[index];
    }

    Chunk CreateChunk(Vector3Int coord)
    {
        GameObject chunk = new GameObject($"Chunk ({coord.x}, {coord.y}, {coord.z})");
        chunk.transform.parent = chunkHolder.transform;
        Chunk newChunk = chunk.AddComponent<Chunk>();
        newChunk.gameObject.layer = newChunk.transform.parent.gameObject.layer;
        newChunk.SetCoords(coord);
        return newChunk;
    }

    public void UpdateWorld()
    {
        if (chunks.Count != worldBounds.x * worldBounds.y * worldBounds.z)
        {
            // Create new chunks
            for (int x = 0; x < worldBounds.x; x++)
            {
                for (int y = 0; y < worldBounds.y; y++)
                {
                    for (int z = 0; z < worldBounds.z; z++)
                    {
                        Chunk chunk = CreateChunk(new Vector3Int(x, y, z));
                        chunk.Setup(material, numPointsPerAxis);
                        chunks.Add(chunk);
                    }
                }
            }
        }
        // Give meshes
        BuildAllChunks();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.F))
        {
            UpdateWorld();
        }

        foreach(Chunk chunk in chunks)
        {
            if (chunk.valuesChanged)
            {
                RebuildMesh(chunk);
            }
        }
    }

    void BuildAllChunks()
    {
        foreach (Chunk chunk in chunks)
        {
            GenerateMesh(chunk);
        }
    }

    void OnDestroy()
    {
        ReleaseBuffers();

        foreach (Chunk chunk in chunks)
        {
            chunk.ReleaseBuffers();
        }
    }
}
