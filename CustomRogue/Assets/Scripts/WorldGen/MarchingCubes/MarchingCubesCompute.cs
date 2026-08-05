using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class MarchingCubesCompute : MonoBehaviour
{
    [Header("Compute Shader")]
    public ComputeShader computeShader;
    [SerializeField] ComputeShader regenComputeShader;

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

    public static int numThreadsPerAxis;

    private const int MaxTrianglesPerCube = 5;
    private const int MarchBatchSize = 32;

    private NativeArray<int> triTableNative;
    private NativeArray<int> cornerAFromEdgeNative;
    private NativeArray<int> cornerBFromEdgeNative;

    public float GetSurfaceLevel()
    {
        return worldSettings.surfaceLevel;
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

    private void Awake()
    {
        chunks = new List<Chunk>();
        dirtyChunks = new Queue<Chunk>();

        CreateLookupTables();
    }
    private void Start()
    {
        UpdateWorld();
    }

    public Vector3 CentreFromCoord(Vector3Int coord)
    {
        return new Vector3(coord.x, coord.y, coord.z) * worldSettings.boundsSize;
    }


    // Builds the mesh without re-generating it's noise. Instead goes off of it's current point values
    public void RebuildMesh(Chunk chunk)
    {
        if (chunk == null)
            return;

        NativeArray<Point> points = chunk.GetPoints();

        if (!points.IsCreated)
        {
            Debug.LogError(
                $"Cannot rebuild {chunk.name}: its point NativeArray is not created.",
                chunk);

            chunk.MarkRebuilt();
            return;
        }

        int pointsPerAxis = worldSettings.numPointsPerAxis;
        int cubesPerAxis = pointsPerAxis - 1;

        if (cubesPerAxis <= 0)
        {
            Debug.LogError(
                "numPointsPerAxis must be at least 2.",
                this);

            chunk.MarkRebuilt();
            return;
        }

        int expectedPointCount =
            pointsPerAxis *
            pointsPerAxis *
            pointsPerAxis;

        if (points.Length != expectedPointCount)
        {
            Debug.LogError(
                $"Chunk {chunk.name} has {points.Length} points, " +
                $"but {expectedPointCount} were expected.",
                chunk);

            chunk.MarkRebuilt();
            return;
        }

        int cubeCount =
            cubesPerAxis *
            cubesPerAxis *
            cubesPerAxis;

        NativeArray<Triangle> jobTriangleBuffer =
            new NativeArray<Triangle>(
                cubeCount * MaxTrianglesPerCube,
                Allocator.TempJob,
                NativeArrayOptions.UninitializedMemory);

        NativeArray<byte> triangleCounts =
            new NativeArray<byte>(
                cubeCount,
                Allocator.TempJob,
                NativeArrayOptions.ClearMemory);

        try
        {
            MarchingCubesJob marchJob = new MarchingCubesJob
            {
                points = points,

                triTable = triTableNative,
                cornerIndexAFromEdge = cornerAFromEdgeNative,
                cornerIndexBFromEdge = cornerBFromEdgeNative,

                triangleBuffer = jobTriangleBuffer,
                triangleCounts = triangleCounts,

                numPointsPerAxis = pointsPerAxis,
                surfaceLevel = worldSettings.surfaceLevel
            };

            JobHandle marchHandle = marchJob.Schedule(
                cubeCount,
                MarchBatchSize,
                chunk.GetPointDependency());

            chunk.SetPointDependency(marchHandle);

            // Unity Mesh objects must be updated on the main thread.
            chunk.CompletePointJobs();

            BuildMeshFromJobResults(
                chunk,
                jobTriangleBuffer,
                triangleCounts,
                cubeCount);
        }
        finally
        {
            if (jobTriangleBuffer.IsCreated)
                jobTriangleBuffer.Dispose();

            if (triangleCounts.IsCreated)
                triangleCounts.Dispose();

            chunk.MarkRebuilt();
        }
    }

    //void GenerateMesh(Chunk chunk)
    //{
    //    Vector3Int coord = chunk.GetCoords();
    //    Vector3 centre = CentreFromCoord(coord);

    //    // Put in build chunk
    //    float pointSpacing = worldSettings.boundsSize / (worldSettings.numPointsPerAxis - 1);
    //    densityGenerator.Generate(chunk.pointsBuffer, worldSettings.numPointsPerAxis, worldSettings.boundsSize, worldDimensions, Vector3.zero, centre, worldSettings.offset, pointSpacing);

    //    DispatchComputeShader(chunk);

    //    // Get number of triangles in the triangle buffer
    //    ComputeBuffer.CopyCount(triangleBuffer, triCountBuffer, 0);
    //    int[] triCountArray = { 0 };
    //    triCountBuffer.GetData(triCountArray);
    //    int numTris = triCountArray[0];

    //    // Get triangle data from shader
    //    Triangle[] tris = new Triangle[numTris];
    //    triangleBuffer.GetData(tris, 0, 0, numTris);
    //    var meshVertices = new Vector3[numTris * 3];
    //    // One triangle index list per material
    //    chunk.SetSubmeshAmount(materials.Length);

    //    for (int i = 0; i < numTris; i++)
    //    {
    //        int matIndex = Mathf.Clamp(tris[i].material, 0, materials.Length - 1);

    //        for (int j = 0; j < 3; j++)
    //        {
    //            int vertIndex = i * 3 + j;
    //            meshVertices[vertIndex] = tris[i][j];
    //            chunk.AddVertexIndexTo(matIndex, vertIndex);
    //        }
    //    }

    //    Mesh mesh = new Mesh();
    //    mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
    //    mesh.vertices = meshVertices;
    //    mesh.subMeshCount = materials.Length;

    //    for (int m = 0; m < materials.Length; m++)
    //    {
    //        mesh.SetTriangles(chunk.GetTris(m), m);
    //    }

    //    mesh.RecalculateNormals();
    //    chunk.SetMesh(mesh);
    //}

    private void BuildMeshFromJobResults(Chunk chunk, NativeArray<Triangle> triangleBuffer, NativeArray<byte> triangleCounts, int cubeCount)
    {
        int numTriangles = 0;

        for (int cubeIndex = 0; cubeIndex < cubeCount; cubeIndex++)
        {
            numTriangles += triangleCounts[cubeIndex];
        }

        Vector3[] vertices = new Vector3[numTriangles * 3];

        chunk.ClearTris();

        int outputTriangleIndex = 0;

        for (int cubeIndex = 0; cubeIndex < cubeCount; cubeIndex++)
        {
            int triangleCount = triangleCounts[cubeIndex];
            int triangleStart = cubeIndex * 5;

            for (int localTriangleIndex = 0; localTriangleIndex < triangleCount; localTriangleIndex++)
            {
                Triangle triangle =
                    triangleBuffer[
                        triangleStart + localTriangleIndex];

                int materialIndex = Mathf.Clamp(
                    triangle.material,
                    0,
                    materials.Length - 1);

                int vertexStart = outputTriangleIndex * 3;

                vertices[vertexStart] =
                    triangle.vertexA;

                vertices[vertexStart + 1] =
                    triangle.vertexB;

                vertices[vertexStart + 2] =
                    triangle.vertexC;

                chunk.AddVertexIndexTo(
                    materialIndex,
                    vertexStart);

                chunk.AddVertexIndexTo(
                    materialIndex,
                    vertexStart + 1);

                chunk.AddVertexIndexTo(
                    materialIndex,
                    vertexStart + 2);

                outputTriangleIndex++;
            }
        }

        Mesh mesh = chunk.GetMesh();

        mesh.Clear(false);
        mesh.SetVertices(vertices);
        mesh.subMeshCount = materials.Length;

        for (int materialIndex = 0;
             materialIndex < materials.Length;
             materialIndex++)
        {
            mesh.SetTriangles(
                chunk.GetTris(materialIndex),
                materialIndex,
                false);
        }

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        chunk.SetCollider();
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

    //public void EditSphere(ComputeShader computeEditting, Vector3 worldPos, float radius)
    //{
    //    // World-space AABB of the sphere
    //    Vector3 min = worldPos - Vector3.one * radius;
    //    Vector3 max = worldPos + Vector3.one * radius;

    //    // Convert to chunk coordinates
    //    Vector3Int minCoord = WorldToChunkCoord(min);
    //    Vector3Int maxCoord = WorldToChunkCoord(max);

    //    // Clamp to world bounds
    //    minCoord.x = Mathf.Clamp(minCoord.x, 0, (int)worldDimensions.x - 1);
    //    minCoord.y = Mathf.Clamp(minCoord.y, 0, (int)worldDimensions.y - 1);
    //    minCoord.z = Mathf.Clamp(minCoord.z, 0, (int)worldDimensions.z - 1);

    //    maxCoord.x = Mathf.Clamp(maxCoord.x, 0, (int)worldDimensions.x - 1);
    //    maxCoord.y = Mathf.Clamp(maxCoord.y, 0, (int)worldDimensions.y - 1);
    //    maxCoord.z = Mathf.Clamp(maxCoord.z, 0, (int)worldDimensions.z - 1);

    //    int numVoxelsPerAxis = worldSettings.numPointsPerAxis - 1;
    //    int numThreadsPerAxis = Mathf.CeilToInt(numVoxelsPerAxis / 8f);

    //    // Loop through all potentially affected chunks
    //    for (int x = minCoord.x; x <= maxCoord.x; x++)
    //    {
    //        for (int y = minCoord.y; y <= maxCoord.y; y++)
    //        {
    //            for (int z = minCoord.z; z <= maxCoord.z; z++)
    //            {
    //                Chunk chunk = GetChunkFromCoord(new Vector3Int(x, y, z));
    //                if (chunk == null)
    //                    continue;

    //                // This uses world-space positions stored in the buffer,
    //                // so we can pass the same worldPos & radius to every chunk.
    //                computeEditting.SetBuffer(0, "points", chunk.pointsBuffer);
    //                computeEditting.SetInt("numPointsPerAxis", worldSettings.numPointsPerAxis);

    //                // Ammo Buffer
    //                computeEditting.SetBuffer(0, "ammos", GunManager.ammoBuffer);

    //                computeEditting.Dispatch(0, numThreadsPerAxis, numThreadsPerAxis, numThreadsPerAxis);

    //                GunManager.UpdateAmmoCPU();
    //                chunk.HasChanged();
    //            }
    //        }
    //    }
    //}

    //public void EditChunk(ComputeShader computeEditting, Chunk chunk)
    //{
    //    if (chunk == null)
    //        return;

    //    int editKernel = computeEditting.FindKernel("CSMain");

    //    int numVoxelsPerAxis = worldSettings.numPointsPerAxis - 1;
    //    int numThreadsPerAxis = Mathf.CeilToInt(numVoxelsPerAxis / 8f);

    //    computeEditting.SetBuffer(editKernel, "points", chunk.pointsBuffer);
    //    computeEditting.SetInt("numPointsPerAxis", worldSettings.numPointsPerAxis);

    //    computeEditting.Dispatch(editKernel, numThreadsPerAxis, numThreadsPerAxis, numThreadsPerAxis);

    //    chunk.HasChanged();
    //}

    //public void EditTunnel(ComputeShader computeEditting, Vector3 cylinderStart, Vector3 cylinderEnd, float startRadius, float endRadius)
    //{
    //    float maxRadius = Mathf.Max(startRadius, endRadius);

    //    // World-space AABB of the rotated tapered cylinder
    //    Vector3 min = Vector3.Min(cylinderStart, cylinderEnd) - Vector3.one * maxRadius;
    //    Vector3 max = Vector3.Max(cylinderStart, cylinderEnd) + Vector3.one * maxRadius;

    //    // Convert to chunk coordinates
    //    Vector3Int minCoord = WorldToChunkCoord(min);
    //    Vector3Int maxCoord = WorldToChunkCoord(max);

    //    // Clamp to world bounds
    //    minCoord.x = Mathf.Clamp(minCoord.x, 0, (int)worldDimensions.x - 1);
    //    minCoord.y = Mathf.Clamp(minCoord.y, 0, (int)worldDimensions.y - 1);
    //    minCoord.z = Mathf.Clamp(minCoord.z, 0, (int)worldDimensions.z - 1);

    //    maxCoord.x = Mathf.Clamp(maxCoord.x, 0, (int)worldDimensions.x - 1);
    //    maxCoord.y = Mathf.Clamp(maxCoord.y, 0, (int)worldDimensions.y - 1);
    //    maxCoord.z = Mathf.Clamp(maxCoord.z, 0, (int)worldDimensions.z - 1);

    //    int numVoxelsPerAxis = worldSettings.numPointsPerAxis - 1;
    //    int numThreadsPerAxis = Mathf.CeilToInt(numVoxelsPerAxis / 8f);

    //    // Loop through all potentially affected chunks
    //    for (int x = minCoord.x; x <= maxCoord.x; x++)
    //    {
    //        for (int y = minCoord.y; y <= maxCoord.y; y++)
    //        {
    //            for (int z = minCoord.z; z <= maxCoord.z; z++)
    //            {
    //                Chunk chunk = GetChunkFromCoord(new Vector3Int(x, y, z));
    //                if (chunk == null)
    //                    continue;

    //                computeEditting.SetBuffer(0, "points", chunk.pointsBuffer);
    //                computeEditting.SetInt("numPointsPerAxis", worldSettings.numPointsPerAxis);

    //                computeEditting.SetVector("cylinderStart", cylinderStart);
    //                computeEditting.SetVector("cylinderEnd", cylinderEnd);
    //                computeEditting.SetFloat("startRadius", startRadius);
    //                computeEditting.SetFloat("endRadius", endRadius);

    //                computeEditting.Dispatch(kernel, numThreadsPerAxis, numThreadsPerAxis, numThreadsPerAxis);

    //                chunk.HasChanged();
    //            }
    //        }
    //    }
    //}

    private void CreateLookupTables()
    {
        triTableNative = MarchTables.CreateTriTable(Allocator.Persistent);

        cornerAFromEdgeNative = MarchTables.CreateCornerAFromEdge(Allocator.Persistent);
        cornerBFromEdgeNative = MarchTables.CreateCornerBFromEdge(Allocator.Persistent);
    }

    private void OnDestroy()
    {
        if (chunks != null)
        {
            foreach (Chunk chunk in chunks)
            {
                if (chunk != null)
                {
                    chunk.CompletePointJobs();
                    chunk.ReleaseBuffers();
                }
            }
        }

        if (triTableNative.IsCreated)
            triTableNative.Dispose();

        if (cornerAFromEdgeNative.IsCreated)
            cornerAFromEdgeNative.Dispose();

        if (cornerBFromEdgeNative.IsCreated)
            cornerBFromEdgeNative.Dispose();

        dirtyChunks?.Clear();
    }
}
