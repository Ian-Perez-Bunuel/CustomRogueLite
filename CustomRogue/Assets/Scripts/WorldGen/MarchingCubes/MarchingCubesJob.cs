using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

public struct Triangle
{
    public float3 vertexC;
    public float3 vertexB;
    public float3 vertexA;

    public int material;
}

[BurstCompile]
public struct MarchingCubesJob : IJobParallelFor
{
    private const int MaxTrianglesPerCube = 5;

    [ReadOnly]
    public NativeArray<Point> points;

    // Flattened 256 x 16 triangle table.
    // Entry = triTable[cubeConfiguration * 16 + tableIndex]
    [ReadOnly]
    public NativeArray<int> triTable;

    [ReadOnly]
    public NativeArray<int> cornerIndexAFromEdge;

    [ReadOnly]
    public NativeArray<int> cornerIndexBFromEdge;

    public int numPointsPerAxis;
    public float surfaceLevel;

    /*
     * Each cube owns five consecutive output slots.
     *
     * cube 0: triangle slots 0-4
     * cube 1: triangle slots 5-9
     * cube 2: triangle slots 10-14
     */
    [WriteOnly]
    public NativeArray<Triangle> triangleBuffer;

    // Number of valid triangles written by each cube.
    [WriteOnly]
    public NativeArray<byte> triangleCounts;

    public void Execute(int cubeIndex)
    {
        int cubesPerAxis = numPointsPerAxis - 1;

        int x = cubeIndex % cubesPerAxis;
        int y = (cubeIndex / cubesPerAxis) % cubesPerAxis;
        int z = cubeIndex / (cubesPerAxis * cubesPerAxis);

        var cubeCorners = new FixedList512Bytes<Point>();

        cubeCorners.Add(points[IndexFromCoord(x, y, z)]);
        cubeCorners.Add(points[IndexFromCoord(x + 1, y, z)]);
        cubeCorners.Add(points[IndexFromCoord(x + 1, y, z + 1)]);
        cubeCorners.Add(points[IndexFromCoord(x, y, z + 1)]);

        cubeCorners.Add(points[IndexFromCoord(x, y + 1, z)]);
        cubeCorners.Add(points[IndexFromCoord(x + 1, y + 1, z)]);
        cubeCorners.Add(points[IndexFromCoord(x + 1, y + 1, z + 1)]);
        cubeCorners.Add(points[IndexFromCoord(x, y + 1, z + 1)]);

        int configuration = CalculateCubeConfiguration(cubeCorners);

        int triangleCount = 0;
        int outputStart = cubeIndex * MaxTrianglesPerCube;

        // A marching-cubes table has up to 15 edge entries:
        // five triangles, with three edges per triangle.
        for (int tableIndex = 0; tableIndex < 15; tableIndex += 3)
        {
            int edge0 = TriValue(configuration, tableIndex);

            if (edge0 == -1)
                break;

            int edge1 = TriValue(configuration, tableIndex + 1);
            int edge2 = TriValue(configuration, tableIndex + 2);

            int a0 = cornerIndexAFromEdge[edge0];
            int b0 = cornerIndexBFromEdge[edge0];

            int a1 = cornerIndexAFromEdge[edge1];
            int b1 = cornerIndexBFromEdge[edge1];

            int a2 = cornerIndexAFromEdge[edge2];
            int b2 = cornerIndexBFromEdge[edge2];

            triangleBuffer[outputStart + triangleCount] = new Triangle
            {
                vertexA = InterpolateVerts(cubeCorners[a0], cubeCorners[b0]),
                vertexB = InterpolateVerts(cubeCorners[a1], cubeCorners[b1]),
                vertexC = InterpolateVerts(cubeCorners[a2], cubeCorners[b2]),

                // Same choice as the compute shader.
                material = cubeCorners[0].material
            };

            triangleCount++;
        }

        triangleCounts[cubeIndex] = (byte)triangleCount;
    }

    private int IndexFromCoord(int x, int y, int z)
    {
        return z * numPointsPerAxis * numPointsPerAxis
             + y * numPointsPerAxis
             + x;
    }

    private int TriValue(int cubeConfiguration, int tableIndex)
    {
        return triTable[cubeConfiguration * 16 + tableIndex];
    }

    private int CalculateCubeConfiguration(
        FixedList512Bytes<Point> cubeCorners)
    {
        int configuration = 0;

        if (cubeCorners[0].density < surfaceLevel)
            configuration |= 1;

        if (cubeCorners[1].density < surfaceLevel)
            configuration |= 2;

        if (cubeCorners[2].density < surfaceLevel)
            configuration |= 4;

        if (cubeCorners[3].density < surfaceLevel)
            configuration |= 8;

        if (cubeCorners[4].density < surfaceLevel)
            configuration |= 16;

        if (cubeCorners[5].density < surfaceLevel)
            configuration |= 32;

        if (cubeCorners[6].density < surfaceLevel)
            configuration |= 64;

        if (cubeCorners[7].density < surfaceLevel)
            configuration |= 128;

        return configuration;
    }

    private float3 InterpolateVerts(Point v1, Point v2)
    {
        float densityDifference = v2.density - v1.density;

        // Prevent NaN if both points have effectively equal density.
        if (math.abs(densityDifference) < 0.000001f)
            return (v1.position + v2.position) * 0.5f;

        float t = (surfaceLevel - v1.density) / densityDifference;

        return math.lerp(v1.position, v2.position, t);
    }
}