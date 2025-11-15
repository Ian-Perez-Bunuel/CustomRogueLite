using UnityEngine;

public class Chunk : MonoBehaviour
{
    // Mesh
    MeshFilter meshFilter;
    MeshRenderer meshRenderer;
    MeshCollider meshCollider;

    Vector3Int coords;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Setup(Material mat, bool genColliders)
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
}
