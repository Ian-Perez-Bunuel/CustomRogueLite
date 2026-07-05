using System.Buffers;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class MarchingCubesCompute : MonoBehaviour
{
    [StructLayout(LayoutKind.Sequential)]
    struct Triangle
    {
#pragma warning disable 649 // disable unassigned variable warning
        public Vector3 a;
        public Vector3 b;
        public Vector3 c;

        public int material;

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
    public Vector3 worldDimensions;
    [SerializeField] GameObject chunkHolder;
    List<Chunk> chunks;
    public static Queue<Chunk> dirtyChunks;

    [Header("Voxel Settings")]
    public Material[] materials;
    public WorldSettings worldSettings;

    // Buffers
    int kernel;
    ComputeBuffer triangleBuffer;
    ComputeBuffer triCountBuffer;
    int numThreadsPerAxis;

    public float GetSurfaceLevel()
    {
        return worldSettings.surfaceLevel;
    }

    public void AddToDirtyChunks(Chunk c)
    {
        dirtyChunks.Enqueue(c);
    }

    public Vector3 GetDimensions()
    {
        Vector3 dimensions = worldDimensions;
        dimensions *= worldSettings.boundsSize;

        return dimensions;
    }

    public Vector3 GetWorldCenter()
    {
        return (worldDimensions - Vector3.one) * worldSettings.boundsSize * 0.5f;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CreateBuffers();
        kernel = computeShader.FindKernel("March");

        chunks = new List<Chunk>();
        dirtyChunks = new Queue<Chunk>();

        int numVoxelsPerAxis = worldSettings.numPointsPerAxis - 1;
        numThreadsPerAxis = Mathf.CeilToInt(numVoxelsPerAxis / 4f);

        UpdateWorld();
    }

    public Vector3 CentreFromCoord(Vector3Int coord)
    {
        return new Vector3(coord.x, coord.y, coord.z) * worldSettings.boundsSize;
    }

    void DispatchComputeShader(Chunk chunk)
    {
        triangleBuffer.SetCounterValue(0);
        computeShader.SetBuffer(kernel, "points", chunk.pointsBuffer);
        computeShader.SetBuffer(kernel, "triangles", triangleBuffer);
        computeShader.SetInt("numPointsPerAxis", worldSettings.numPointsPerAxis);
        computeShader.SetFloat("surfaceLevel", worldSettings.surfaceLevel);

        computeShader.Dispatch(kernel, numThreadsPerAxis, numThreadsPerAxis, numThreadsPerAxis);
    }

    // Builds the mesh without re-generating it's noise. Instead goes off of it's current point values
    public void RebuildMesh(Chunk chunk)
    {
        DispatchComputeShader(chunk);

        // Get number of triangles in the triangle buffer
        ComputeBuffer.CopyCount(triangleBuffer, triCountBuffer, 0);
        int[] triCountArray = { 0 };
        triCountBuffer.GetData(triCountArray);
        int numTris = triCountArray[0];

        // Get triangle data from shader
        Triangle[] tris = ArrayPool<Triangle>.Shared.Rent(numTris);
        Vector3[] meshVertices = ArrayPool<Vector3>.Shared.Rent(numTris * 3);
        triangleBuffer.GetData(tris, 0, 0, numTris);

        // One triangle index list per material
        chunk.ClearTris();

        for (int i = 0; i < numTris; i++)
        {
            int matIndex = Mathf.Clamp(tris[i].material, 0, materials.Length - 1);

            for (int j = 0; j < 3; j++)
            {
                int vertIndex = i * 3 + j;
                meshVertices[vertIndex] = tris[i][j];

                chunk.AddVertexIndexTo(matIndex, vertIndex);
            }
        }

        Mesh mesh = chunk.GetMesh();
        mesh.Clear(false);

        mesh.SetVertices(meshVertices, 0, numTris * 3);
        mesh.subMeshCount = materials.Length;

        for (int m = 0; m < materials.Length; m++)
        {
            mesh.SetTriangles(chunk.GetTris(m), m);
        }

        ArrayPool<Triangle>.Shared.Return(tris);
        ArrayPool<Vector3>.Shared.Return(meshVertices);

        mesh.RecalculateNormals();
        chunk.SetCollider();
    }

    void GenerateMesh(Chunk chunk)
    {
        Vector3Int coord = chunk.GetCoords();
        Vector3 centre = CentreFromCoord(coord);

        // Put in build chunk
        float pointSpacing = worldSettings.boundsSize / (worldSettings.numPointsPerAxis - 1);
        densityGenerator.Generate(chunk.pointsBuffer, worldSettings.numPointsPerAxis, worldSettings.boundsSize, worldDimensions, Vector3.zero, centre, worldSettings.offset, pointSpacing);

        DispatchComputeShader(chunk);

        // Get number of triangles in the triangle buffer
        ComputeBuffer.CopyCount(triangleBuffer, triCountBuffer, 0);
        int[] triCountArray = { 0 };
        triCountBuffer.GetData(triCountArray);
        int numTris = triCountArray[0];

        // Get triangle data from shader
        Triangle[] tris = new Triangle[numTris];
        triangleBuffer.GetData(tris, 0, 0, numTris);
        var meshVertices = new Vector3[numTris * 3];
        // One triangle index list per material
        chunk.SetSubmeshAmount(materials.Length);

        for (int i = 0; i < numTris; i++)
        {
            int matIndex = Mathf.Clamp(tris[i].material, 0, materials.Length - 1);

            for (int j = 0; j < 3; j++)
            {
                int vertIndex = i * 3 + j;
                meshVertices[vertIndex] = tris[i][j];
                chunk.AddVertexIndexTo(matIndex, vertIndex);
            }
        }

        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = meshVertices;
        mesh.subMeshCount = materials.Length;

        for (int m = 0; m < materials.Length; m++)
        {
            mesh.SetTriangles(chunk.GetTris(m), m);
        }

        mesh.RecalculateNormals();
        chunk.SetMesh(mesh);
    }

    void CreateBuffers()
    {
        int numPoints = worldSettings.numPointsPerAxis * worldSettings.numPointsPerAxis * worldSettings.numPointsPerAxis;
        int numVoxelsPerAxis = worldSettings.numPointsPerAxis - 1;
        int numVoxels = numVoxelsPerAxis * numVoxelsPerAxis * numVoxelsPerAxis;
        int maxTriangleCount = numVoxels * 5;

        triangleBuffer = new ComputeBuffer(maxTriangleCount, sizeof(float) * 3 * 3 + sizeof(int), ComputeBufferType.Append);
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
        float s = worldSettings.boundsSize;

        int x = Mathf.FloorToInt((pos.x + s * 0.5f) / s);
        int y = Mathf.FloorToInt((pos.y + s * 0.5f) / s);
        int z = Mathf.FloorToInt((pos.z + s * 0.5f) / s);

        return new Vector3Int(x, y, z);
    }
    public Chunk GetChunkFromWorldPos(Vector3 worldPos)
    {
        Vector3Int coord = WorldToChunkCoord(worldPos);

        if (coord.x < 0 || coord.x >= worldDimensions.x ||
            coord.y < 0 || coord.y >= worldDimensions.y ||
            coord.z < 0 || coord.z >= worldDimensions.z)
        {
            return null; // Out of the world
        }

        int index = coord.x * ((int)worldDimensions.y * (int)worldDimensions.z) + coord.y * (int)worldDimensions.z + coord.z;

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
        if (chunks.Count != worldDimensions.x * worldDimensions.y * worldDimensions.z)
        {
            // Create new chunks
            for (int x = 0; x < worldDimensions.x; x++)
            {
                for (int y = 0; y < worldDimensions.y; y++)
                {
                    for (int z = 0; z < worldDimensions.z; z++)
                    {
                        Chunk chunk = CreateChunk(new Vector3Int(x, y, z));
                        chunk.Setup(materials, worldSettings.numPointsPerAxis);
                        chunks.Add(chunk);
                    }
                }
            }
        }

        // Give meshes
        BuildAllChunks();
    }

    private void FixedUpdate()
    {
        while (dirtyChunks.Count > 0)
        {
            Chunk chunk = dirtyChunks.Dequeue();
            RebuildMesh(chunk);
        }
    }

    public Vector3 GetChunkDimensions()
    {
        return Vector3.one * worldSettings.boundsSize;
    }

    public Chunk GetChunkFromCoord(Vector3Int coord)
    {
        if (coord.x < 0 || coord.x >= worldDimensions.x ||
            coord.y < 0 || coord.y >= worldDimensions.y ||
            coord.z < 0 || coord.z >= worldDimensions.z)
        {
            return null;
        }

        int index = coord.x * ((int)worldDimensions.y * (int)worldDimensions.z)
                  + coord.y * (int)worldDimensions.z
                  + coord.z;

        return chunks[index];
    }

    public void EditSphere(ComputeShader computeEditting, Vector3 worldPos, float radius)
    {
        // World-space AABB of the sphere
        Vector3 min = worldPos - Vector3.one * radius;
        Vector3 max = worldPos + Vector3.one * radius;

        // Convert to chunk coordinates
        Vector3Int minCoord = WorldToChunkCoord(min);
        Vector3Int maxCoord = WorldToChunkCoord(max);

        // Clamp to world bounds
        minCoord.x = Mathf.Clamp(minCoord.x, 0, (int)worldDimensions.x - 1);
        minCoord.y = Mathf.Clamp(minCoord.y, 0, (int)worldDimensions.y - 1);
        minCoord.z = Mathf.Clamp(minCoord.z, 0, (int)worldDimensions.z - 1);

        maxCoord.x = Mathf.Clamp(maxCoord.x, 0, (int)worldDimensions.x - 1);
        maxCoord.y = Mathf.Clamp(maxCoord.y, 0, (int)worldDimensions.y - 1);
        maxCoord.z = Mathf.Clamp(maxCoord.z, 0, (int)worldDimensions.z - 1);

        int numVoxelsPerAxis = worldSettings.numPointsPerAxis - 1;
        int numThreadsPerAxis = Mathf.CeilToInt(numVoxelsPerAxis / 8f);

        // Loop through all potentially affected chunks
        for (int x = minCoord.x; x <= maxCoord.x; x++)
        {
            for (int y = minCoord.y; y <= maxCoord.y; y++)
            {
                for (int z = minCoord.z; z <= maxCoord.z; z++)
                {
                    Chunk chunk = GetChunkFromCoord(new Vector3Int(x, y, z));
                    if (chunk == null)
                        continue;

                    // This uses world-space positions stored in the buffer,
                    // so we can pass the same worldPos & radius to every chunk.
                    computeEditting.SetBuffer(0, "points", chunk.pointsBuffer);
                    computeEditting.SetInt("numPointsPerAxis", worldSettings.numPointsPerAxis);
                    // Other variables are passed in the terraformer

                    computeEditting.Dispatch(0, numThreadsPerAxis, numThreadsPerAxis, numThreadsPerAxis);
                    
                    dirtyChunks.Enqueue(chunk);
                }
            }
        }
    }

    public void EditChunk(ComputeShader computeEditting, Chunk chunk)
    {
        if (chunk == null)
            return;

        int editKernel = computeEditting.FindKernel("CSMain");

        int numVoxelsPerAxis = worldSettings.numPointsPerAxis - 1;
        int numThreadsPerAxis = Mathf.CeilToInt(numVoxelsPerAxis / 8f);

        computeEditting.SetBuffer(editKernel, "points", chunk.pointsBuffer);
        computeEditting.SetInt("numPointsPerAxis", worldSettings.numPointsPerAxis);

        computeEditting.Dispatch(editKernel, numThreadsPerAxis, numThreadsPerAxis, numThreadsPerAxis);

        dirtyChunks.Enqueue(chunk);
    }

    public void EditTunnel(ComputeShader computeEditting, Vector3 cylinderStart, Vector3 cylinderEnd, float startRadius, float endRadius)
    {
        float maxRadius = Mathf.Max(startRadius, endRadius);

        // World-space AABB of the rotated tapered cylinder
        Vector3 min = Vector3.Min(cylinderStart, cylinderEnd) - Vector3.one * maxRadius;
        Vector3 max = Vector3.Max(cylinderStart, cylinderEnd) + Vector3.one * maxRadius;

        // Convert to chunk coordinates
        Vector3Int minCoord = WorldToChunkCoord(min);
        Vector3Int maxCoord = WorldToChunkCoord(max);

        // Clamp to world bounds
        minCoord.x = Mathf.Clamp(minCoord.x, 0, (int)worldDimensions.x - 1);
        minCoord.y = Mathf.Clamp(minCoord.y, 0, (int)worldDimensions.y - 1);
        minCoord.z = Mathf.Clamp(minCoord.z, 0, (int)worldDimensions.z - 1);

        maxCoord.x = Mathf.Clamp(maxCoord.x, 0, (int)worldDimensions.x - 1);
        maxCoord.y = Mathf.Clamp(maxCoord.y, 0, (int)worldDimensions.y - 1);
        maxCoord.z = Mathf.Clamp(maxCoord.z, 0, (int)worldDimensions.z - 1);

        int numVoxelsPerAxis = worldSettings.numPointsPerAxis - 1;
        int numThreadsPerAxis = Mathf.CeilToInt(numVoxelsPerAxis / 8f);

        // Loop through all potentially affected chunks
        for (int x = minCoord.x; x <= maxCoord.x; x++)
        {
            for (int y = minCoord.y; y <= maxCoord.y; y++)
            {
                for (int z = minCoord.z; z <= maxCoord.z; z++)
                {
                    Chunk chunk = GetChunkFromCoord(new Vector3Int(x, y, z));
                    if (chunk == null)
                        continue;

                    computeEditting.SetBuffer(0, "points", chunk.pointsBuffer);
                    computeEditting.SetInt("numPointsPerAxis", worldSettings.numPointsPerAxis);

                    computeEditting.SetVector("cylinderStart", cylinderStart);
                    computeEditting.SetVector("cylinderEnd", cylinderEnd);
                    computeEditting.SetFloat("startRadius", startRadius);
                    computeEditting.SetFloat("endRadius", endRadius);

                    computeEditting.Dispatch(kernel, numThreadsPerAxis, numThreadsPerAxis, numThreadsPerAxis);

                    dirtyChunks.Enqueue(chunk);
                }
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
