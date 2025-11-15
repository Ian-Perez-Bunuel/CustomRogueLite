using UnityEngine;

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

    // Pass the position and area you want to edit and it will change it's values
    public void EditData(Vector3 t_pos, float t_radius, bool t_breaking)
    {
        // Read all points
        Vector4[] data = new Vector4[numPoints];
        pointsBuffer.GetData(data);

        // Modify data[i].w (density)
        for (int i = 0; i < data.Length; i++)
        {
            Vector3 position = new Vector3(data[i].x, data[i].y, data[i].z);

            if (PointInsideSphere(position, t_pos, t_radius))
            {
                if (t_breaking)
                    data[i].w -= 0.1f;
                else
                    data[i].w += 0.1f;
            }
        }

        // Write back
        pointsBuffer.SetData(data);
        valuesChanged = true;
    }

    public static bool PointInsideSphere(Vector3 point, Vector3 sphereCenter, float radius)
    {
        // Compare squared distances (faster than using Vector3.Distance)
        return (point - sphereCenter).sqrMagnitude <= radius * radius;
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

    public void SetCollider() { meshCollider.sharedMesh = meshFilter.sharedMesh; }

    public void ReleaseBuffers()
    {
        if (pointsBuffer != null)
        {
            pointsBuffer.Release();
            pointsBuffer = null;
        }
    }
}
