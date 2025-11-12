using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

// Position equality with tolerance (for interpolated floats)
class Vec3Approx : IEqualityComparer<Vector3>
{
    readonly float eps;
    public Vec3Approx(float epsilon = 1e-4f) { eps = epsilon; }

    public bool Equals(Vector3 a, Vector3 b) =>
        Mathf.Abs(a.x - b.x) < eps &&
        Mathf.Abs(a.y - b.y) < eps &&
        Mathf.Abs(a.z - b.z) < eps;

    public int GetHashCode(Vector3 v)
    {
        // Quantize to a grid of size eps to get a stable hash
        int qx = Mathf.RoundToInt(v.x / eps);
        int qy = Mathf.RoundToInt(v.y / eps);
        int qz = Mathf.RoundToInt(v.z / eps);
        int h = 17;
        h = h * 31 + qx;
        h = h * 31 + qy;
        h = h * 31 + qz;
        return h;
    }
}

public class ChunkMarching : MonoBehaviour
{
    public MarchingCube marchingCube;

    // UI
    public Slider surfaceLevelSlider;
    public Slider amplitudeSlider;
    public Slider frequencySlider;
    public Slider persistenceSlider;
    public Slider octaveSlider;
    public Slider seedSlider;

    // Mesh
    MeshFilter meshFilter;
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();

    // cache to avoid duplicate vertices (+ approx comparer for float safety)
    Dictionary<Vector3, int> vertexToIndex;

    // Reuse/create an index for a vertex position
    int GetVertexIndex(Vector3 v)
    {
        if (vertexToIndex.TryGetValue(v, out int idx))
            return idx;

        idx = vertices.Count;
        vertices.Add(v);
        vertexToIndex[v] = idx;
        return idx;
    }

    // Amount of boxes per axis
    [Header("Space")]
    public Vector3Int dimensions;
    public float surfaceLevel;
    private float[,,] densities;

    [Header("Noise Parameters")]
    public float frequency;
    public float amplitude;
    public float persistence;
    public int octave;
    public int seed;

    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();

        // Set Position so it's visible to the camera
        transform.position = new Vector3(-dimensions.x / 2f, -dimensions.y / 2f, 0);

        densities = new float[dimensions.x + 1, dimensions.y + 1, dimensions.z + 1];
        SetDensities();

        // Set UI to default
        surfaceLevelSlider.value = surfaceLevel;
        amplitudeSlider.value = amplitude;
        frequencySlider.value = frequency;
        persistenceSlider.value = persistence;
        octaveSlider.value = octave;
        seedSlider.value = seed;
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            BuildWorld();
        }
        
        if (Input.GetKey(KeyCode.E))
        {
            for (int x = 0; x < dimensions.x + 1; x++)
            {
                for (int y = 0; y < dimensions.y + 1; y++)
                {
                    for (int z = 0; z < dimensions.z + 1; z++)
                    {
                        densities[x, y, z] += 0.01f;
                    }
                }
            }

            BuildWorld();
        }

        if (Input.GetKey(KeyCode.Q))
        {
            for (int x = 0; x < dimensions.x + 1; x++)
            {
                for (int y = 0; y < dimensions.y + 1; y++)
                {
                    for (int z = 0; z < dimensions.z + 1; z++)
                    {
                        densities[x, y, z] -= 0.01f;
                    }
                }
            }

            BuildWorld();
        }
    }

    void ClearMeshData()
    {
        vertices.Clear();
        triangles.Clear();
        vertexToIndex = new Dictionary<Vector3, int>(new Vec3Approx(1e-4f));
    }

    void BuildMesh()
    {
        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; // optional if you may exceed 65k verts
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;
    }

    private void SetDensities()
    {
        for (int x = 0; x < dimensions.x + 1; x++) 
        {
            for (int y = 0; y < dimensions.y + 1; y++)
            {
                for (int z = 0; z < dimensions.z + 1; z++)
                {
                    densities[x, y, z] = PerlinNoise3D(new Vector3(x, y, z));
                }
            }
        }
    }

    private float GetDensity(int x, int y, int z)
    {
        return densities[x, y, z];
    }
    private float GetDensity(Vector3 pos)
    {
        return densities[(int)pos.x, (int)pos.y, (int)pos.z];
    }

    public void BuildWorld()
    {
        ClearMeshData();

        // March cube through grid
        for (int x = 0; x < dimensions.x; x++)
        {
            for (int y = 0; y < dimensions.y; y++)
            {
                for (int z = 0; z < dimensions.z; z++)
                {
                    Vector3 step = new Vector3(x, y, z);

                    // Sample noise at the 8 cube corners
                    for (int i = 0; i < 8; i++)
                    {
                        Vector3 cornerPos = step + (Vector3)marchingCube.CornerTable[i];
                        marchingCube.SetCornerValue(i, GetDensity(cornerPos), cornerPos);
                    }

                    // Pass the index cache and index resolver
                    marchingCube.MarchCube(triangles, surfaceLevel, step, getVertexIndex: GetVertexIndex);
                }
            }
        }

        BuildMesh();
    }

    public void UpdateValues()
    {
        surfaceLevel = surfaceLevelSlider.value;
        frequency = frequencySlider.value;
        amplitude = amplitudeSlider.value;
        persistence = persistenceSlider.value;
        octave = (int)octaveSlider.value;
        seed = (int)seedSlider.value;
    }

    // --- simple hash so each seed shifts the noise domain ---
    static float HashFloat(int seed)
    {
        // make sure same seed always gives same offset
        System.Random r = new System.Random(seed);
        return (float)r.NextDouble() * 10000f;
    }

    float PerlinNoise3D(Vector3 pos)
    {
        // Helper
        float x = pos.x;
        float y = pos.y; 
        float z = pos.z;

        // shift domain by seed
        float shift = HashFloat(seed);
        x += shift;
        y += shift;
        z += shift;

        float total = 0f;
        float freq = frequency;
        float amp = amplitude;

        for (int i = 0; i < octave; i++)
        {
            // sample 3D by blending 2D slices
            float ab = Mathf.PerlinNoise(x * freq, y * freq);
            float bc = Mathf.PerlinNoise(y * freq, z * freq);
            float ac = Mathf.PerlinNoise(x * freq, z * freq);

            float ba = Mathf.PerlinNoise(y * freq, x * freq);
            float cb = Mathf.PerlinNoise(z * freq, y * freq);
            float ca = Mathf.PerlinNoise(z * freq, x * freq);

            float value = (ab + bc + ac + ba + cb + ca) / 6f; // average

            total += value * amp;

            // next octave
            freq *= 2f;
            amp *= persistence;
        }

        return total;
    }
}
